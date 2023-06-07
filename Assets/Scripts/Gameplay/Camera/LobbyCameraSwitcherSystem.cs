using Unity.Burst;
using Unity.Entities.Racing.Common;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Creates camera transition for Lobby
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct LobbyCameraSwitcherSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalUser>();
            state.RequireForUpdate<CarInput>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var localPlayer in Query<LocalPlayerAspect>())
            {
                if (localPlayer.Player.State != PlayerState.Lobby)
                {
                    return;
                }

                if (localPlayer.HasAnyInput() && CameraSwitcher.Instance != null)
                {
                    CameraSwitcher.Instance.ShowBackCamera();
                    state.Enabled = false;
                }
            }
        }
    }
}