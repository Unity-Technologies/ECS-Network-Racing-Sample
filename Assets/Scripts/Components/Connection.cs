using Unity.Entities;
using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    public struct AutoConnect : IComponentData
    {
    }

    public struct ResetServerOnDisconnect : IComponentData
    {
    }
    public readonly partial struct NetworkIdAspect : IAspect
    {
        public readonly Entity Self;
        readonly RefRO<NetworkIdComponent> m_NetworkId;
        readonly RefRO<NetworkSnapshotAckComponent> m_NetworkSnapshot;
        public int Id => m_NetworkId.ValueRO.Value;
        public int EstimatedRTT => (int) m_NetworkSnapshot.ValueRO.EstimatedRTT;

    }
}