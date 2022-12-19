using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Creates an array of SortableProgress components.
    /// </summary>
    [BurstCompile]
    public partial struct CreateProgressArrayJob : IJobEntity
    {
        public NativeArray<SortableProgress> SortableProgresses;

        private void Execute([EntityIndexInQuery] int index, Entity entity, in LapProgress progress,
            in TransformAspect transformAspect)
        {
            var distance = math.distancesq(progress.LastCheckPointPosition, transformAspect.WorldPosition);
            SortableProgresses[index] = new SortableProgress
            {
                Distance = distance,
                Progress = progress,
                Entity = entity
            };
        }
    }

    /// <summary>
    /// Sorts the native array based on progress and rank
    /// </summary>
    [BurstCompile]
    public partial struct SortProgressArrayJob : IJob
    {
        public NativeArray<SortableProgress> SortableProgresses;

        public void Execute()
        {
            SortableProgresses.Sort(new SortableProgressComparer());
            for (var i = 0; i < SortableProgresses.Length; i++)
            {
                var item = SortableProgresses[i];
                item.Rank = i + 1;
                SortableProgresses[i] = item;
            }

            SortableProgresses.Sort(new SortableRankComparer());
        }
    }

    /// <summary>
    /// Applies the progress to each entity storing a rank component.
    /// </summary>
    [BurstCompile]
    public partial struct ApplyProgressArrayJob : IJobEntity
    {
        [ReadOnly] public NativeArray<SortableProgress> SortableProgresses;

        private void Execute(Entity entity, ref Rank rank)
        {
            for (var i = 0; i < SortableProgresses.Length; i++)
            {
                var sortableProgress = SortableProgresses[i];
                if (sortableProgress.Entity == entity)
                {
                    rank.Value = sortableProgress.Rank;
                }
            }
        }
    }

    /// <summary>
    /// Checks if a player has already finished the race
    /// based on the total checkpoints visited.
    /// Increases the laps and sets the player state.
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct CheckPlayerFinishedJob : IJobEntity
    {
        public int TotalCheckPoints;
        public int LapsCount;
        public float CelebrationIdleTimer;
        public double ElapseTime;

        private void Execute(PlayerAspect player)
        {
            if (player.LapProgress.CurrentCheckPoint != TotalCheckPoints || player.Player.State != PlayerState.Race)
            {
                return;
            }

            // Completes the lastlap
            if (player.LapProgress.CurrentLap < LapsCount) 
            {
                player.IncreaseLapCount();
            }

            // Finish Lap            
            if (player.LapProgress.CurrentLap < LapsCount)
            {
                player.ResetCheckpoint();
            }
            // Finish the race            
            else
            {
                player.SetCelebration(CelebrationIdleTimer, ElapseTime);
            }
        }
    }

    /// <summary>
    /// Updates all player progress data based on the check points.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct UpdatePlayerProgressSystem : ISystem, ISystemStartStop
    {
        private EntityQuery m_CheckPointQuery;
        private int m_TotalCheckpoints;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
            m_CheckPointQuery = state.GetEntityQuery(ComponentType.ReadOnly<CheckPoint>());
            state.RequireForUpdate(m_CheckPointQuery);
            m_TotalCheckpoints = 0;
        }

        public void OnStartRunning(ref SystemState state)
        {
            m_TotalCheckpoints = m_CheckPointQuery.CalculateEntityCount();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();

            if (!race.IsInProgress)
            {
                return;
            }

            var updateLapProgressJob = new CheckPlayerFinishedJob
            {
                TotalCheckPoints = m_TotalCheckpoints,
                LapsCount = race.LapCount,
                CelebrationIdleTimer = race.CelebrationIdleTimer,
                ElapseTime = state.WorldUnmanaged.Time.ElapsedTime
            };
            state.Dependency = updateLapProgressJob.ScheduleParallel(state.Dependency);
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }

    /// <summary>
    /// Sets the rank for each player.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct UpdatePlayerRankSystem : ISystem
    {
        private EntityQuery m_PlayersQuery;
        public void OnCreate(ref SystemState state)
        {
            m_PlayersQuery = state.GetEntityQuery(ComponentType.ReadOnly<Player>());
            state.RequireForUpdate<LapProgress>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playersCount = m_PlayersQuery.CalculateEntityCount();
            var sortableProgresses =
                CollectionHelper.CreateNativeArray<SortableProgress>(playersCount, state.WorldUpdateAllocator);

            var checkCarProgressJob = new CreateProgressArrayJob
            {
                SortableProgresses = sortableProgresses
            };
            state.Dependency = checkCarProgressJob.Schedule(state.Dependency);

            var sortProgressJob = new SortProgressArrayJob
            {
                SortableProgresses = sortableProgresses
            };
            state.Dependency = sortProgressJob.Schedule(state.Dependency);

            var setCarProgressJob = new ApplyProgressArrayJob
            {
                SortableProgresses = sortableProgresses
            };
            state.Dependency = setCarProgressJob.ScheduleParallel(state.Dependency);

            sortableProgresses.Dispose(state.Dependency);
        }
    }
}