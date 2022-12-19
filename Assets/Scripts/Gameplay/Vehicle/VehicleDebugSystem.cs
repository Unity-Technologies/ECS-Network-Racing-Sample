using Unity.Entities.Racing.Common;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Draws vectors of the wheels' forces 
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    internal partial struct VehicleDebugSystem : ISystem
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