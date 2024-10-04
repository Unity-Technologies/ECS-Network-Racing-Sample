using Unity.Entities.Racing.Common;
using Unity.Transforms;
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

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (wheel, wheelHitData, transform, suspension ) 
                     in Query<RefRO<Wheel>,RefRO<WheelHitData>, RefRO<LocalTransform>, RefRO<Suspension>>())
            {
                // Show wheel forces for debugging
                Debug.DrawRay(wheelHitData.ValueRO.WheelCenter, suspension.ValueRO.SuspensionForce * transform.ValueRO.Up(), Color.blue);
                Debug.DrawRay(wheelHitData.ValueRO.WheelCenter, wheel.ValueRO.DriveForce * transform.ValueRO.Forward(), Color.red);
                Debug.DrawRay(wheelHitData.ValueRO.WheelCenter, wheel.ValueRO.SidewaysForce * transform.ValueRO.Right(), Color.green);
            }
        }
    }
}