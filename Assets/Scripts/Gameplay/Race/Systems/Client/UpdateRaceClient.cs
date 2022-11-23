using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Entities;
using Unity.Entities.Racing.Gameplay;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    /// <summary>
    /// Update UI State based on the Player State
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateClientHUD : ISystem
    {
        private PlayerState m_CurrentState;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalUser>();
            state.RequireForUpdate<Player>();
        }

        public void OnDestroy(ref SystemState state) { }
        
        public void OnUpdate(ref SystemState state)
        {
            if (HUDController.Instance == null)
                return;

            TryGetSingletonEntity<LocalUser>(out var localUser);
            var playerState = state.EntityManager.GetComponentData<Player>(localUser).State;

            var race = GetSingleton<Race>();
            HUDController.Instance.EnableStartButton(race.State == RaceState.Lobby);

            if (m_CurrentState == playerState)
                return;

            switch (playerState)
            {
                case PlayerState.Lobby:
                    // Reset HUD, Timeline and Leaderboard
                    HUDController.Instance.ShowLobbyState();
                    TimelineManager.Instance.ResetFinalCameras();
                    LeaderboardPanel.Instance.ClearLeaderboard();
                    LeaderboardPanel.Instance.ShowLeaderboard(false);
                    break;
                case PlayerState.Race:
                    HUDController.Instance.ShowMenuButton(false);
                    HUDController.Instance.ShowRaceInfoPanel(true);
                    break;
                case PlayerState.ReadyToRace:
                    HUDController.Instance.ShowCancelButton(true);
                    HUDController.Instance.ShowBottomMessage(true,
                        "THE RACE IS ABOUT TO START, SOON YOU WILL BE MOVED TO THE STARTING POINT");
                    HUDController.Instance.ShowLeftMenu(false, false);
                    HUDController.Instance.ShowMenuButton(false);
                    break;
                case PlayerState.StartingRace:
                    HUDController.Instance.ShowBottomMessage(false);
                    HUDController.Instance.ShowCancelButton(false);
                    HUDController.Instance.ShowMenuButton(false);
                    break;
                case PlayerState.Countdown:
                    HUDController.Instance.StartCountDown();
                    break;
                case PlayerState.Leaderboard:
                    LeaderboardPanel.Instance.ShowLeaderboard(true);
                    TimelineManager.Instance.PlayLeaderboardTimeline();
                    HUDController.Instance.Finish(false);
                    HUDController.Instance.ShowBottomMessage(false);
                    break;
            }

            m_CurrentState = playerState;
        }
    }

    /// <summary>
    /// Update player Rank and Lap
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateClientRaceInfo : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            if (HUDController.Instance == null)
                return;

            var race = GetSingleton<Race>();

            if (race.State != RaceState.InProgress)
                return;
            
            foreach (var player in Query<PlayerAspect>().WithAll<LocalUser>())
            {
                HUDController.Instance.SetPosition(player.Rank, race.PlayersInRace);
                HUDController.Instance.SetLap(player.LapProgress.CurrentLap, player.LapProgress.LapCount);
                if (player.LapProgress.Finished && player.LapProgress.InRace)
                    HUDController.Instance.Finish(true, player.Rank);
            }
        }
    }
}