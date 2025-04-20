using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    public class VehicleAuthoring : MonoBehaviour
    {
        [Header("Suspension")] public float RestLength = 0.45f;
        public float SpringStiffness;
        public float DamperStiffness;

        [Header("Steering")] public float TurnRadius;
        public float SteeringForce = 2;
        public float SteeringTime = 10;

        [Header("AntiRollBar")] public float AntiRollStiffness = 5000;
        public float DownForce = 50;

        [Header("Acceleration")] public float DriveTorque = 50;
        public float MaxDriveTorque = 70;
        public AnimationCurve DriveTorqueCurve = new();
        public CollisionCategories BodyCollisionMask;

        [Header("Wheels")] public float WheelsRadius;
        public CollisionCategories WheelsCollisionMask;
        public float MaxSafeVelocity = 50f; // Maximum safe velocity for wheel raycast
        public WheelAuthoringHelper[] Wheels;

        [Header("Engine Sound")] public float MinAudioVolume = 0.4f;
        public float MaxAudioVolume = 1.0f;

        [Serializable]
        public class WheelAuthoringHelper
        {
            public GameObject WheelSpring;
            public GameObject WheelMesh;
            public WheelPlacement Placement;
            public bool IsSteering;
            [Range(0, 1)] public float GripFactor = 0.3f;
        }

        private void OnDrawGizmos()
        {
            foreach (var wheel in Wheels)
            {
                var pos = wheel.WheelSpring.transform.position;
                var wheelCenter = pos - RestLength * Vector3.up;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pos, pos - RestLength * Vector3.up);

                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(wheelCenter, WheelsRadius);
            }
        }
        
        private class Baker : Baker<VehicleAuthoring>
        {
            public override void Bake(VehicleAuthoring authoring)
            {
                var vehicleBakingData = new VehicleBakingData
                {
                    Authoring = authoring,
                    WheelVisual0 = GetEntity(authoring.Wheels[0].WheelMesh.gameObject, TransformUsageFlags.Dynamic),
                    WheelVisual1 = GetEntity(authoring.Wheels[1].WheelMesh.gameObject, TransformUsageFlags.Dynamic),
                    WheelVisual2 = GetEntity(authoring.Wheels[2].WheelMesh.gameObject, TransformUsageFlags.Dynamic),
                    WheelVisual3 = GetEntity(authoring.Wheels[3].WheelMesh.gameObject, TransformUsageFlags.Dynamic),
                    WheelSpring0 = GetEntity(authoring.Wheels[0].WheelSpring.gameObject, TransformUsageFlags.Dynamic),
                    WheelSpring1 = GetEntity(authoring.Wheels[1].WheelSpring.gameObject, TransformUsageFlags.Dynamic),
                    WheelSpring2 = GetEntity(authoring.Wheels[2].WheelSpring.gameObject, TransformUsageFlags.Dynamic),
                    WheelSpring3 = GetEntity(authoring.Wheels[3].WheelSpring.gameObject, TransformUsageFlags.Dynamic)
                };

                var vehicleEntity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(vehicleEntity, vehicleBakingData);

                AddComponent<LapProgress>(vehicleEntity);
                AddComponent<Skin>(vehicleEntity);
                AddComponent<PlayerName>(vehicleEntity);
                AddComponent<Rank>(vehicleEntity);
                AddComponent<Reset>(vehicleEntity);

                AddComponent(vehicleEntity, new VolumeData
                {
                    Min = authoring.MinAudioVolume,
                    Max = authoring.MaxAudioVolume,
                });
            }
        }
    }

    [TemporaryBakingType]
    public struct VehicleBakingData : IComponentData
    {
        public UnityObjectRef<VehicleAuthoring> Authoring;
        public Entity WheelVisual0;
        public Entity WheelVisual1;
        public Entity WheelVisual2;
        public Entity WheelVisual3;

        public Entity WheelSpring0;
        public Entity WheelSpring1;
        public Entity WheelSpring2;
        public Entity WheelSpring3;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    public partial class VehicleBaker : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithEntityQueryOptions(EntityQueryOptions.IncludePrefab)
                .WithStructuralChanges()
                .ForEach((Entity entity, ref VehicleBakingData vehicleBakingData) =>
                {
                    var vehicleAuthoring = vehicleBakingData.Authoring.Value;
                    var chassis = new ChassisReference { Value = entity };
                    var suspension = new Suspension
                    {
                        RestLength = vehicleAuthoring.RestLength,
                        SpringStiffness = vehicleAuthoring.SpringStiffness,
                        DamperStiffness = vehicleAuthoring.DamperStiffness,
                        SpringLength = vehicleAuthoring.RestLength
                    };
                    //Setup steering 
                    var wheelsBase = GetWheelsBaseDistance(vehicleAuthoring.Wheels);
                    var rearTrack = GetRearTrackDistance(vehicleAuthoring.Wheels);
                    var antiRoll = new AntiRollBar { Stiffness = vehicleAuthoring.AntiRollStiffness };
                    var driveTorqueCurve =
                        AnimationCurveBlob.CreateBlob(vehicleAuthoring.DriveTorqueCurve, Allocator.Persistent);
                    var vehicleChassis = new VehicleChassis
                    {
                        DownForce = vehicleAuthoring.DownForce,
                        CollisionMask = vehicleAuthoring.BodyCollisionMask
                    };
                    for (var i = 0; i < vehicleAuthoring.Wheels.Length; i++)
                    {
                        var wheelAuthoring = vehicleAuthoring.Wheels[i];

                        var visualMesh = i switch
                        {
                            0 => vehicleBakingData.WheelVisual0,
                            1 => vehicleBakingData.WheelVisual1,
                            2 => vehicleBakingData.WheelVisual2,
                            3 => vehicleBakingData.WheelVisual3,
                            _ => vehicleBakingData.WheelVisual0
                        };

                        var wheel = new Wheel
                        {
                            CollisionMask = vehicleAuthoring.WheelsCollisionMask,
                            Radius = vehicleAuthoring.WheelsRadius,
                            DriveTorque = vehicleAuthoring.DriveTorque,
                            MaxDriveTorque = vehicleAuthoring.MaxDriveTorque,
                            DriveTorqueCurve = driveTorqueCurve,
                            GripFactor = wheelAuthoring.GripFactor,
                            Placement = wheelAuthoring.Placement,
                            VisualMesh = visualMesh,
                            MaxSafeVelocity = vehicleAuthoring.MaxSafeVelocity,
                        };

                        var wheelEntity = i switch
                        {
                            0 => vehicleBakingData.WheelSpring0,
                            1 => vehicleBakingData.WheelSpring1,
                            2 => vehicleBakingData.WheelSpring2,
                            3 => vehicleBakingData.WheelSpring3,
                            _ => vehicleBakingData.WheelSpring0
                        };

                        if (wheelAuthoring.IsSteering)
                        {
                            var steering = new Steering
                            {
                                TurnRadius = vehicleAuthoring.TurnRadius,
                                SteeringForce = vehicleAuthoring.SteeringForce,
                                WheelsBase = wheelsBase,
                                RearTrack = rearTrack,
                                SteeringTime = vehicleAuthoring.SteeringTime,
                            };
                            EntityManager.AddComponentData(wheelEntity, steering);
                        }

                        switch (wheelAuthoring.Placement)
                        {
                            case WheelPlacement.FrontRight:
                                antiRoll.FrontRightWheel = wheelEntity;
                                break;
                            case WheelPlacement.FrontLeft:
                                antiRoll.FrontLeftWheel = wheelEntity;
                                break;
                            case WheelPlacement.RearRight:
                                antiRoll.RearRightWheel = wheelEntity;
                                break;
                            case WheelPlacement.RearLeft:
                                antiRoll.RearLeftWheel = wheelEntity;
                                break;
                        }

                        EntityManager.AddComponentData(wheelEntity, wheel);
                        EntityManager.AddComponentData(wheelEntity, suspension);
                        EntityManager.AddComponentData(wheelEntity, chassis);
                        EntityManager.AddComponent<WheelDriveControls>(wheelEntity);
                        EntityManager.AddComponent<WheelHitData>(wheelEntity);
                    }

                    EntityManager.AddComponentData(entity, antiRoll);
                    EntityManager.AddComponentData(entity, vehicleChassis);
                }).Run();
        }

        // Calculates the distance between front and rear wheels 
        private float GetWheelsBaseDistance(VehicleAuthoring.WheelAuthoringHelper[] wheels)
        {
            var frontLeft = wheels.First(w => w.Placement == WheelPlacement.FrontLeft);
            var rearLeft = wheels.First(w => w.Placement == WheelPlacement.RearLeft);
            return math.distance(frontLeft.WheelSpring.transform.position, rearLeft.WheelSpring.transform.position);
        }

        // Calculates the distance between the rear wheels  
        private float GetRearTrackDistance(VehicleAuthoring.WheelAuthoringHelper[] wheels)
        {
            var rearRight = wheels.First(w => w.Placement == WheelPlacement.RearRight);
            var rearLeft = wheels.First(w => w.Placement == WheelPlacement.RearLeft);
            return math.distance(rearLeft.WheelSpring.transform.position, rearRight.WheelSpring.transform.position);
        }
    }
}