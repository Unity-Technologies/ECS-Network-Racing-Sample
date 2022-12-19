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
    
    /// <summary>
    /// Points to all check point component and transform,
    /// for accessing the position location and rotation.
    /// </summary>
    public readonly partial struct CheckPointAspect : IAspect
    {
        public readonly TransformAspect Transform;
        readonly RefRO<CheckPoint> m_CheckPoint;
        public int CheckPointId => m_CheckPoint.ValueRO.Id;
    }

    /// <summary>
    /// Accessing the Check point entity locator or visuals
    /// </summary>
    public readonly partial struct CheckPointLocatorAspect : IAspect
    {
        public readonly TransformAspect Transform;
        readonly RefRO<CheckPointLocator> m_CheckPointLocator;
        public float3 GetResetPosition => m_CheckPointLocator.ValueRO.ResetPosition;
    }
}