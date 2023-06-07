using UnityEngine;
using Unity.Entities.Racing.Common;

namespace Unity.Entities.Racing.Authoring
{
    public class RaceAuthoring : MonoBehaviour
    {
        [Header("Timers")] public float CountDownTimer = 3;
        public float FinalTimer = 30;
        public float PlayersReadyTimer = 10;
        public float IntroRaceTimer = 7;
        public float LeaderboardTimer = 5;
        public float CelebrationIdleTimer = 0.1f;
        [Header("Laps")] public int LapCount = 2;
        public RaceState InitialRaceState = RaceState.NotStarted;

        private class Baker : Baker<RaceAuthoring>
        {
            public override void Bake(RaceAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
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
                AddComponent(entity, race);
                AddBuffer<LeaderboardData>(entity);
            }
        }
    }
}