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
        readonly RefRO<LocalTransform> m_LocalTransform;
        readonly RefRO<CheckPoint> m_CheckPoint;
        public int CheckPointId => m_CheckPoint.ValueRO.Id;
        public float3 LocalPosition => m_LocalTransform.ValueRO.Position;
        public quaternion LocalRotation => m_LocalTransform.ValueRO.Rotation;
    }

    /// <summary>
    /// Accessing the Check point entity locator or visuals
    /// </summary>
    public readonly partial struct CheckPointLocatorAspect : IAspect
    {
        readonly RefRW<LocalTransform> m_LocalTransform;
        readonly RefRO<CheckPointLocator> m_CheckPointLocator;
        public float3 GetResetPosition => m_CheckPointLocator.ValueRO.ResetPosition;

        public void SetLocalTransform(float3 position, quaternion rotation)
        {
            m_LocalTransform.ValueRW.Position = position;
            m_LocalTransform.ValueRW.Rotation = rotation;
        }
    }
}