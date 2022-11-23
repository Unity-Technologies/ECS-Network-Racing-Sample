using Unity.Mathematics;

namespace Unity.Entities.Racing.Common
{
    public struct CarSelection : IComponentData
    {
        public float3 SkinPosition;
        public quaternion SkinRotation;
        public Entity CurrentSkin;
        public int SelectedId;
    }

    public struct CarSelectionUpdate : IComponentData
    {
        public bool ShouldUpdate;
        public int NewSkinId;
    }

    [InternalBufferCapacity(16)]
    public struct CarSelectionSkinData : IBufferElementData
    {
        public Entity Prefab;
    }
}