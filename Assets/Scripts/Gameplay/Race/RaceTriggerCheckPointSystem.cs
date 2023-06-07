using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Evaluates and trigger events for Physics 
    /// collisions between the player and the checkpoint.
    /// </summary>
    [BurstCompile]
    public struct TriggerCheckpointJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<CheckPoint> CheckPointLookup;
        public ComponentLookup<LapProgress> LapProgressLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;

        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;

            var isBodyATrigger = CheckPointLookup.HasComponent(entityA);
            var isBodyBTrigger = CheckPointLookup.HasComponent(entityB);

            // Ignoring Triggers overlapping other Triggers
            if (isBodyATrigger && isBodyBTrigger)
            {
                return;
            }

            var isBodyADynamic = LapProgressLookup.HasComponent(entityA);
            var isBodyBDynamic = LapProgressLookup.HasComponent(entityB);

            // Ignoring overlapping static bodies
            if ((isBodyATrigger && !isBodyBDynamic) ||
                (isBodyBTrigger && !isBodyADynamic))
            {
                return;
            }

            var triggerEntity = isBodyATrigger ? entityA : entityB;
            var dynamicEntity = isBodyATrigger ? entityB : entityA;

            // Update Lap Progress
            var triggerCheckPoint = CheckPointLookup[triggerEntity];
            var lapProgress = LapProgressLookup.GetRefRW(dynamicEntity);
            var currentCheckPointId = triggerCheckPoint.Id;

            if (lapProgress.ValueRO.NextPointId == currentCheckPointId)
            {
                lapProgress.ValueRW.CurrentCheckPoint = currentCheckPointId;
                lapProgress.ValueRW.LastCheckPointPosition = LocalTransformLookup[dynamicEntity].Position;
            }
        }
    }

    /// <summary>
    /// Runs the job to evaluate the player and checkpoint collision
    /// </summary>
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct UpdateTriggerCheckPoint : ISystem
    {
        private ComponentDataHandles m_Handles;

        private struct ComponentDataHandles
        {
            public ComponentLookup<CheckPoint> CheckPointLookup;
            public ComponentLookup<LapProgress> LapProgressLookup;
            public ComponentLookup<LocalTransform> LocalTransformLookup;

            public ComponentDataHandles(ref SystemState state)
            {
                CheckPointLookup = state.GetComponentLookup<CheckPoint>(true);
                LapProgressLookup = state.GetComponentLookup<LapProgress>();
                LocalTransformLookup = state.GetComponentLookup<LocalTransform>(true);
            }

            public void Update(ref SystemState state)
            {
                CheckPointLookup.Update(ref state);
                LapProgressLookup.Update(ref state);
                LocalTransformLookup.Update(ref state);
            }
        }

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CheckPoint>();
            m_Handles = new ComponentDataHandles(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            m_Handles.Update(ref state);
            state.Dependency = new TriggerCheckpointJob
            {
                CheckPointLookup = m_Handles.CheckPointLookup,
                LapProgressLookup = m_Handles.LapProgressLookup,
                LocalTransformLookup = m_Handles.LocalTransformLookup
            }.Schedule(simulationSingleton, state.Dependency);
        }
    }
}