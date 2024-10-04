using Unity.Collections;
using Unity.Mathematics;
using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Tags the Entity which spawns the players.
    /// </summary>
    public struct PlayerSpawner : IComponentData
    {
    }

    /// <summary>
    /// Tags each spawn point data
    /// </summary>
    public struct SpawnPoint : IBufferElementData
    {
        public float3 TrackPosition;
        public quaternion TrackRotation;
        public float3 LobbyPosition;
        public quaternion LobbyRotation;
    }

    /// <summary>
    /// RPC, to be sent to the server to spawn a player.
    /// </summary>
    public struct SpawnPlayerRequest : IRpcCommand
    {
        public FixedString64Bytes Name;
        public int Id;
    }

    /// <summary>
    /// Tags a player who already spawns an instance.
    /// </summary>
    public struct PlayerSpawned : IComponentData { }
}