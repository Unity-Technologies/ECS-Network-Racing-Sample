using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
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

    /// <summary>
    /// References the components to Calculate volume according to the velocity.
    /// </summary>
    public readonly partial struct AudioAspect : IAspect
    {
        public readonly Entity Self;
        readonly RefRO<PhysicsVelocity> m_Velocity;
        readonly RefRO<LocalToWorld> m_LocalToWorld;
        private readonly RefRO<VolumeData> m_VolumeData;
        public LocalToWorld LocalToWorld => m_LocalToWorld.ValueRO;
        public float GetVolumeRange()
        { 
            var audioVolumeRange = new float2(m_VolumeData.ValueRO.Min, m_VolumeData.ValueRO.Max);
            var linearVelocityMagnitude = math.length(m_Velocity.ValueRO.Linear);
            return math.min(audioVolumeRange.y, audioVolumeRange.x + (linearVelocityMagnitude / 100f));
        }
    }
}