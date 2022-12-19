using Unity.Mathematics;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Stores the limits of the level in AABB format.
    /// </summary>
    public struct LevelBounds : IComponentData
    {
        public AABB Value;
    }

    /// <summary>
    /// Stores the initial transform and rotation for resetting the player.
    /// </summary>
    public struct ResetTransform : IComponentData
    {
        public float3 Translation;
        public quaternion Rotation;
    }
}