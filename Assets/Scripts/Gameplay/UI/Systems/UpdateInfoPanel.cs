using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Collects system information metrics
    /// and send all the data to the Info panel UI
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateInfoPanel : ISystem
    {
        private EntityQuery m_NumberOfPlayersQuery;
        private EntityQuery m_NetworkIDComponentQuery;

        private NetworkSnapshotAckComponent m_PingData;

        #region UpdateFPS

        private float m_UpdateRateSeconds;
        private int m_FarmeCount;
        private float m_DeltaTime;
        private float m_FPS;

        #endregion

        #region UpdateSystemsRunning

        private uint m_PreviousFrameVersion;
        private uint m_CurrentFrameVersion;
        private uint m_NumberOfSystems;

        #endregion

        #region Ping

        private int EstimatedRTT;
        private int DeviationRTT;

        #endregion

        public void OnCreate(ref SystemState state)
        {
            m_UpdateRateSeconds = 4.0F;
            m_FarmeCount = 0;
            m_DeltaTime = 0.0F;
            m_FPS = 0.0F;
            m_NumberOfPlayersQuery = state.GetEntityQuery(ComponentType.ReadOnly<LapProgress>());
            m_NetworkIDComponentQuery = state.GetEntityQuery(ComponentType.ReadOnly<NetworkIdComponent>());
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        private float UpdateFPS()
        {
            m_FarmeCount++;
            m_DeltaTime += Time.unscaledDeltaTime;
            if (m_DeltaTime > 1.0 / m_UpdateRateSeconds)
            {
                m_FPS = m_FarmeCount / m_DeltaTime;
                m_FarmeCount = 0;
                m_DeltaTime -= 1.0F / m_UpdateRateSeconds;
            }

            return m_FPS;
        }

        private void UpdatePing(ref SystemState state)
        {
            SystemAPI.TryGetSingletonEntity<LocalUser>(out var playerEntity);
            if (!state.EntityManager.Exists(playerEntity))
                return;

            var owner = state.EntityManager.GetComponentData<GhostOwnerComponent>(playerEntity);
            var networkIdEntities = m_NetworkIDComponentQuery.ToEntityArray(state.WorldUpdateAllocator);
            foreach (var entity in networkIdEntities)
            {
                var networkIdComponent = state.EntityManager.GetComponentData<NetworkIdComponent>(entity);
                if (owner.NetworkId == networkIdComponent.Value)
                    m_PingData = state.EntityManager.GetComponentData<NetworkSnapshotAckComponent>(entity);
            }

            if (m_DeltaTime + Time.unscaledDeltaTime > 1.0 / m_UpdateRateSeconds)
            {
                EstimatedRTT = (int) m_PingData.EstimatedRTT;
                DeviationRTT = (int) m_PingData.DeviationRTT;
            }
        }

        private void GetNumberOfSystems(ref SystemState state)
        {
            m_CurrentFrameVersion = state.EntityManager.GlobalSystemVersion - m_PreviousFrameVersion;
            m_PreviousFrameVersion = state.EntityManager.GlobalSystemVersion;
            if (m_DeltaTime + Time.unscaledDeltaTime > 1.0 / m_UpdateRateSeconds)
            {
                m_NumberOfSystems = m_CurrentFrameVersion;
            }
        }

        private int GetAllEntities(EntityManager entityManager)
        {
            return entityManager.GetAllEntities().Length;
        }

        public void OnUpdate(ref SystemState state)
        {
            if(InfoPanel.Instance == null)
                return;
            
            GetNumberOfSystems(ref state);
            UpdatePing(ref state);

            var playerCount = m_NumberOfPlayersQuery.CalculateEntityCount();
            var fpsCount = UpdateFPS();
            var entityCount = GetAllEntities(state.EntityManager);
            InfoPanel.Instance.SetFPSLabel(fpsCount);
            InfoPanel.Instance.SetNumberOfPlayers(playerCount);
            InfoPanel.Instance.SetPingLabel(EstimatedRTT, DeviationRTT);
            InfoPanel.Instance.SetSystemsLabel(m_NumberOfSystems);
            InfoPanel.Instance.SetEntitiesLabel(entityCount);
        }
    }
}