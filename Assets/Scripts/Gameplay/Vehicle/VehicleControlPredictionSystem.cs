using Unity.Burst;
using Unity.NetCode;
using Unity.Physics;
using Unity.Vehicles;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// For more information, see the Vehicle package documentation: 
    /// https://docs.unity3d.com/Packages/com.unity.vehicles@0.1/manual/networking.html
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(PredictedFixedStepSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct VehicleControlPredictionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            VehicleControlSystem.VehicleControlJob job = new VehicleControlSystem.VehicleControlJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                WheelControlLookup = SystemAPI.GetComponentLookup<WheelControl>(true),
                EngineControlLookup = SystemAPI.GetComponentLookup<EngineStartStop>(false),
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);

            state.Dependency = new VehicleControlSystem.VehicleEngineControlJob
            {
                EngineControlLookup = SystemAPI.GetComponentLookup<EngineStartStop>(false),
            }.Schedule(state.Dependency);
        }
    }

}