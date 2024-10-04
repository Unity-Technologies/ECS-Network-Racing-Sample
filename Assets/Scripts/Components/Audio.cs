using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Points to the Entities with AudioSource attached
    /// </summary>
    public struct AudioSourceTag : IComponentData
    {
    }

    /// <summary>
    /// Stores Volume Data
    /// </summary>
    public struct VolumeData : IComponentData
    {
        public float Min;
        public float Max;
    }
}