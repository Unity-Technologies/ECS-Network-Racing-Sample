using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics.Systems;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Process user's input to store the result in a component.
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct WheelsVehicleInputJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<CarInput> CarInputs;
        [ReadOnly] public Race Race;

        private void Execute(in ChassisReference chassisReference, ref WheelDriveControls driveControls)
        {
            if (!CarInputs.HasComponent(chassisReference.Value))
            {
                return;
            }

            // Ignore input if we are in CountDown
            if (Race.State is RaceState.CountDown or RaceState.Leaderboard or RaceState.StartingRace)
            {
                driveControls.Reset();
            }
            else
            {
                driveControls.DriveAmount = CarInputs[chassisReference.Value].Vertical;
                driveControls.SteerAmount = CarInputs[chassisReference.Value].Horizontal;
            }
        }
    }

    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial struct CarInputSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CarInput>();
            state.RequireForUpdate<NetworkId>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var networkId = GetSingleton<NetworkId>().Value;

            foreach (var (input, owner) in Query<RefRW<CarInput>, RefRO<GhostOwner>>())
            {
                if (owner.ValueRO.NetworkId != networkId)
                {
                    continue;
                }

                input.ValueRW.Horizontal = default;
                input.ValueRW.Vertical = default;

                // Instead of adding values, take the maximum absolute value and preserve its sign
                float maxHorizontal = 0;
                float maxVertical = 0;
                
                // Process keyboard input
                float horizontal = Input.GetAxis("Horizontal");
                UpdateMaxAbsoluteValue(ref maxHorizontal, horizontal);
                
                // Process steering input
                float steer = Input.GetAxis("Steer");
                UpdateMaxAbsoluteValue(ref maxHorizontal, steer);

                // Process vertical movement input
                float vertical = Input.GetAxis("Vertical");
                UpdateMaxAbsoluteValue(ref maxVertical, vertical);
                
                // Process drive input
                float drive = Input.GetAxis("Drive");
                UpdateMaxAbsoluteValue(ref maxVertical, drive);
                
                // Process drive triggers input
                float driveTriggers = Input.GetAxis("DriveTriggers");
                UpdateMaxAbsoluteValue(ref maxVertical, driveTriggers);
                
                // Process left stick Y input
                float leftStickY = Input.GetAxis("LeftStickY");
                UpdateMaxAbsoluteValue(ref maxVertical, leftStickY);
                
                // Process D-pad Y input
                float dPadY = Input.GetAxis("DPadY");
                UpdateMaxAbsoluteValue(ref maxVertical, dPadY);
                
                // Process mobile input if available
                if (UIMobileInput.Instance != null)
                {
                    UpdateMaxAbsoluteValue(ref maxHorizontal, UIMobileInput.Instance.Horizontal);
                    UpdateMaxAbsoluteValue(ref maxVertical, UIMobileInput.Instance.Vertical);
                }

                // Assign final clamped values
                input.ValueRW.Horizontal = math.clamp(maxHorizontal, -1, 1);
                input.ValueRW.Vertical = math.clamp(maxVertical, -1, 1);
            }
        }
        
        // Helper method to update the maximum absolute value while preserving the sign
        private static void UpdateMaxAbsoluteValue(ref float currentMax, float newValue)
        {
            if (math.abs(newValue) > math.abs(currentMax))
            {
                currentMax = newValue;
            }
        }
    }

    [UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
    [BurstCompile]
    public partial struct ProcessVehicleWheelsInputSystem : ISystem
    {
        private ComponentLookup<CarInput> m_CarInputs;

        public void OnCreate(ref SystemState state)
        {
            m_CarInputs = state.GetComponentLookup<CarInput>(true);
            state.RequireForUpdate<Race>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.CompleteDependency();
            m_CarInputs.Update(ref state);

            var race = GetSingleton<Race>();

            var wheelsVehicleInputJob = new WheelsVehicleInputJob
            {
                CarInputs = m_CarInputs,
                Race = race
            };

            state.Dependency = wheelsVehicleInputJob.ScheduleParallel(state.Dependency);
        }
    }
}