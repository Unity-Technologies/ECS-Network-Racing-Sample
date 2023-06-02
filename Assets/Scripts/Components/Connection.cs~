using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Tags component to allow auto connection to the multiplayer server
    /// </summary>
    public struct AutoConnect : IComponentData { }

    /// <summary>
    /// Sets the state for resetting the server if all players got disconnected.
    /// </summary>
    public struct ResetServerOnDisconnect : IComponentData
    {
    }

    public struct TimeOutServer : IComponentData
    {
        public double Value;
    }

    /// <summary>
    /// Stores the network global components to track the connection
    /// </summary>
    public readonly partial struct NetworkIdAspect : IAspect
    {
        public readonly Entity Self;
        readonly RefRO<NetworkId> m_NetworkId;
        readonly RefRO<NetworkSnapshotAck> m_NetworkSnapshot;
        public int Id => m_NetworkId.ValueRO.Value;
        public int EstimatedRTT => (int) m_NetworkSnapshot.ValueRO.EstimatedRTT;
    }
}