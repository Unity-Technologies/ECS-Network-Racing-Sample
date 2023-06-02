using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.NetCode;
using Unity.Scenes;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Restarts the server when all players got disconnected.
    /// </summary>
    public partial struct ResetServerSystem : ISystem
    {
        private EntityQuery m_query;
        private EntityQuery m_networkStreamQuery;
        private EntityQuery m_subSceneQuery;

        public void OnCreate(ref SystemState state)
        {
            m_query = state.GetEntityQuery(ComponentType.ReadOnly<ResetServerOnDisconnect>());
            m_subSceneQuery = state.GetEntityQuery(ComponentType.ReadOnly<SceneReference>());
            m_networkStreamQuery = state.GetEntityQuery(ComponentType.ReadOnly<NetworkStreamConnection>());
            state.RequireForUpdate(m_query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var connections = m_networkStreamQuery.CalculateEntityCount();
            if (connections > 0)
                return;
            
            // All players disconnected 
            // We reset the race scene 
            Debug.Log("Resetting server");

            var subScenes = m_subSceneQuery.ToEntityArray(state.WorldUpdateAllocator);
            foreach (var subScene in subScenes)
            {
                SceneSystem.UnloadScene(state.WorldUnmanaged, subScene);
                SceneSystem.LoadSceneAsync(state.WorldUnmanaged, subScene);
            }

            state.EntityManager.DestroyEntity(m_query);
        }
    }

    [BurstCompile]
    public partial struct ConnectionValidationSystem : ISystem, ISystemStartStop
    {
        private EntityQuery m_LocalUserQuery;
        private double TimeOutCachedValue;

        public void OnCreate(ref SystemState state)
        {
            TimeOutCachedValue = 0;
            state.RequireForUpdate<TimeOutServer>();
            m_LocalUserQuery = state.GetEntityQuery(ComponentType.ReadOnly<LocalUser>());
        }

        public void OnStartRunning(ref SystemState state)
        {
            ResetTimeout();
            Debug.Log($"initializing connection at: {state.WorldUnmanaged.Time.ElapsedTime}");
        }

        private void ResetTimeout()
        {
            var timeout = GetSingleton<TimeOutServer>();
            if (TimeOutCachedValue <= 0)
            {
                TimeOutCachedValue = timeout.Value;
            }
            else
            {
                timeout.Value = TimeOutCachedValue;
                SetSingleton(timeout);
            }
        }

        public void OnStopRunning(ref SystemState state)
        {
            LoadingScreen.Instance.ShowLoadingScreen(false);
            Debug.Log($"Connection completed at: {state.WorldUnmanaged.Time.ElapsedTime}");
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!ServerConnectionUtils.IsTryingToConnect)
                return;

            LoadingScreen.Instance.ShowLoadingScreen(true, "CONNECTING...");
            var timeout = GetSingleton<TimeOutServer>();
            if (timeout.Value > 0)
            {
                timeout.Value -= state.WorldUnmanaged.Time.DeltaTime;
                SetSingleton(timeout);
                if (m_LocalUserQuery.CalculateEntityCount() > 0)
                {
                    ServerConnectionUtils.IsTryingToConnect = false;
                    ResetTimeout();
                    state.Enabled = false;
                }
            }
            else 
            {
                ServerConnectionUtils.IsTryingToConnect = false;
                ResetTimeout();
                Popup.Instance.Show("Connection Error", 
                                    "The server could not be found.", 
                                    "Restart",
                                    () => SceneLoader.Instance.LoadScene(SceneType.MainMenu));
                Debug.Log($"Server connection timeout: {state.WorldUnmanaged.Time.ElapsedTime}");
                state.Enabled = false;
            }
        }
    }
}