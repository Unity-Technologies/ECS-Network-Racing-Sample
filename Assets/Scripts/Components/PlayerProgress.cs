using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Stores player progress in the game
    /// </summary>
    public struct LapProgress : IComponentData
    {
        [GhostField] public int CurrentCheckPoint;
        [GhostField] public int LapCount;
        [GhostField] public int CurrentLap;
        [GhostField] public float3 LastCheckPointPosition;
        [GhostField] public bool AddedToLeaderboard;
        [GhostField] public float CelebrationIdleDelay;
        [GhostField] public double ArrivalTime;

        public int NextPointId => CurrentCheckPoint + 1;

        public bool HasArrived => ArrivalTime > 0;

        public void Reset(int lapCount = 1)
        {
            CurrentCheckPoint = 0;
            CurrentLap = 1;
            AddedToLeaderboard = false;
            LapCount = lapCount;
            CelebrationIdleDelay = 0;
            ArrivalTime = 0;
        }
    }

    /// <summary>
    /// Access the player's progress data to do a comparison process
    /// </summary>
    public struct SortableProgress
    {
        public float Distance;
        public LapProgress Progress;
        public Entity Entity;
        public int Rank;
    }

    /// <summary>
    /// Executes comparison between rank parameters
    /// </summary>
    [BurstCompile]
    public struct SortableRankComparer : IComparer<SortableProgress>
    {
        public int Compare(SortableProgress x, SortableProgress y)
        {
            return y.Rank.CompareTo(x.Rank);
        }
    }

    /// <summary>
    /// Executes comparison between progress parameters
    /// </summary>
    [BurstCompile]
    public struct SortableProgressComparer : IComparer<SortableProgress>
    {
        public int Compare(SortableProgress x, SortableProgress y)
        {
            if(x.Progress.HasArrived && y.Progress.HasArrived)
                return x.Progress.ArrivalTime.CompareTo(y.Progress.ArrivalTime);

            if (x.Progress.HasArrived || y.Progress.HasArrived)
                return y.Progress.HasArrived.CompareTo(x.Progress.HasArrived);

            if (x.Progress.CurrentLap != y.Progress.CurrentLap)
                return y.Progress.CurrentLap.CompareTo(x.Progress.CurrentLap);

            if (x.Progress.CurrentCheckPoint == y.Progress.CurrentCheckPoint)
            {
                return y.Distance.CompareTo(x.Distance);
            }

            return y.Progress.CurrentCheckPoint.CompareTo(x.Progress.CurrentCheckPoint);
        }
    }
    
    /// <summary>
    /// Stores the current rank in the leaderboard
    /// </summary>
    public struct Rank : IComponentData
    {
        [GhostField] public int Value;
    }
}