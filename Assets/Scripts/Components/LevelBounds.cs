using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Entities.Racing.Common
{
    public struct LevelBounds : IComponentData
    {
        public AABB Value;
    }

    public struct ResetTransform : IComponentData
    {
        public float3 Translation;
        public quaternion Rotation;
    }
}