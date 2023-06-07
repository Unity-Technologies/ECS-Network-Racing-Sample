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

                // TODO: Since DOTS still does not yet support the new Unity input system, we have different axes for the Gamepad
                var horizontal = Input.GetAxis("Horizontal");
                horizontal += Input.GetAxis("Steer");

                var vertical = Input.GetAxis("Vertical");
                vertical += Input.GetAxis("Drive");
                vertical += Input.GetAxis("DriveTriggers");
                vertical += Input.GetAxis("LeftStickY");
                vertical += Input.GetAxis("DPadY");
                
                if (UIMobileInput.Instance != null)
                {
                    horizontal += UIMobileInput.Instance.Horizontal;
                    vertical += UIMobileInput.Instance.Vertical;
                }

                horizontal = math.clamp(horizontal, -1, 1);
                vertical = math.clamp(vertical, -1, 1);

                input.ValueRW.Horizontal = horizontal;
                input.ValueRW.Vertical = vertical;
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