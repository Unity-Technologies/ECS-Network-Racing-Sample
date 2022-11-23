using Unity.Burst;
using Unity.Entities;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    partial struct VehicleDebugSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = false; // Comment out to show forces debug lines 
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var wheel in Query<WheelDebugAspect>())
            {
                // Show wheel forces for debugging
                Debug.DrawRay(wheel.WheelHitData.WheelCenter, wheel.Suspension.SuspensionForce * wheel.Transform.Up,
                    Color.blue);
                Debug.DrawRay(wheel.WheelHitData.WheelCenter, wheel.Wheel.DriveForce * wheel.Transform.Forward,
                    Color.red);
                Debug.DrawRay(wheel.WheelHitData.WheelCenter, wheel.Wheel.SidewaysForce * wheel.Transform.Right,
                    Color.green);
            }
        }
    }
}