using System;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Smooth the forces between ghost and client.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [CreateAfter(typeof(GhostPredictionSmoothingSystem))]
    [BurstCompile]

    public unsafe partial struct VehicleSmoothingSystem : ISystem
    {
        private const float k_DefaultSmoothingFactor = 0.7f;
        private static readonly SharedStatic<float> s_SmoothingFactor = SharedStatic<float>.GetOrCreate<VehicleSmoothingSystem>();
        
        [Serializable]
        public struct SmoothingConfig : IComponentData
        {
            public float SmoothingFactor;
        }
        
        public void OnCreate(ref SystemState state)
        {
            // Initialize SmoothingConfig with default values
            var smoothingConfig = new SmoothingConfig { SmoothingFactor = k_DefaultSmoothingFactor };
            state.EntityManager.AddComponentData(state.SystemHandle, smoothingConfig);
            s_SmoothingFactor.Data = k_DefaultSmoothingFactor;
            
            GetSingletonRW<GhostPredictionSmoothing>().ValueRW
                .RegisterSmoothingAction<LocalTransform>(state.EntityManager, DefaultTranslationSmoothingAction.Action);
            GetSingletonRW<GhostPredictionSmoothing>().ValueRW.RegisterSmoothingAction<PhysicsVelocity>(
                state.EntityManager,
                new PortableFunctionPointer<GhostPredictionSmoothing.SmoothingActionDelegate>(
                    PhysicsVelocitySmoothing));
            GetSingletonRW<GhostPredictionSmoothing>().ValueRW.RegisterSmoothingAction<Suspension>(state.EntityManager,
                new PortableFunctionPointer<GhostPredictionSmoothing.SmoothingActionDelegate>(SuspensionSmoothing));
            GetSingletonRW<GhostPredictionSmoothing>().ValueRW.RegisterSmoothingAction<Wheel>(state.EntityManager,
                new PortableFunctionPointer<GhostPredictionSmoothing.SmoothingActionDelegate>(WheelSmoothing));
            GetSingletonRW<GhostPredictionSmoothing>().ValueRW.RegisterSmoothingAction<WheelHitData>(
                state.EntityManager,
                new PortableFunctionPointer<GhostPredictionSmoothing.SmoothingActionDelegate>(WheelHitSmoothing));
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var smoothingConfig = state.EntityManager.GetComponentData<SmoothingConfig>(state.SystemHandle);
            s_SmoothingFactor.Data = smoothingConfig.SmoothingFactor;
        }

        [BurstCompile(DisableDirectCall = true)]
        private static void SuspensionSmoothing(IntPtr currentData, IntPtr previousData, IntPtr usrData)
        {
            ref var current = ref UnsafeUtility.AsRef<Suspension>((void*)currentData);
            ref var previous = ref UnsafeUtility.AsRef<Suspension>((void*)previousData);
            current.Lerp(previous, s_SmoothingFactor.Data);
        }

        [BurstCompile(DisableDirectCall = true)]
        private static void WheelSmoothing(IntPtr currentData, IntPtr previousData, IntPtr usrData)
        {
            ref var current = ref UnsafeUtility.AsRef<Wheel>((void*)currentData);
            ref var previous = ref UnsafeUtility.AsRef<Wheel>((void*)previousData);
            current.Lerp(previous, s_SmoothingFactor.Data);
        }

        [BurstCompile(DisableDirectCall = true)]
        private static void PhysicsVelocitySmoothing(IntPtr currentData, IntPtr previousData, IntPtr usrData)
        {
            ref var current = ref UnsafeUtility.AsRef<PhysicsVelocity>((void*)currentData);
            ref var previous = ref UnsafeUtility.AsRef<PhysicsVelocity>((void*)previousData);
            current.Angular = math.lerp(current.Angular, previous.Angular, s_SmoothingFactor.Data);
            current.Linear = math.lerp(current.Linear, previous.Linear, s_SmoothingFactor.Data);
        }

        [BurstCompile(DisableDirectCall = true)]
        private static void WheelHitSmoothing(IntPtr currentData, IntPtr previousData, IntPtr usrData)
        {
            ref var current = ref UnsafeUtility.AsRef<WheelHitData>((void*)currentData);
            ref var previous = ref UnsafeUtility.AsRef<WheelHitData>((void*)previousData);
            current.Lerp(previous, s_SmoothingFactor.Data);
        }
    }
}