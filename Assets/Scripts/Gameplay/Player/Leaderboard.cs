using System.Collections.Generic;
using Dots.Racing;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities.Racing.Common;
using Unity.Entities.Racing.Gameplay;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Show Leaderboard and pass to Lobby
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(UpdateTimerSystem))]
    public partial struct ShowLeaderboard : ISystem
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
            if (race.State is not RaceState.Leaderboard)
                return;

            var leaderboard = GetSingletonBuffer<LeaderboardData>();

            race.CurrentTimer -= Time.DeltaTime;
            if (race.CurrentTimer < 0)
            {
                // Change all the players state
                var changePlayerStateJob = new ChangePlayerStateJob
                {
                    CurrentState = PlayerState.Leaderboard,
                    TargetState = PlayerState.Lobby
                };
                state.Dependency = changePlayerStateJob.ScheduleParallel(state.Dependency);

                race.SetRaceState(RaceState.Lobby);

                // Clear Leaderboard Buffer
                leaderboard.Clear();

                // Reset Players in Race                
                race.ResetRace();

                var resetWheelsJob = new ResetWheelsJob();
                state.Dependency = resetWheelsJob.ScheduleParallel(state.Dependency);
            }

            SetSingleton(race);
        }
    }

 
    /// <summary>
    /// Fills the Leaderboard Data for each player that finishes the race
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SetLeaderboard : ISystem
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
            var race = GetSingleton<Race>();

            // If the race is not finished, skip this
            if (race.State is not (RaceState.AllFinished or RaceState.Finished))
                return;

            var leaderboard = GetSingletonBuffer<LeaderboardData>();

            foreach (var car in Query<PlayerAspect>())
            {
                var ping = 0;
                if (car.LapProgress.Finished && !car.LapProgress.AddedToLeaderboard)
                {
                    var time = Time.ElapsedTime - race.InitialTime;
                    foreach (var networkId in Query<NetworkIdAspect>())
                    {
                        if (car.NetworkId == networkId.Id)
                        {
                            ping = networkId.EstimatedRTT;
                        }
                    }

                    leaderboard.Add(new LeaderboardData
                    {
                        Name = car.Name,
                        Rank = car.Rank,
                        Time = (float) time,
                        Ping = ping
                    });

                    car.AddedToLeaderboard();
                }
                
                if (!car.LapProgress.Finished && !car.LapProgress.AddedToLeaderboard && race.State is RaceState.AllFinished)
                {
                    foreach (var networkId in Query<NetworkIdAspect>())
                    {
                        if (car.NetworkId == networkId.Id)
                        {
                            ping = networkId.EstimatedRTT;
                        }
                    }
                    
                    leaderboard.Add(new LeaderboardData
                    {
                        Name = car.Name,
                        Rank = car.Rank,
                        Time = 0,
                        Ping = ping
                    });

                    car.AddedToLeaderboard();
                }
            }

            leaderboard.AsNativeArray().Sort(new SortableLeadboardComparer());
        }
    }


    [BurstCompile]
    public struct SortableLeadboardComparer : IComparer<LeaderboardData>
    {
        public int Compare(LeaderboardData x, LeaderboardData y)
        {
            return x.Rank.CompareTo(y.Rank);
        }
    }

    [BurstCompile]
    [UpdateAfter(typeof(UpdateTimerSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct WaitToShowLeaderboard : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;

            // If the race is not finished, skip this
            if (race.State is not RaceState.AllFinished)
                return;

            race.CurrentTimer -= Time.DeltaTime;
            if (race.CurrentTimer < 0)
            {
                // Change all the players state from FINISHED to LEADERBOARD
                var changePlayerStateJob = new ChangePlayerStateJob
                {
                    CurrentState = PlayerState.Finished,
                    TargetState = PlayerState.Leaderboard
                };
                state.Dependency = changePlayerStateJob.ScheduleParallel(state.Dependency);

                // Show the Leaderboard
                race.SetRaceState(RaceState.Leaderboard);
            }
            SetSingleton(race);
        }
    }
}