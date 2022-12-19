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
    
    /// <summary>
    /// Access the necessary information to spawn a player.
    /// </summary>
    public readonly partial struct SpawnPlayerRequestAspect : IAspect
    {
        public readonly Entity Self;
        readonly RefRO<SpawnPlayerRequest> m_SpawnRequest;
        readonly RefRO<ReceiveRpcCommandRequestComponent> m_ReceiveRpcRequest;
        public int Id => m_SpawnRequest.ValueRO.Id;
        public FixedString64Bytes Name => m_SpawnRequest.ValueRO.Name;
        public Entity SourceConnection => m_ReceiveRpcRequest.ValueRO.SourceConnection;
    }
}