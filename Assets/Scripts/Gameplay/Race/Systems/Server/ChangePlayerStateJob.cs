using System.Collections.Generic;
using Dots.Racing;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities.Racing.Common;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Update Finish Timers in the Server
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(UpdateTimerSystem))]
    public partial struct FinishCountdownController : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;
            if (race.State is not RaceState.Finished)
                return;

            race.CurrentTimer -= Time.DeltaTime;
            if (race.CurrentTimer < 0)
            {
                // Force all the players who still in RACE to FINISHED
                var forcePlayersToFinishJob = new ChangePlayerStateJob
                {
                    CurrentState = PlayerState.Race,
                    TargetState = PlayerState.Finished
                };
                state.Dependency = forcePlayersToFinishJob.ScheduleParallel(state.Dependency);
                race.SetRaceState(RaceState.AllFinished);
            }

            SetSingleton(race);
        }
    }
    
    /// <summary>
    /// Handles the events when the Player Finishes the race
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateBefore(typeof(UpdateTimerSystem))]
    public partial struct RaceFinishController : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;
            if (race.State is not (RaceState.InProgress or RaceState.Finished))
                return;

            var playersFinished = 0;
            foreach (var car in Query<PlayerAspect>())
            {
                if (car.LapProgress.Finished)
                {
                    playersFinished++;
                    car.SetFinishedRace();
                }
            }

            if (playersFinished <= 0)
                return;

            race.PlayersFinished = playersFinished;
            race.SetRaceState(playersFinished == race.PlayersInRace ? RaceState.AllFinished : RaceState.Finished);
            SetSingleton(race);
        }
    }
}