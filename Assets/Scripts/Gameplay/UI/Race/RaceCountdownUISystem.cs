using Unity.Burst;
using Unity.Entities.Racing.Common;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    ///     Prepare the client to Start the Race
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    [BurstCompile]
    public partial struct RaceCountdownUISystem : ISystem
    {
        private PlayerState m_LastState;

        public void OnUpdate(ref SystemState state)
        {
            foreach (var localPlayer in Query<RefRO<Player>>().WithAll<LocalUser>())
            {
                if (localPlayer.ValueRO.State == m_LastState)
                {
                    return;
                }

                if (!localPlayer.ValueRO.StartingRace)
                {
                    m_LastState = localPlayer.ValueRO.State;
                    return;
                }

                if (Fader.Instance != null)
                {
                    Fader.Instance.FadeOutIn();
                }

                if (TimelineManager.Instance != null)
                {
                    TimelineManager.Instance.PlayCountdownTimeline();
                }

                m_LastState = localPlayer.ValueRO.State;
            }
        }
    }
}