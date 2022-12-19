using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Mathematics;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Prevents the car to rolling without input.
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct AntiRollBarJob : IJobEntity
    {
        public PhysicsWorld PhysicsWorld;
        [ReadOnly] public ComponentLookup<Suspension> SuspensionFromEntity;
        [ReadOnly] public ComponentLookup<WheelHitData> WheelHitFromEntity;

        private void Execute(Entity entity, in LocalTransform localTrans, in AntiRollBar antiRollBar)
        {
            var ceIdx = PhysicsWorld.GetRigidBodyIndex(entity);
            var flSuspension = SuspensionFromEntity[antiRollBar.FrontLeftWheel];
            var frSuspension = SuspensionFromEntity[antiRollBar.FrontRightWheel];
            var rlSuspension = SuspensionFromEntity[antiRollBar.RearLeftWheel];
            var rrSuspension = SuspensionFromEntity[antiRollBar.RearRightWheel];

            var localUp = math.mul(localTrans.Rotation, new float3(0,1,0));

            var flHit = WheelHitFromEntity[antiRollBar.FrontLeftWheel];
            var frHit = WheelHitFromEntity[antiRollBar.FrontRightWheel];
            var rlHit = WheelHitFromEntity[antiRollBar.RearLeftWheel];
            var rrHit = WheelHitFromEntity[antiRollBar.RearRightWheel];

            var frontLengthDiff = 0.0f;
            var rearLengthDiff = 0.0f;

            if (flHit.HasHit || frHit.HasHit)
            {
                frontLengthDiff = (flSuspension.SpringLength - frSuspension.SpringLength) / flSuspension.RestLength;
            }

            if (rlHit.HasHit || rrHit.HasHit)
            {
                rearLengthDiff = (rlSuspension.SpringLength - rrSuspension.SpringLength) / rlSuspension.RestLength;
            }

            var frontForce = frontLengthDiff * antiRollBar.Stiffness;
            var rearForce = rearLengthDiff * antiRollBar.Stiffness;

            //Front wheels anti roll forces
            PhysicsWorld.ApplyImpulse(ceIdx, -localUp * frontForce, flHit.WheelCenter);
            PhysicsWorld.ApplyImpulse(ceIdx, localUp * frontForce, frHit.WheelCenter);

            //Rear wheels anti roll forces
            PhysicsWorld.ApplyImpulse(ceIdx, -localUp * rearForce, rlHit.WheelCenter);
            PhysicsWorld.ApplyImpulse(ceIdx, localUp * rearForce, rrHit.WheelCenter);
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(PhysicsSimulationGroup))]
    [UpdateAfter(typeof(WheelsForcesSystem))]
    public partial struct AntiRollBarSystem : ISystem
    {
        private ComponentLookup<Suspension> SuspensionFromEntity;
        private ComponentLookup<WheelHitData> WheelHitFromEntity;

        public void OnCreate(ref SystemState state)
        {
            SuspensionFromEntity = state.GetComponentLookup<Suspension>(true);
            WheelHitFromEntity = state.GetComponentLookup<WheelHitData>(true);
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            SuspensionFromEntity.Update(ref state);
            WheelHitFromEntity.Update(ref state);
            var physicsWorld = GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
            var antiRollBarJob = new AntiRollBarJob
            {
                PhysicsWorld = physicsWorld,
                SuspensionFromEntity = SuspensionFromEntity,
                WheelHitFromEntity = WheelHitFromEntity
            };
            state.Dependency = antiRollBarJob.Schedule(state.Dependency);
            //state.Dependency.Complete();
        }
    }
}