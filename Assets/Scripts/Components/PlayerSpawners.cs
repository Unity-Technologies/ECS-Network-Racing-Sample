using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    public struct PlayerSpawner : IComponentData
    {
    }

    public struct SpawnPoint : IBufferElementData
    {
        public float3 TrackPosition;
        public quaternion TrackRotation;
        public float3 LobbyPosition;
        public quaternion LobbyRotation;
    }
    
    public struct SpawnPlayerRequest : IRpcCommand
    {
        public FixedString64Bytes Name;
        public int Id;
    }

    public struct PlayerSpawned : IComponentData { }
    
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