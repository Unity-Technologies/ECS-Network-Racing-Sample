using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.NetCode;
using Unity.Physics.GraphicsIntegration;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Disable all physics of the wheels to teleport the players
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    [WithAll(typeof(VehicleChassis))]
    public partial struct DisableSmoothingJob : IJobEntity
    {
        private void Execute(ref PhysicsGraphicalSmoothing physicsGraphicalSmoothing, PlayerAspect playerAspect)
        {
            if (playerAspect.Reset.Transform)
                physicsGraphicalSmoothing.ApplySmoothing = 0;
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct TeleportPlayersJob : IJobEntity
    {
        private void Execute(PlayerAspect player)
        {
            if (!player.Reset.Transform)
            {
                return;
            }

            player.ResetPlayer();
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct ResetWheelsJob : IJobEntity
    {
        public Entity Target;

        private void Execute(in ChassisReference chassisReference, ref Wheel wheel, ref Suspension suspension,
            ref WheelHitData wheelHitData)
        {
            if (Target != Entity.Null && chassisReference.Value != Target)
            {
                return;
            }

            wheel.Reset();
            suspension.Reset();
            wheelHitData.Reset();
        }
    }

    [UpdateAfter(typeof(TeleportCarSystem))]
    public partial struct ResetWheelsSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Change state of the players and count players in race
            foreach (var playerAspect in Query<PlayerAspect>())
            {
                if (playerAspect.Reset.Wheels)
                {
                    var resetWheelsJob = new ResetWheelsJob
                    {
                        Target = playerAspect.Self
                    };

                    state.Dependency = resetWheelsJob.ScheduleParallel(state.Dependency);
                    playerAspect.SetPlayerWheelsReady();
                }
            }
        }
    }

    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial struct DisableSmoothingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var disableSmoothingJob = new DisableSmoothingJob();
            disableSmoothingJob.ScheduleParallel();
        }
    }

    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial struct TeleportCarSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var teleportPlayersJob = new TeleportPlayersJob();
            state.Dependency = teleportPlayersJob.ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }
    }

    [UpdateBefore(typeof(TeleportCarSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ResetCarSystem : ISystem
    {
        private EntityQuery m_ResetCarQuery;

        public void OnCreate(ref SystemState state)
        {
            m_ResetCarQuery = state.GetEntityQuery(ComponentType.ReadOnly<ResetCarRPC>(),
                ComponentType.ReadOnly<ReceiveRpcCommandRequest>());
            state.RequireForUpdate(m_ResetCarQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var requests = m_ResetCarQuery.ToComponentDataArray<ResetCarRPC>(Allocator.Temp);
            foreach (var request in requests)
            {
                foreach (var car in Query<PlayerAspect>())
                {
                    if (car.NetworkId == request.Id)
                    {
                        car.ResetVehicle();
                    }
                }
            }

            state.EntityManager.DestroyEntity(m_ResetCarQuery);
        }
    }
}