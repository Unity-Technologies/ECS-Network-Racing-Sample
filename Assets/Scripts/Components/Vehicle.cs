using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Vehicles;

namespace Unity.Entities.Racing.Common
{
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


    /// <summary>
    /// The ghost variants (VehicleControlData_GhostVariant, WheelOnVehicle_GhostVariant) are required to update wheel visuals.
    /// See: https://docs.unity3d.com/Packages/com.unity.vehicles@0.1/manual/networking.html 
    /// </summary>
    [GhostComponentVariation(typeof(VehicleControlData))]
    public struct VehicleControlData_GhostVariant
    {
        [GhostField()]
        public float SteeringPosition;
        [GhostField()]
        public int TransmissionCurrentGear;
        [GhostField()]
        public float TransmissionCurrentGearRatio;
        [GhostField()]
        public float EngineAngularVelocity;
        [GhostField()]
        public bool EngineIsRunning;
    }

    [GhostComponentVariation(typeof(WheelOnVehicle))]
    public struct WheelOnVehicle_GhostVariant
    {
        [GhostField(SendData = false)]
        public int WheelProtectorChildColliderIndex;
        [GhostField(SendData = false)]
        public RigidTransform SuspensionLocalTransform;
        [GhostField(SendData = false)]
        public Entity AxlePairedWheelEntity;
        [GhostField(SendData = false)]
        public int AxlePairedWheelIndex;
        [GhostField(SendData = false)]
        public Entity Entity;
        [GhostField(SendData = false)]
        public float MotorTorque;
        [GhostField(SendData = false)]
        public float BrakeTorque;
        [GhostField(SendData = false)]
        public float AngularVelocityLimit;

        [GhostField()]
        public float SteerAngle;
        [GhostField()]
        public float AngularVelocity;
        [GhostField()]
        public float RotationAngle;
        [GhostField()]
        public float SuspensionLength;
        [GhostField(SendData = false)]
        public float VisualSuspensionLength;

        [GhostField(SendData = false)]
        public bool IsGrounded;
        [GhostField(SendData = false)]
        public ColliderCastHit WheelHit;
        [GhostField(SendData = false)]
        public float3 SuspensionImpulse;
        [GhostField(SendData = false)]
        public float3 FrictionImpulse;
        [GhostField()]
        public float2 FrictionSlip;
        [GhostField(SendData = false)]
        public float2 FrictionSpeed;

        [GhostField()]
        public bool StaticFrictionReferenceIsSet;
        [GhostField()]
        public float3 StaticFrictionRefPosition;
        [GhostField(SendData = false)]
        public bool DisableStaticFrictionSingleFrame;
    }

}