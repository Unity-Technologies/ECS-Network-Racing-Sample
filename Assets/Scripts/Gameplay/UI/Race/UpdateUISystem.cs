using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.NetCode;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Update UI State based on the Player State
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateUISystem : ISystem
    {
        private PlayerState m_CurrentState;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalUser>();
            state.RequireForUpdate<Player>();
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state) { }
        
        public void OnUpdate(ref SystemState state)
        {
            if (HUDController.Instance == null)
                return;

            TryGetSingletonEntity<LocalUser>(out var localUser);
            var playerState = state.EntityManager.GetComponentData<Player>(localUser).State;

            var race = GetSingleton<Race>();
            HUDController.Instance.EnableStartButton(race.State == RaceState.NotStarted);

            if (m_CurrentState == playerState)
                return;

            switch (playerState)
            {
                case PlayerState.Lobby:
                    ShowLobbyUI();
                    break;
                case PlayerState.ReadyToRace:
                    ShowReadyToRaceUI();
                    break;
                case PlayerState.Race:
                    ShowInRaceUI();
                    break;
                case PlayerState.StartingRace:
                    ShowRaceIntroUI();
                    break;
                case PlayerState.Countdown:
                    ShowCountdownUI();
                    break;
                case PlayerState.Leaderboard:
                    ShowLeaderboard();
                    break;
            }

            m_CurrentState = playerState;
        }

        // Reset HUD, Timeline and Leaderboard
        private void ShowLobbyUI()
        {
            HUDController.Instance.ShowLobbyState();
            TimelineManager.Instance.ResetFinalCameras();
            LeaderboardPanel.Instance.ClearLeaderboard();
            LeaderboardPanel.Instance.ShowLeaderboard(false);
            HUDController.Instance.Finish(false);
            if(UIMobileInput.Instance)
                UIMobileInput.Instance.Show();
        }

        private void ShowReadyToRaceUI()
        {
            HUDController.Instance.ShowCancelButton(true);
            HUDController.Instance.ShowBottomMessage(true,
                "THE RACE IS ABOUT TO START, SOON YOU WILL BE MOVED TO THE STARTING POINT");
            HUDController.Instance.ShowLeftMenu(false, false);
            HUDController.Instance.ShowMenuButton(false);
            if(UIMobileInput.Instance)
                UIMobileInput.Instance.Hide();
        }

        private void ShowInRaceUI()
        {
            HUDController.Instance.ShowMenuButton(false);
            HUDController.Instance.ShowRaceInfoPanel(true);
            if(UIMobileInput.Instance)
                UIMobileInput.Instance.Show();
        }

        private void ShowRaceIntroUI()
        {
            HUDController.Instance.ShowBottomMessage(false);
            HUDController.Instance.ShowCancelButton(false);
            HUDController.Instance.ShowMenuButton(false);
            if(UIMobileInput.Instance)
                UIMobileInput.Instance.Hide();
        }

        private void ShowCountdownUI()
        {
            HUDController.Instance.StartCountDown();
        }

        private void ShowLeaderboard()
        {
            LeaderboardPanel.Instance.ShowLeaderboard(true);
            TimelineManager.Instance.PlayLeaderboardTimeline();
            HUDController.Instance.Finish(false);
            HUDController.Instance.ShowBottomMessage(false);
            if(UIMobileInput.Instance)
                UIMobileInput.Instance.Hide();
        }
    }

    /// <summary>
    /// Update player Rank and Lap
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateUIRaceInfo : ISystem
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

            if (!race.IsInProgress)
                return;
            
            foreach (var (player, rank, lapProgress) 
                     in Query<RefRO<Player>,RefRO<Rank>,RefRO<LapProgress>>()
                         .WithAll<GhostOwner>()
                         .WithAll<LocalUser>())
            {
                HUDController.Instance.SetPosition(rank.ValueRO.Value, race.PlayersInRace);
                HUDController.Instance.SetLap(lapProgress.ValueRO.CurrentLap, lapProgress.ValueRO.LapCount);
                if (player.ValueRO.IsCelebrating)
                    HUDController.Instance.Finish(true, rank.ValueRO.Value);
            }
        }
    }
}