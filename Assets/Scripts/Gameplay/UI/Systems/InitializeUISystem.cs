using Unity.Entities.Racing.Common;
using Unity.NetCode;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Adds events to lobby buttons.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct InitializeHUDSystem : ISystem
    {
        private bool m_HUDInitialized;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (m_HUDInitialized)
                return;

            if (HUDController.Instance == null)
                return;

            var entityManager = state.EntityManager;
            var networkId = GetSingleton<NetworkId>().Value;

            HUDController.CancelStartButton.clicked += () =>
            {
                PlayerAudioManager.Instance.PlayClick();
                entityManager.CreateEntity(typeof(CancelPlayerReadyRPC), typeof(SendRpcCommandRequest));
            };

            HUDController.ResetCarButton.clicked += () =>
            {
                var requestEntity = entityManager.CreateEntity(typeof(SendRpcCommandRequest));
                entityManager.AddComponentData(requestEntity, new ResetCarRPC {Id = networkId});
                PlayerAudioManager.Instance.PlayClick();
            };
            
            HUDController.StartRaceButton.clicked += () =>
            {
                entityManager.CreateEntity(typeof(PlayersReadyRPC), typeof(SendRpcCommandRequest));
                PlayerAudioManager.Instance.PlayClick();
            };

            m_HUDInitialized = true;
        }
    }
}