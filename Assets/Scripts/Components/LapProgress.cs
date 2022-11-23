using Unity.Mathematics;
using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    public struct LapProgress : IComponentData
    {
        [GhostField] public int CurrentCheckPoint;
        [GhostField] public int LapCount;
        [GhostField] public int CurrentLap;
        [GhostField] public bool InRace;
        [GhostField] public float3 LastCheckPointPosition;
        [GhostField] public bool Finished;
        [GhostField] public bool AddedToLeaderboard;
        [GhostField] public float TimerToMovePlayer;

        public int NextPointId => CurrentCheckPoint + 1;

        public void Reset(int lapCount = 1)
        {
            InRace = true;
            CurrentCheckPoint = 0;
            CurrentLap = 1;
            Finished = false;
            AddedToLeaderboard = false;
            TimerToMovePlayer = 5f;
            LapCount = lapCount;
        }
    }

    public struct Rank : IComponentData
    {
        [GhostField] public int Value;
    }
}