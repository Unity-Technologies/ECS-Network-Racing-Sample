using Unity.Burst;
using Unity.Collections;
using Unity.NetCode;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Component added to the player entity when the Vivox connection is established.
    /// </summary>
    public struct VivoxConnectionStatus : IComponentData
    {
    }

    /// <summary>
    /// System that initializes the Vivox connection for the local player.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct InitializedVivoxSystem : ISystem
    {
        private EntityQuery m_PlayerQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            m_PlayerQuery = state.GetEntityQuery(ComponentType.ReadOnly<GhostOwnerIsLocal>(),
                ComponentType.Exclude<VivoxConnectionStatus>());
            state.RequireForUpdate(m_PlayerQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (PlayerInfoController.Instance == null || 
                VivoxManager.Instance == null || 
                VivoxManager.Instance.Service == null)
                return;

            var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (local, entity) in Query<RefRO<GhostOwnerIsLocal>>()
                         .WithNone<VivoxConnectionStatus>()
                         .WithEntityAccess())
            {
                UnityEngine.Debug.Log($"<color=green>[VIVOX] Player [{PlayerInfoController.Instance.LocalPlayerName}] is starting vivox</color>");
                if (!string.IsNullOrEmpty(PlayerInfoController.Instance.LocalPlayerName))
                {
                    UnityEngine.Debug.Log("<color=green>[VIVOX] Start login to vivox</color>");
                    VivoxManager.Instance.Session.Login(PlayerInfoController.Instance.LocalPlayerName);
                    cmdBuffer.AddComponent<VivoxConnectionStatus>(entity);
                }
            }

            cmdBuffer.Playback(state.EntityManager);
            cmdBuffer.Dispose();
        }
    }
}