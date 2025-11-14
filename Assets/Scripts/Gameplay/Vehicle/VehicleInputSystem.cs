using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Vehicles;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    [BurstCompile(CompileSynchronously = false, DisableDirectCall = true, FloatMode = FloatMode.Default, FloatPrecision = FloatPrecision.Standard, DisableSafetyChecks = false)]
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

            foreach (var (input, owner, playerInput) in Query<RefRW<CarInput>, RefRO<GhostOwner>, RefRO<PlayerGameInput>>())
            {
                if (owner.ValueRO.NetworkId != networkId )
                {
                    continue;
                }

                input.ValueRW.Horizontal = default;
                input.ValueRW.Vertical = default;

                var inputData = playerInput.ValueRO.GetInput();
                var vehicleBreak = inputData.Break.WasPressedThisFrame() ? 1f  : 0f;
                var handbreak = inputData.Handbrake.WasPressedThisFrame() ? 1f : 0f;
                var engineStartStop = inputData.EngineStartStop.WasPressedThisFrame();
                
                var move = inputData.Move.ReadValue<Vector2>();
                var horizontal = move.x;
                var vertical = move.y;
                
                if (UIMobileInput.Instance != null)
                {
                    horizontal += UIMobileInput.Instance.Horizontal;
                    vertical += UIMobileInput.Instance.Vertical;
                }

                horizontal = math.clamp(horizontal, -1, 1);
                vertical = math.clamp(vertical, -1, 1);

                input.ValueRW.Vertical          = vertical;
                input.ValueRW.Horizontal        = horizontal;
                input.ValueRW.Break             = vehicleBreak;
                input.ValueRW.Handbreak         = handbreak;
                input.ValueRW.EngineStartStop   = engineStartStop;

                playerInput.ValueRO.Free();
            }
        }
    }

    /// <summary>
    /// For more information, see the Vehicle package documentation: 
    /// https://docs.unity3d.com/Packages/com.unity.vehicles@0.1/manual/networking.html
    /// </summary>
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(PredictedFixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(CopyCommandBufferToInputSystemGroup))]
    [UpdateBefore(typeof(VehicleControlPredictionSystem))]
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

            var vehicleJobs = new PlayerVehicleControlJob { Race = race };
            state.Dependency = vehicleJobs.ScheduleParallel (state.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct PlayerVehicleControlJob : IJobEntity
    {
        [ReadOnly] public Race Race;
        void Execute(ref CarInput playerInputs, ref VehicleControl vehicleControl)
        {
            vehicleControl.RawThrottleInput = default;
            vehicleControl.RawBrakeInput    = playerInputs.Break;
            vehicleControl.HandbrakeInput   = playerInputs.Handbreak;
            vehicleControl.RawSteeringInput = playerInputs.Horizontal;
            vehicleControl.EngineStartStopInput = playerInputs.EngineStartStop;

            if (playerInputs.Vertical > 0) 
                vehicleControl.RawThrottleInput = playerInputs.Vertical;
            else
                vehicleControl.RawBrakeInput = math.abs(playerInputs.Vertical);
        }
    }
}