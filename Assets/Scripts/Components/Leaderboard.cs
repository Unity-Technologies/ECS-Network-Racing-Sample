using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    public struct LeaderboardData : IBufferElementData
    {
        [GhostField] public int Rank;
        [GhostField] public FixedString128Bytes Name;
        [GhostField] public float Time;
        [GhostField] public int Ping;
    }
}