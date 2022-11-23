using Unity.Entities;
using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Calculate anti-roll bar and apply correction forces to reduce body roll
    /// https://en.wikipedia.org/wiki/Anti-roll_bar
    /// </summary>
    public struct AntiRollBar : IComponentData
    {
        public float Stiffness;
        public Entity FrontRightWheel;
        public Entity FrontLeftWheel;
        public Entity RearRightWheel;
        public Entity RearLeftWheel;
    }

    public struct VehicleChassis : IComponentData
    {
        public CollisionCategories CollisionMask;
        [GhostField(Quantization = 10000)] public float DownForce;
    }
}