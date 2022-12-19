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

    /// <summary>
    /// Stores data for the chassis and physics forces.
    /// Quantized or unquantized. Where quantized means a float value is sent as an int with a certain multiplication factor which sets the precision (12.456789 can be sent as 12345 with a quantization factor of 1000).
    /// Smoothing method as clamp or interpolate/extrapolate.Meaning the value can be applied from a snapshot as interpolated/extrapolated or unmodified directly(clamped).
    /// </summary>
    public struct VehicleChassis : IComponentData
    {
        public CollisionCategories CollisionMask;
        [GhostField(Quantization = 10000)] public float DownForce;
    }
}