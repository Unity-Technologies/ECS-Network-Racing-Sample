using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    /// <summary>
    /// Update Checkpoint positions
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateCheckPointPositionSystem : ISystem
    {
        private float3 m_ResetPosition;
        private Entity m_Player;
        private CheckPoint m_CurrentCheckPoint;
        private float3 m_CurrentPosition;
        private quaternion m_CurrentRotation;
        
        public void OnCreate(ref SystemState state)
        {
            m_ResetPosition = new float3(0, -12f, 0);
            
            state.RequireForUpdate<Race>();
            state.RequireForUpdate<CheckPointLocator>();
            state.RequireForUpdate<LocalUser>();
            Reset();
        }

        public void OnUpdate(ref SystemState state)
        {
            if(!state.EntityManager.Exists(m_Player))
                m_Player = GetSingletonEntity<LocalUser>();
            
            var race = GetSingleton<Race>();
            if (race.State == RaceState.AllFinished)
            {
                Reset();
                foreach (var checkPointLocator in Query<CheckPointLocatorAspect>())
                {
                    checkPointLocator.Transform.WorldPosition = m_CurrentPosition;
                    checkPointLocator.Transform.WorldRotation = m_CurrentRotation;
                }
            }

            if (race.State is RaceState.InProgress or RaceState.Finished)
            {
                var currentProgress = state.EntityManager.GetComponentData<LapProgress>(m_Player);
                if (!currentProgress.InRace || currentProgress.Finished || currentProgress.NextPointId == m_CurrentCheckPoint.Id)
                    return;
            
                foreach (var checkPoint in Query<CheckPointAspect>())
                {
                    if (checkPoint.CheckPointId == currentProgress.NextPointId)
                    {
                        m_CurrentCheckPoint.Id = checkPoint.CheckPointId;
                        m_CurrentPosition = checkPoint.Transform.WorldPosition;
                        m_CurrentRotation = checkPoint.Transform.WorldRotation;
                        break;
                    }
                }

                foreach (var checkPointLocator in Query<CheckPointLocatorAspect>())
                {
                    checkPointLocator.Transform.WorldPosition = m_CurrentPosition;
                    checkPointLocator.Transform.WorldRotation = m_CurrentRotation;
                }
            }
        }
        
        public void OnDestroy(ref SystemState state) { }

        private void Reset()
        {
            m_CurrentCheckPoint = new CheckPoint {Id = int.MinValue };
            m_CurrentPosition = m_ResetPosition;
            m_CurrentRotation = quaternion.identity;
        }
    }
}