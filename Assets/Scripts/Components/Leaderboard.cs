using Unity.Collections;

using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Stores the result to be used in the UI.
    /// </summary>
    public struct LeaderboardData : IBufferElementData
    {
        [GhostField] public int Rank;
        [GhostField] public FixedString128Bytes Name;
        [GhostField] public float Time;
        [GhostField] public int Ping;
    }
}