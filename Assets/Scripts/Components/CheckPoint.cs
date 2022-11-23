using Unity.Transforms;
#pragma warning disable CS0414

namespace Unity.Entities.Racing.Common
{
    public struct CheckPoint : IComponentData
    {
        public int Id;
    }

    public struct CheckPointLocator : IComponentData
    {
    }
    
    public readonly partial struct CheckPointAspect : IAspect
    {
        public readonly TransformAspect Transform;
        readonly RefRO<CheckPoint> m_CheckPoint;
        public int CheckPointId => m_CheckPoint.ValueRO.Id;
    }

    public readonly partial struct CheckPointLocatorAspect : IAspect
    {
        public readonly TransformAspect Transform;
        readonly RefRO<CheckPointLocator> m_CheckPointLocator;
    }
}