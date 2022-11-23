using Unity.Entities;
using UnityEngine;
using Unity.Entities.Racing.Common;

namespace Dots.Racing
{
    public class RaceAuthoring : MonoBehaviour
    {
        [Header("Timers")]
        public int CountDownTimer = 3;
        public int FinalTimer = 30;
        public int PlayersReadyTimer = 10;
        public int IntroRaceTimer = 7;
        public int LeaderboardTimer = 5;
        public int WaitToShowLeaderboardTimer = 3;
        [Header("Laps")]
        public int LapCount = 2;
        public RaceState InitialRaceState;
    }

    public class RaceBaker : Baker<RaceAuthoring>
    {
        public override void Bake(RaceAuthoring authoring)
        {
            var race = new Race
            {
                CountDownTimer = authoring.CountDownTimer,
                FinalTimer = authoring.FinalTimer,
                PlayersReadyTimer = authoring.PlayersReadyTimer,
                IntroRaceTimer = authoring.IntroRaceTimer,
                LeaderboardTimer = authoring.LeaderboardTimer,
                WaitToShowLeaderboardTimer = authoring.WaitToShowLeaderboardTimer,
                LapCount = authoring.LapCount,
            };
            race.SetRaceState(authoring.InitialRaceState);
            AddComponent(race);
            AddBuffer<LeaderboardData>();
        }
    }
}