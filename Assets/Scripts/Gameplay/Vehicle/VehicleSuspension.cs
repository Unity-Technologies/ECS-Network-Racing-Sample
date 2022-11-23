using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    /// <summary>
    /// Calculate and apply suspension forces to the vehicle wheels
    /// https://en.wikipedia.org/wiki/Car_suspension
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct WheelRaycastJob : IJobEntity
    {
        public PhysicsWorld PhysicsWorld;

        void Execute(in ChassisReference chassisReference, in LocalToWorld localToWorld, in Wheel wheel,
            in Suspension suspension, ref WheelHitData wheelHitData)
        {
            var ceIdx = PhysicsWorld.GetRigidBodyIndex(chassisReference.Value);
            var filter = PhysicsWorld.GetCollisionFilter(ceIdx);
            filter.CollidesWith = (uint)wheel.CollisionMask;
            var start = localToWorld.Position;
            var end = start - localToWorld.Up * (suspension.RestLength + wheel.Radius);
            var input = new RaycastInput
            {
                Start = start,
                End = end,
                Filter = filter,
            };
            wheelHitData.WheelCenter = localToWorld.Position - localToWorld.Up * suspension.SpringLength;
            wheelHitData.Velocity = PhysicsWorld.GetLinearVelocity(ceIdx, wheelHitData.WheelCenter);
            wheelHitData.HasHit = false;
            if (math.length(wheelHitData.Velocity) > 50)
            {
                wheelHitData.Reset();
                return;
            }
             
            if (PhysicsWorld.CollisionWorld.CastRay(input, out var result))
            {
                wheelHitData.Origin = localToWorld.Position;
                wheelHitData.HasHit = true;
                wheelHitData.Position = result.Position;
                wheelHitData.SurfaceFriction = result.Material.Friction;
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct WheelForcesJob : IJobEntity
    {
        public PhysicsWorld PhysicsWorld;
        void Execute(in ChassisReference chassisReference, in LocalToWorld localToWorld, in WheelHitData wheelHitData,
            in Suspension suspension, in Wheel wheel)
        {
            var ceIdx = PhysicsWorld.GetRigidBodyIndex(chassisReference.Value);

            if (!wheelHitData.HasHit)
                return;
            PhysicsWorld.ApplyImpulse(ceIdx, suspension.SuspensionForce * localToWorld.Up, wheelHitData.WheelCenter);
            PhysicsWorld.ApplyImpulse(ceIdx, wheel.DriveForce * localToWorld.Forward, wheelHitData.WheelCenter);
            PhysicsWorld.ApplyImpulse(ceIdx, wheel.SidewaysForce * localToWorld.Right, wheelHitData.WheelCenter);
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct SuspensionForceJob : IJobEntity
    {
        void Execute(in ChassisReference chassisReference, in LocalToWorld localToWorld, in WheelHitData wheelHitData,
            in Wheel wheel, ref Suspension suspension)
        {
            suspension.SpringLength = math.distance(wheelHitData.Position, wheelHitData.Origin) - wheel.Radius;

            var springVelocity = math.dot(localToWorld.Up, wheelHitData.Velocity);
            springVelocity = math.clamp(10, -10, springVelocity); // Clamp the spring force TODO: maybe expose that as authoring component
            suspension.SpringForce = (suspension.RestLength - suspension.SpringLength) * suspension.SpringStiffness;
            suspension.DamperForce = springVelocity * suspension.DamperStiffness;
            var totalForce = suspension.SpringForce - suspension.DamperForce;
            totalForce = math.max(0, totalForce);
            suspension.SuspensionForce = totalForce;
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct DriveForceJob : IJobEntity
    {
        void Execute(in LocalToWorld localToWorld, in WheelHitData wheelHitData,
            in WheelDriveControls wheelDriveControls, ref Wheel wheel, in Suspension suspension)
        {
            if (wheelDriveControls.DriveAmount == 0)
                return;
            var speed = math.abs(math.dot(localToWorld.Forward, wheelHitData.Velocity));
            var speedNormalized = math.clamp(speed / wheel.DriveTorque, 0, 1);
            var torque = wheel.DriveTorqueCurve.Value.Evaluate(speedNormalized) * wheelDriveControls.DriveAmount * wheel.MaxDriveTorque;
            wheel.DriveForce = torque;
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct SidewaysForceJob : IJobEntity
    {
        public float DeltaTime;

        void Execute(in LocalToWorld localToWorld, in WheelHitData wheelHitData, ref Wheel wheel)
        {
            if (!wheelHitData.HasHit)
            {
                wheel.SidewaysForce = 0;
                return;
            }
            
            var steeringVelocity = math.dot(localToWorld.Right, wheelHitData.Velocity);
            var steeringDelta = -steeringVelocity * wheelHitData.SurfaceFriction * wheel.GripFactor;
            wheel.SidewaysForce = steeringDelta / DeltaTime;
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(PhysicsSimulationGroup))]
    [UpdateAfter(typeof(WheelsForcesSystem))]
    public partial struct WheelRaycastSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
            var wheelRaycastJob = new WheelRaycastJob
            {
                PhysicsWorld = physicsWorld
            };
            state.Dependency = wheelRaycastJob.Schedule(state.Dependency);
            state.CompleteDependency();
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(PhysicsSimulationGroup))]
    public partial struct WheelsForcesSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var suspensionForceJob = new SuspensionForceJob();
            state.Dependency = suspensionForceJob.ScheduleParallel(state.Dependency);
            var driveForceJob = new DriveForceJob();
            state.Dependency = driveForceJob.ScheduleParallel(state.Dependency);
            var sidewaysForceJob = new SidewaysForceJob
            {
                DeltaTime = state.WorldUnmanaged.Time.fixedDeltaTime
            };
            state.Dependency = sidewaysForceJob.ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }
    }


    [BurstCompile]
    [UpdateInGroup(typeof(PhysicsSimulationGroup))]
    [UpdateBefore(typeof(WheelsForcesSystem))]
    public partial struct WheelForcesApplySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
            var wheelForcesJob = new WheelForcesJob
            {
                PhysicsWorld = physicsWorld
            };
            state.Dependency = wheelForcesJob.Schedule(state.Dependency);
        }
    }
}