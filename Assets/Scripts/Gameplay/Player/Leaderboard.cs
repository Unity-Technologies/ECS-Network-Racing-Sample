using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.NetCode;
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

            foreach (var (playerName, rank, player, lapProgress, ghostOwner) 
                     in Query<RefRO<PlayerName>,
                             RefRO<Rank>,
                             RefRO<Player>, 
                             RefRW<LapProgress>,
                             RefRO<GhostOwner>>())
            {
                var ping = 0;

                var hasArrived = (player.ValueRO.IsCelebrating || player.ValueRO.HasFinished) && lapProgress.ValueRO.HasArrived;
                if (hasArrived && !lapProgress.ValueRO.AddedToLeaderboard)
                {
                    var time = lapProgress.ValueRO.ArrivalTime - race.InitialTime;
                    foreach (var (networkId, networkSnapshotAck) 
                             in Query<RefRO<NetworkId>, RefRO<NetworkSnapshotAck>>())
                    {
                        if (ghostOwner.ValueRO.NetworkId == networkId.ValueRO.Value)
                        {
                            ping = (int) networkSnapshotAck.ValueRO.EstimatedRTT;
                        }
                    }

                    leaderboard.Add(new LeaderboardData
                    {
                        Name = playerName.ValueRO.Name,
                        Rank = rank.ValueRO.Value,
                        Time = (float) time,
                        Ping = ping
                    });

                    lapProgress.ValueRW.AddedToLeaderboard = true;
                }
                
                if (player.ValueRO.HasFinished && !lapProgress.ValueRO.HasArrived && !lapProgress.ValueRO.AddedToLeaderboard)
                {
                    foreach (var (networkId, networkSnapshotAck) in Query<RefRO<NetworkId>, RefRO<NetworkSnapshotAck>>())
                    {
                        if (ghostOwner.ValueRO.NetworkId == networkId.ValueRO.Value)
                        {
                            ping = (int) networkSnapshotAck.ValueRO.EstimatedRTT;
                        }
                    }
                    
                    leaderboard.Add(new LeaderboardData
                    {
                        Name = playerName.ValueRO.Name,
                        Rank = rank.ValueRO.Value,
                        Time = 0,
                        Ping = ping
                    });

                    lapProgress.ValueRW.AddedToLeaderboard = true;
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