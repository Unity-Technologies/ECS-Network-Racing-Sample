using Unity.Entities;
using UnityEngine;
using Unity.Entities.Racing.Common;

namespace Dots.Racing
{
    public class RaceAuthoring : MonoBehaviour
    {
        [Header("Timers")]
        public float CountDownTimer = 3;
        public float FinalTimer = 30;
        public float PlayersReadyTimer = 10;
        public float IntroRaceTimer = 7;
        public float LeaderboardTimer = 5;
        public float CelebrationIdleTimer = 0.1f;
        [Header("Laps")]
        public int LapCount = 2;
        public RaceState InitialRaceState = RaceState.NotStarted;
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
                CelebrationIdleTimer = authoring.CelebrationIdleTimer,
                LapCount = authoring.LapCount,
                CurrentTimer = authoring.CountDownTimer,
            };
            race.SetRaceState(authoring.InitialRaceState);
            AddComponent(race);
            AddBuffer<LeaderboardData>();
        }
    }
}