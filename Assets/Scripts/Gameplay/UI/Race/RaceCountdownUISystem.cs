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

        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var localPlayer in Query<LocalPlayerAspect>())
            {
                if (localPlayer.Player.State == m_LastState)
                {
                    return;
                }

                if (!localPlayer.Player.StartingRace)
                {
                    m_LastState = localPlayer.Player.State;
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

                m_LastState = localPlayer.Player.State;
            }
        }
    }
}