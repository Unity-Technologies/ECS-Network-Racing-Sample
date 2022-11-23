using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    [GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToNonOwner)]
    public struct CarInput : IInputComponentData
    {
        [GhostField(Quantization = 1000)] public float Horizontal;
        [GhostField(Quantization = 1000)] public float Vertical;
        [GhostField] public bool HandBreak;
    }
}