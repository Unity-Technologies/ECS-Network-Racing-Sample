using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct AntiRollBarJob : IJobEntity
    {
        public PhysicsWorld PhysicsWorld;
        [ReadOnly] public ComponentLookup<Suspension> SuspensionFromEntity;
        [ReadOnly] public ComponentLookup<WheelHitData> WheelHitFromEntity;
        [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldFromEntity;

        void Execute(Entity entity, in AntiRollBar antiRollBar)
        {
            var ceIdx = PhysicsWorld.GetRigidBodyIndex(entity);
            var flSuspension = SuspensionFromEntity[antiRollBar.FrontLeftWheel];
            var frSuspension = SuspensionFromEntity[antiRollBar.FrontRightWheel];
            var rlSuspension = SuspensionFromEntity[antiRollBar.RearLeftWheel];
            var rrSuspension = SuspensionFromEntity[antiRollBar.RearRightWheel];

            var flLocalToWorld = LocalToWorldFromEntity[antiRollBar.FrontLeftWheel];
            var frLocalToWorld = LocalToWorldFromEntity[antiRollBar.FrontRightWheel];
            var rlLocalToWorld = LocalToWorldFromEntity[antiRollBar.RearLeftWheel];
            var rrLocalToWorld = LocalToWorldFromEntity[antiRollBar.RearRightWheel];

            var flHit = WheelHitFromEntity[antiRollBar.FrontLeftWheel];
            var frHit = WheelHitFromEntity[antiRollBar.FrontRightWheel];
            var rlHit = WheelHitFromEntity[antiRollBar.RearLeftWheel];
            var rrHit = WheelHitFromEntity[antiRollBar.RearRightWheel];

            var frontLengthDiff = 0.0f;
            var rearLengthDiff = 0.0f;

            if (flHit.HasHit || frHit.HasHit)
                frontLengthDiff = (flSuspension.SpringLength - frSuspension.SpringLength) / flSuspension.RestLength;

            if (rlHit.HasHit || rrHit.HasHit)
                rearLengthDiff = (rlSuspension.SpringLength - rrSuspension.SpringLength) / rlSuspension.RestLength;

            var frontForce = frontLengthDiff * antiRollBar.Stiffness;
            var rearForce = rearLengthDiff * antiRollBar.Stiffness;

            //Front wheels anti roll forces
            PhysicsWorld.ApplyImpulse(ceIdx, -flLocalToWorld.Up * frontForce, flHit.WheelCenter);
            PhysicsWorld.ApplyImpulse(ceIdx, frLocalToWorld.Up * frontForce, frHit.WheelCenter);
            
            //Rear wheels anti roll forces
            PhysicsWorld.ApplyImpulse(ceIdx, -rlLocalToWorld.Up * rearForce, rlHit.WheelCenter);
            PhysicsWorld.ApplyImpulse(ceIdx, rrLocalToWorld.Up * rearForce, rrHit.WheelCenter);
        }
    }
    
    [BurstCompile]
    [UpdateInGroup(typeof(PhysicsSimulationGroup))]
    public partial struct AntiRollBarSystem : ISystem
    {
        ComponentLookup<Suspension> SuspensionFromEntity;
        ComponentLookup<LocalToWorld> LocalToWorldFromEntity;
        ComponentLookup<WheelHitData> WheelHitFromEntity;
        
        public void OnCreate(ref SystemState state)
        {
            SuspensionFromEntity = state.GetComponentLookup<Suspension>(true);
            LocalToWorldFromEntity = state.GetComponentLookup<LocalToWorld>(true);
            WheelHitFromEntity  = state.GetComponentLookup<WheelHitData>(true);
        }
        
        public void OnDestroy(ref SystemState state)
        {
        }
        
        public void OnUpdate(ref SystemState state)
        {
            SuspensionFromEntity.Update(ref state);
            LocalToWorldFromEntity.Update(ref state);
            WheelHitFromEntity.Update(ref state);
            var physicsWorld = GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
            var antiRollBarJob = new AntiRollBarJob
            {
                PhysicsWorld = physicsWorld,
                SuspensionFromEntity = SuspensionFromEntity,
                LocalToWorldFromEntity = LocalToWorldFromEntity,
                WheelHitFromEntity = WheelHitFromEntity,
            };
            // TODO: Add it to the forces system
            state.Dependency.Complete();
        }
    }
}