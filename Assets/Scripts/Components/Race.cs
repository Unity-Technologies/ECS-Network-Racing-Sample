using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    public enum RaceState
    {
        None,
        NotStarted,
        ReadyToRace,
        StartingRace,
        CountDown,
        InProgress,
        Finishing,
        Finished,
        Leaderboard,
        StartingRaceAutomatically
    }

    /// <summary>
    /// Stores the global data, states, and timers for the race.
    /// </summary>
    public struct Race : IComponentData
    {
        [GhostField] public RaceState State;
        [GhostField] public int LapCount;
        [GhostField] public int PlayersInRace;
        [GhostField] public int PlayersFinished;
        [GhostField] public float CurrentTimer;
        
        public RaceState LastState;
        public float CountDownTimer;
        public float FinalTimer;
        public float PlayersReadyTimer;
        public float IntroRaceTimer;
        public float CelebrationIdleTimer;
        public float LeaderboardTimer;
        // Time when the race starts
        public double InitialTime;
        public bool TimerFinished;
        public bool CanJoinRace => State is RaceState.NotStarted or RaceState.StartingRaceAutomatically;
        public bool IsReadyToRace =>State is RaceState.ReadyToRace;
        public bool HasFinished => State is RaceState.Finished;
        public bool NotStarted => State is RaceState.NotStarted;
        public bool IsRaceStarting => State is RaceState.StartingRace;
        public bool IsCountDown => State is RaceState.CountDown;
        public bool IsFinishing => State is RaceState.Finishing;
        public bool IsInProgress => State is RaceState.InProgress or RaceState.Finishing;
        public bool IsShowingLeaderboard => State is RaceState.Leaderboard;
        public bool DidAllPlayersFinish => PlayersFinished == PlayersInRace;      
        
        public void CancelRaceStart()
        {
            if (State == RaceState.ReadyToRace)
                State = RaceState.NotStarted;
        }
        public void StartRace()
        {
            State = RaceState.StartingRace;
        }
        public void SetRaceState(RaceState state)
        {
            State = state;
            TimerFinished = false;
        }

        public void ResetRace()
        {
            PlayersInRace = 0;
            PlayersFinished = 0;
        }
    }
}