using Unity.Entities;
using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    public enum RaceState
    {
        None,
        Lobby,
        ReadyToRace,
        StartingRace,
        CountDown,
        InProgress,
        Finished,
        AllFinished,
        Leaderboard,
        StartingRaceAutomatically
    }

    public struct Race : IComponentData
    {
        [GhostField] public RaceState State;
        [GhostField] public int LapCount;
        [GhostField] public int PlayersInRace;
        [GhostField] public int PlayersFinished;
        [GhostField] public float CurrentTimer;
        public float CountDownTimer;
        public float FinalTimer;
        public float PlayersReadyTimer;
        public float IntroRaceTimer;
        public float WaitToShowLeaderboardTimer;
        public float LeaderboardTimer;

        // Time when the race starts
        public double InitialTime;

        public void SetRaceState(RaceState state)
        {
            State = state;
        }

        public void ResetRace()
        {
            PlayersInRace = 0;
            PlayersFinished = 0;
        }

        public bool IsFinished()
        {
            return State is (RaceState.Finished or RaceState.AllFinished or RaceState.Leaderboard);
        }
    }
}