using Unity.Mathematics;
using Unity.Transforms;
#pragma warning disable CS0414

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Stores the current Check point asset Id.
    /// </summary>
    public struct CheckPoint : IComponentData
    {
        public int Id;
    }

    /// <summary>
    /// Points to the visual Check point to move to the next 
    /// location of the check point.
    /// </summary>
    public struct CheckPointLocator : IComponentData
    {
        public float3 ResetPosition;
    }
}