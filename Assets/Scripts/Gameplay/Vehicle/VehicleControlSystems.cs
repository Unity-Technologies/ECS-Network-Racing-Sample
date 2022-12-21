using Unity.Burst;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Collections;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    ///     Calculate and apply suspension forces to the vehicle wheels
    ///     https://en.wikipedia.org/wiki/Car_suspension
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct WheelRaycastJob : IJobEntity
    {
        public PhysicsWorld PhysicsWorld;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;

        private void Execute(Entity entity, in ChassisReference chassisReference, in Wheel wheel,
            in Suspension suspension, ref WheelHitData wheelHitData)
        {
            var wheelTransform = LocalTransformLookup[entity];
            var chassisTransform = LocalTransformLookup[chassisReference.Value];
            var localPosition = math.mul(chassisTransform.Rotation, wheelTransform.Position) + chassisTransform.Position;
            var localUp = math.mul(chassisTransform.Rotation, new float3(0,1,0));
            var ceIdx = PhysicsWorld.GetRigidBodyIndex(chassisReference.Value);
            var filter = PhysicsWorld.GetCollisionFilter(ceIdx);
            filter.CollidesWith = (uint)wheel.CollisionMask;
            var start = localPosition;
            var end = start - localUp * (suspension.RestLength + wheel.Radius);
            var input = new RaycastInput
            {
                Start = start,
                End = end,
                Filter = filter
            };
            wheelHitData.WheelCenter = localPosition - localUp * suspension.SpringLength;
            wheelHitData.Velocity = PhysicsWorld.GetLinearVelocity(ceIdx, wheelHitData.WheelCenter);
            wheelHitData.HasHit = false;
            if (math.length(wheelHitData.Velocity) > 50)
            {
                wheelHitData.Reset();
                return;
            }

            if (PhysicsWorld.CollisionWorld.CastRay(input, out var result))
            {
                wheelHitData.Origin = localPosition;
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
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;

        private void Execute(Entity entity, in ChassisReference chassisReference,
            in WheelHitData wheelHitData,
            in Suspension suspension, in Wheel wheel)
        {
            var ceIdx = PhysicsWorld.GetRigidBodyIndex(chassisReference.Value);

            if (!wheelHitData.HasHit)
            {
                return;
            }
            var wheelTransform = LocalTransformLookup[entity];
            var chassisTransform = LocalTransformLookup[chassisReference.Value];
            var localUp = math.mul(chassisTransform.Rotation, new float3(0,1,0));
            var localForward = math.mul(chassisTransform.Rotation, math.mul(wheelTransform.Rotation, new float3(0,0,1)));
            var localRight = math.mul(chassisTransform.Rotation, math.mul(wheelTransform.Rotation, new float3(1,0,0)));

            PhysicsWorld.ApplyImpulse(ceIdx, suspension.SuspensionForce * localUp, wheelHitData.WheelCenter);
            PhysicsWorld.ApplyImpulse(ceIdx, wheel.DriveForce * localForward, wheelHitData.WheelCenter);
            PhysicsWorld.ApplyImpulse(ceIdx, wheel.SidewaysForce * localRight, wheelHitData.WheelCenter);
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct SuspensionForceJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        private void Execute(in ChassisReference chassisReference,
            in WheelHitData wheelHitData,
            in Wheel wheel, ref Suspension suspension)
        {
            suspension.SpringLength = math.distance(wheelHitData.Position, wheelHitData.Origin) - wheel.Radius;

            var localTransform = LocalTransformLookup[chassisReference.Value];
            var localUp = math.mul(localTransform.Rotation, new float3(0,1,0));
            var springVelocity = math.dot(localUp, wheelHitData.Velocity);
            springVelocity =
                math.clamp(10, -10,
                    springVelocity); // Clamp the spring force TODO: maybe expose that as authoring component
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
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        private void Execute(Entity entity, in ChassisReference chassisReference, in WheelHitData wheelHitData,
            in WheelDriveControls wheelDriveControls, ref Wheel wheel, in Suspension suspension)
        {
            if (wheelDriveControls.DriveAmount == 0)
            {
                wheel.DriveForce = 0;
                return;
            }
            var wheelTransform = LocalTransformLookup[entity];
            var chassisTransform = LocalTransformLookup[chassisReference.Value];
            var localForward = math.mul(chassisTransform.Rotation, math.mul(wheelTransform.Rotation, new float3(0,0,1)));
            var speed = math.abs(math.dot(localForward, wheelHitData.Velocity));
            var speedNormalized = math.clamp(speed / wheel.DriveTorque, 0, 1);
            var torque = wheel.DriveTorqueCurve.Value.Evaluate(speedNormalized) * wheelDriveControls.DriveAmount *
                         wheel.MaxDriveTorque;
            wheel.DriveForce = torque;
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct SidewaysForceJob : IJobEntity
    {
        public float DeltaTime;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;

        private void Execute(Entity entity, in ChassisReference chassisReference, in WheelHitData wheelHitData, ref Wheel wheel)
        {
            if (!wheelHitData.HasHit)
            {
                wheel.SidewaysForce = 0;
                return;
            }

            var wheelTransform = LocalTransformLookup[entity];
            var chassisTransform = LocalTransformLookup[chassisReference.Value];
            var localRight = math.mul(chassisTransform.Rotation, math.mul(wheelTransform.Rotation, new float3(1,0,0)));
            var steeringVelocity = math.dot(localRight, wheelHitData.Velocity);
            var steeringDelta = -steeringVelocity * wheelHitData.SurfaceFriction * wheel.GripFactor;
            wheel.SidewaysForce = steeringDelta / DeltaTime;
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(PhysicsSimulationGroup))]
    [UpdateBefore(typeof(WheelsForcesSystem))]
    [UpdateBefore(typeof(WheelForcesApplySystem))]
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
                PhysicsWorld = physicsWorld,
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true)
            };
            state.Dependency = wheelRaycastJob.Schedule(state.Dependency);
            //state.CompleteDependency();
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
            var localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            var suspensionForceJob = new SuspensionForceJob()
            {
                LocalTransformLookup = localTransformLookup
            };
            state.Dependency = suspensionForceJob.ScheduleParallel(state.Dependency);
            var driveForceJob = new DriveForceJob()
            {
                LocalTransformLookup = localTransformLookup
            };
            state.Dependency = driveForceJob.ScheduleParallel(state.Dependency);
            var sidewaysForceJob = new SidewaysForceJob
            {
                DeltaTime = state.WorldUnmanaged.Time.fixedDeltaTime,
                LocalTransformLookup = localTransformLookup
            };
            state.Dependency = sidewaysForceJob.ScheduleParallel(state.Dependency);
            //state.CompleteDependency();
        }
    }


    [BurstCompile]
    [UpdateInGroup(typeof(PhysicsSimulationGroup))]
    [UpdateAfter(typeof(WheelsForcesSystem))]
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
            var physicsWorld = GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
            var wheelForcesJob = new WheelForcesJob
            {
                PhysicsWorld = physicsWorld,
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true)
            };
            state.Dependency = wheelForcesJob.Schedule(state.Dependency);
        }
    }
}