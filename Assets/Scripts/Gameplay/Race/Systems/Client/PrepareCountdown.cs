using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Entities;
using Unity.Entities.Racing.Gameplay;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    /// <summary>
    /// Prepare the client to Start the Race
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    [BurstCompile]
    public partial struct PrepareCountdown : ISystem
    {
        private PlayerState m_LastState;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalUser>();
            state.RequireForUpdate<Player>();
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            if (TryGetSingletonEntity<LocalUser>(out var localUser))
            {
                var playerState = state.EntityManager.GetComponentData<Player>(localUser).State;

                if (playerState == m_LastState)
                    return;

                if (playerState == PlayerState.StartingRace)
                {
                    Fader.Instance?.FadeOutIn();
                    TimelineManager.Instance?.PlayCountdownTimeline();
                }

                m_LastState = playerState;
            }
        }
    }
}