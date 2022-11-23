using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Entities.SystemAPI;

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[CreateAfter(typeof(GhostPredictionSmoothingSystem))]
public unsafe partial struct VehicleSmoothingSystem : ISystem
{
     public void OnCreate(ref SystemState state)
     {
         GetSingletonRW<GhostPredictionSmoothing>().ValueRW.RegisterSmoothingAction<LocalTransform>(state.EntityManager, DefaultTranslationSmoothingAction.Action);
         GetSingletonRW<GhostPredictionSmoothing>().ValueRW.RegisterSmoothingAction<PhysicsVelocity>(state.EntityManager, new PortableFunctionPointer<GhostPredictionSmoothing.SmoothingActionDelegate>(PhysicsVelocitySmoothing));
         GetSingletonRW<GhostPredictionSmoothing>().ValueRW.RegisterSmoothingAction<Suspension>(state.EntityManager, new PortableFunctionPointer<GhostPredictionSmoothing.SmoothingActionDelegate>(SuspensionSmoothing));
         GetSingletonRW<GhostPredictionSmoothing>().ValueRW.RegisterSmoothingAction<Wheel>(state.EntityManager, new PortableFunctionPointer<GhostPredictionSmoothing.SmoothingActionDelegate>(WheelSmoothing));
         GetSingletonRW<GhostPredictionSmoothing>().ValueRW.RegisterSmoothingAction<WheelHitData>(state.EntityManager, new PortableFunctionPointer<GhostPredictionSmoothing.SmoothingActionDelegate>(WheelHitSmoothing));
     }

     [BurstCompile(DisableDirectCall = true)]
     static void SuspensionSmoothing(IntPtr currentData, IntPtr previousData, IntPtr usrData)
     {
         ref var current = ref UnsafeUtility.AsRef<Suspension>((void*)currentData);
         ref var previous = ref UnsafeUtility.AsRef<Suspension>((void*)previousData);
         current.Lerp(previous, 0.7f);
     }
     
     [BurstCompile(DisableDirectCall = true)]
      static void WheelSmoothing(IntPtr currentData, IntPtr previousData, IntPtr usrData)
      {
          ref var current = ref UnsafeUtility.AsRef<Wheel>((void*)currentData);
          ref var previous = ref UnsafeUtility.AsRef<Wheel>((void*)previousData);
          current.Lerp(previous, 0.7f);
      }
      
     [BurstCompile(DisableDirectCall = true)]
     static void PhysicsVelocitySmoothing(IntPtr currentData, IntPtr previousData, IntPtr usrData)
     {
         ref var current = ref UnsafeUtility.AsRef<PhysicsVelocity>((void*)currentData);
         ref var previous = ref UnsafeUtility.AsRef<PhysicsVelocity>((void*)previousData);
         var delta = 0.7f;
         current.Angular =  math.lerp(current.Angular , previous.Angular, delta);
         current.Linear = math.lerp(current.Linear, previous.Linear, delta);
     }
     
     [BurstCompile(DisableDirectCall = true)]
     static void WheelHitSmoothing(IntPtr currentData, IntPtr previousData, IntPtr usrData)
     {
         ref var current = ref UnsafeUtility.AsRef<WheelHitData>((void*)currentData);
         ref var previous = ref UnsafeUtility.AsRef<WheelHitData>((void*)previousData);
         current.Lerp(previous, 0.7f);
     }

     public void OnUpdate(ref SystemState state)
     {
     }

     public void OnDestroy(ref SystemState state)
     {
     }
}