using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Show Leaderboard and pass to Lobby
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(UpdateTimerSystem))]
    public partial struct ClearLeaderboard : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;
            if (race.State is not RaceState.Leaderboard)
                return;

            if (race.CurrentTimer < 0)
            {
                // Change all the players state
                var changePlayerStateJob = new ChangePlayerStateJob
                {
                    CurrentState = PlayerState.Leaderboard,
                    TargetState = PlayerState.Lobby
                };
                state.Dependency = changePlayerStateJob.ScheduleParallel(state.Dependency);

                // Clear Leaderboard Buffer
                var leaderboard = GetSingletonBuffer<LeaderboardData>();
                leaderboard.Clear();

                // Reset Players in Race
                race.SetRaceState(RaceState.NotStarted);
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
    [UpdateAfter(typeof(RaceFinishSystem))]
    [UpdateBefore(typeof(WaitToShowLeaderboard))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SetLeaderboard : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();

            // If the race is not finished, skip this
            if (!race.HasFinished)
                return;

            var leaderboard = GetSingletonBuffer<LeaderboardData>();

            foreach (var player in Query<PlayerAspect>())
            {
                var ping = 0;

                if (player.HasArrived && !player.LapProgress.AddedToLeaderboard)
                {
                    var time = player.LapProgress.ArrivalTime - race.InitialTime;
                    foreach (var networkId in Query<NetworkIdAspect>())
                    {
                        if (player.NetworkId == networkId.Id)
                        {
                            ping = networkId.EstimatedRTT;
                        }
                    }

                    leaderboard.Add(new LeaderboardData
                    {
                        Name = player.Name,
                        Rank = player.Rank,
                        Time = (float) time,
                        Ping = ping
                    });

                    player.AddedToLeaderboard();
                }
                
                if (player.Player.HasFinished && !player.LapProgress.HasArrived && !player.LapProgress.AddedToLeaderboard)
                {
                    foreach (var networkId in Query<NetworkIdAspect>())
                    {
                        if (player.NetworkId == networkId.Id)
                        {
                            ping = networkId.EstimatedRTT;
                        }
                    }
                    
                    leaderboard.Add(new LeaderboardData
                    {
                        Name = player.Name,
                        Rank = player.Rank,
                        Time = 0,
                        Ping = ping
                    });

                    player.AddedToLeaderboard();
                }
            }

            leaderboard.AsNativeArray().Sort(new SortableLeaderboardComparer());
        }
    }

    /// <summary>
    /// Sort the Native Array based on Rank value
    /// </summary>
    [BurstCompile]
    public struct SortableLeaderboardComparer : IComparer<LeaderboardData>
    {
        public int Compare(LeaderboardData x, LeaderboardData y)
        {
            return x.Rank.CompareTo(y.Rank);
        }
    }

    /// <summary>
    /// Shows the leaderboard after the countdown has finished
    /// Then change race state to show the leaderboard.
    /// </summary>
    [UpdateAfter(typeof(UpdateTimerSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct WaitToShowLeaderboard : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;

            // If the race is not finished, skip this
            if (!race.HasFinished)
                return;

            if (race.CurrentTimer <= 0)
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