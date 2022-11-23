using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct UpdateTimerSystem : ISystem
    {
        private RaceState m_LastState;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();

            // Cache last state to execute the behavior just once
            if (race.State == m_LastState)
                return;

            switch (race.State)
            {
                case RaceState.ReadyToRace:
                    race.CurrentTimer = race.PlayersReadyTimer;
                    break;
                case RaceState.StartingRace:
                    race.CurrentTimer = race.IntroRaceTimer;
                    break;
                case RaceState.CountDown:
                    race.CurrentTimer = race.CountDownTimer;
                    break;
                case RaceState.Finished:
                    race.CurrentTimer = race.FinalTimer;
                    break;
                case RaceState.AllFinished:
                    race.CurrentTimer = race.PlayersInRace == race.PlayersFinished ? race.WaitToShowLeaderboardTimer : 1;
                    break;
                case RaceState.Leaderboard:
                    race.CurrentTimer = race.LeaderboardTimer;
                    break;
            }

            m_LastState = race.State;
            SetSingleton(race);
        }
    }
}