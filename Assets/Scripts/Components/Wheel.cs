using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Stores wheel data such as, visuals, rotation, size...
    /// </summary>
    [GhostComponent(SendDataForChildEntity = true)]
    public struct Wheel : IComponentData
    {
        public CollisionCategories CollisionMask;
        public float Radius;
        public WheelPlacement Placement;
        public float DriveTorque;
        public float MaxDriveTorque;
        public Entity VisualMesh;
        public float RotationAngle;
        public float GripFactor;
        public bool NeedsUpdate;
        [GhostField(Quantization = 10000)] public float SteeringAngle;
        [GhostField(Quantization = 10000)] public float DriveForce;
        [GhostField(Quantization = 10000)] public float SidewaysForce;
        public BlobAssetReference<AnimationCurveBlob> DriveTorqueCurve;

        public bool IsRear => Placement is WheelPlacement.RearLeft or WheelPlacement.RearRight;

        public void Reset()
        {
            RotationAngle = 0;
            SteeringAngle = 0;
            SidewaysForce = 0;
            DriveForce = 0;
        }

        public void Lerp(Wheel newData, float delta)
        {
            RotationAngle = math.lerp(RotationAngle , newData.RotationAngle, delta);
            SteeringAngle = math.lerp(SteeringAngle , newData.SteeringAngle, delta);
            SidewaysForce = math.lerp(SidewaysForce , newData.SidewaysForce, delta);
            DriveForce = (DriveForce + newData.DriveForce) * delta;
        }
    }
    /// <summary>
    /// Stores data for physics Steering.
    /// </summary>
    [GhostComponent(SendDataForChildEntity = true)]
    public struct Steering : IComponentData
    {
        public float TurnRadius;
        public float SteeringForce;
        public float SteeringTime;
        public float WheelsBase;
        public float RearTrack;

        public float CalculateSteeringAngle(float amount, int dir)
        {
            return math.atan(WheelsBase / (TurnRadius + dir * RearTrack / 2)) * amount * SteeringForce;
        }
    }

    public enum WheelPlacement
    {
        FrontRight,
        FrontLeft,
        RearRight,
        RearLeft
    }

    /// <summary>
    /// Based on Physics Category Names.
    /// </summary>
    [Flags]
    public enum CollisionCategories
    {
        Static = 0x1,
        Dynamic = 0x2,
        Triggers = 0x4,
        Car = 0x8,
    }

    /// <summary>
    /// Keeps the driving physics parameters.
    /// </summary>
    public struct WheelDriveControls : IComponentData
    {
        public float DriveAmount;
        public float SteerAmount;

        public void Reset()
        {
            DriveAmount = 0;
            SteerAmount = 0;
        }
    }

    /// <summary>
    /// Stores suspension parameters for physics
    /// </summary>
    [GhostComponent(SendDataForChildEntity = true)]
    public struct Suspension : IComponentData
    {
        public float RestLength;
        public float SpringStiffness;
        public float DamperStiffness;
        [GhostField(Quantization = 100000)] public float SuspensionForce;
        [GhostField(Quantization = 100000)] public float SpringLength;
        [GhostField(Quantization = 100000)] public float SpringForce;
        [GhostField(Quantization = 100000)] public float DamperForce;

        public void Reset()
        {
            SuspensionForce = 0;
            SpringLength = RestLength;
            SpringForce = 0;
            DamperForce = 0;
        }

        public void Lerp(Suspension newSuspension, float delta)
        {
            SuspensionForce = math.lerp(SuspensionForce , newSuspension.SpringForce, delta);
            SpringLength = math.lerp(SpringLength , newSuspension.SpringLength, delta);
            SpringForce = math.lerp(SpringForce , newSuspension.SpringForce, delta);
            DamperForce = math.lerp(DamperForce , newSuspension.DamperForce, delta);
        }
    }

    /// <summary>
    /// Stores all hit collision data for Wheels.
    /// </summary>
    [GhostComponent(SendDataForChildEntity = true)]
    public struct WheelHitData : IComponentData
    {
        public float3 Origin;
        public float3 WheelCenter;
        public float3 Position;
        public float SurfaceFriction;
        public float3 Velocity;
        public bool HasHit;

        public void Reset()
        {
            Origin = float3.zero;
            WheelCenter = float3.zero;
            Position = float3.zero;
            SurfaceFriction = 0;
            HasHit = false;
            Velocity = float3.zero;
        }

        public void Lerp(WheelHitData newHitData, float delta)
        {
            Origin = math.lerp(Origin, newHitData.Origin, delta);
            WheelCenter = math.lerp(WheelCenter, newHitData.WheelCenter, delta);
            Position = math.lerp(Position, newHitData.Position, delta);
            SurfaceFriction = math.lerp(SurfaceFriction, newHitData.SurfaceFriction, delta);
            Velocity = math.lerp(Velocity, newHitData.Velocity, delta);
        }
    }

    /// <summary>
    /// Assign the chassis entity to the wheel for later processing
    /// </summary>
    public struct ChassisReference : IComponentData
    {
        public Entity Value;
    }

    public readonly partial struct WheelAspect : IAspect
    {
        readonly RefRW<Wheel> m_Wheel;
        readonly RefRO<ChassisReference> m_ChassisReference;
        public Wheel Wheel => m_Wheel.ValueRO;
        public Entity ChassisReference => m_ChassisReference.ValueRO.Value;

        public void SetVisualMesh(Entity value)
        {
            m_Wheel.ValueRW.VisualMesh = value;
        }
    }

    /// <summary>
    /// Access all components required for debuging wheels.
    /// </summary>
    public readonly partial struct WheelDebugAspect : IAspect
    {
        public readonly TransformAspect Transform;
        readonly RefRO<Wheel> m_Wheel;
        readonly RefRO<Suspension> m_Suspension;
        readonly RefRO<WheelHitData> m_WheelHitData;
        public Wheel Wheel => m_Wheel.ValueRO;
        public Suspension Suspension => m_Suspension.ValueRO;
        public WheelHitData WheelHitData => m_WheelHitData.ValueRO;
    }
}