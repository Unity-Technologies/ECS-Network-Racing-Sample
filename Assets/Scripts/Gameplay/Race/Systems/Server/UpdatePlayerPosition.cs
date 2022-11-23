using System.Collections.Generic;
using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Dots.Racing
{
    [BurstCompile]
    public partial struct CheckCarProgressJob : IJobEntity
    {
        public NativeArray<SortableProgress> SortableProgresses;

        private void Execute([EntityIndexInQuery] int index, Entity entity, in LapProgress progress, in TransformAspect transformAspect)
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

    [BurstCompile]
    public partial struct SortProgressJob : IJob
    {
        public NativeArray<SortableProgress> SortableProgresses;

        public void Execute()
        {
            SortableProgresses.Sort(new SortableProgressComparer());
            for (int i = 0; i < SortableProgresses.Length; i++)
            {
                var item = SortableProgresses[i];
                item.Rank = i + 1;
                SortableProgresses[i] = item;
            }
            SortableProgresses.Sort(new SortableRankComparer());
        }
    }

    [BurstCompile]
    public partial struct SetCarProgressJob : IJobEntity
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

    public struct SortableProgress
    {
        public float Distance;
        public LapProgress Progress;
        public Entity Entity;
        public int Rank;
    }

    [BurstCompile]
    public struct SortableProgressComparer : IComparer<SortableProgress>
    {
        public int Compare(SortableProgress x, SortableProgress y)
        {
            if (x.Progress.CurrentLap != y.Progress.CurrentLap)
                return y.Progress.CurrentCheckPoint.CompareTo(x.Progress.CurrentLap);

            if (x.Progress.CurrentCheckPoint == y.Progress.CurrentCheckPoint)
            {
                return y.Distance.CompareTo(x.Distance);
            }

            return y.Progress.CurrentCheckPoint.CompareTo(x.Progress.CurrentCheckPoint);
        }
    }

    [BurstCompile]
    public struct SortableRankComparer : IComparer<SortableProgress>
    {
        public int Compare(SortableProgress x, SortableProgress y)
        {
            return y.Rank.CompareTo(x.Rank);
        }
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct UpdatePlayerPosition : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LapProgress>();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var sortableProgresses =
                CollectionHelper.CreateNativeArray<SortableProgress>(16, state.WorldUpdateAllocator);

            var checkCarProgressJob = new CheckCarProgressJob
            {
                SortableProgresses = sortableProgresses
            };
            state.Dependency = checkCarProgressJob.Schedule(state.Dependency);

            var sortProgressJob = new SortProgressJob
            {
                SortableProgresses = sortableProgresses
            };
            state.Dependency = sortProgressJob.Schedule(state.Dependency);

            var setCarProgressJob = new SetCarProgressJob
            {
                SortableProgresses = sortableProgresses
            };
            state.Dependency = setCarProgressJob.ScheduleParallel(state.Dependency);

            sortableProgresses.Dispose(state.Dependency);
        }
    }
}