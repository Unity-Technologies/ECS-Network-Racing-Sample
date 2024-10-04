using Unity.Burst;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
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
            foreach (var (carInput, player) 
                     in Query<RefRO<CarInput>, RefRO<Player>>().WithAll<LocalUser>())
            {
                if (player.ValueRO.State != PlayerState.Lobby)
                {
                    return;
                }

                var hasAnyInput = math.abs(carInput.ValueRO.Horizontal) > 0 || math.abs(carInput.ValueRO.Vertical) > 0;
                if (hasAnyInput && CameraSwitcher.Instance != null)
                {
                    CameraSwitcher.Instance.ShowBackCamera();
                    state.Enabled = false;
                }
            }
        }
    }
}