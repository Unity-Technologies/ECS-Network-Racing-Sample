using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Dots.Racing
{
    [BurstCompile]
    public struct TriggerCheckpointJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<CheckPoint> CheckPointLookup;
        public ComponentLookup<LapProgress> LapProgressLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        public int Count;

        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;

            var isBodyATrigger = CheckPointLookup.HasComponent(entityA);
            var isBodyBTrigger = CheckPointLookup.HasComponent(entityB);

            // Ignoring Triggers overlapping other Triggers
            if (isBodyATrigger && isBodyBTrigger)
                return;

            var isBodyADynamic = LapProgressLookup.HasComponent(entityA);
            var isBodyBDynamic = LapProgressLookup.HasComponent(entityB);

            // Ignoring overlapping static bodies
            if ((isBodyATrigger && !isBodyBDynamic) ||
                (isBodyBTrigger && !isBodyADynamic))
                return;

            var triggerEntity = isBodyATrigger ? entityA : entityB;
            var dynamicEntity = isBodyATrigger ? entityB : entityA;

            // Update Lap Progress
            var triggerCheckPoint = CheckPointLookup[triggerEntity];
            var lapProgress = LapProgressLookup.GetRefRW(dynamicEntity, false);
            var currentCheckPointId = triggerCheckPoint.Id;

            if (lapProgress.ValueRO.NextPointId == currentCheckPointId)
            {
                lapProgress.ValueRW.CurrentCheckPoint = currentCheckPointId;
                lapProgress.ValueRW.LastCheckPointPosition = LocalTransformLookup[dynamicEntity].Position;
            }

            if (lapProgress.ValueRO.CurrentCheckPoint == Count)
            {
                lapProgress.ValueRW.CurrentLap++;
                if (lapProgress.ValueRO.CurrentLap < lapProgress.ValueRO.LapCount)
                {
                    // Finish Lap
                    lapProgress.ValueRW.CurrentCheckPoint = 0;
                }
                else
                {
                    // Finish the race
                    lapProgress.ValueRW.Finished = true;
                }
            }
        }
    }

    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct UpdateTriggerCheckPoint : ISystem
    {
        private ComponentDataHandles m_Handles;
        private EntityQuery m_CheckPointQuery;

        private struct ComponentDataHandles
        {
            public ComponentLookup<CheckPoint> CheckPointLookup;
            public ComponentLookup<LapProgress> LapProgressLookup;
            public ComponentLookup<LocalTransform> LocalTransformLookup;

            public ComponentDataHandles(ref SystemState state)
            {
                CheckPointLookup = state.GetComponentLookup<CheckPoint>(true);
                LapProgressLookup = state.GetComponentLookup<LapProgress>(false);
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
            state.RequireForUpdate(state.GetEntityQuery(ComponentType.ReadOnly<CheckPoint>()));
            m_Handles = new ComponentDataHandles(ref state);
            m_CheckPointQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(CheckPoint)
                }
            });
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            m_Handles.Update(ref state);
            var count = m_CheckPointQuery.CalculateEntityCount();
            state.Dependency = new TriggerCheckpointJob
            {
                CheckPointLookup = m_Handles.CheckPointLookup,
                LapProgressLookup = m_Handles.LapProgressLookup,
                LocalTransformLookup = m_Handles.LocalTransformLookup,
                Count = count
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        }
    }
}