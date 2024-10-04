using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
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
    [WithAll(typeof(GhostOwner))]
    [WithAll(typeof(Player))]
    [WithAll(typeof(Rank))]
    public partial struct DisableSmoothingJob : IJobEntity
    {
        private void Execute(ref PhysicsGraphicalSmoothing physicsGraphicalSmoothing, in Reset reset)
        {
            if (reset.Transform)
                physicsGraphicalSmoothing.ApplySmoothing = 0;
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    [WithAll(typeof(GhostOwner))]
    [WithAll(typeof(Player))]
    [WithAll(typeof(Rank))]
    public partial struct TeleportPlayersJob : IJobEntity
    {
        private void Execute(ref LocalTransform localTransform, 
            ref PhysicsVelocity velocity,
            ref Reset reset)
        {
            if (!reset.Transform)
            {
                return;
            }
            
            localTransform.Position = reset.TargetPosition;
            localTransform.Rotation = reset.TargetRotation;
            
            velocity.Linear = float3.zero;
            velocity.Angular = float3.zero;
            reset.Transform = false;
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
            foreach (var (reset, entity) in Query<RefRW<Reset>>()
                         .WithEntityAccess()
                         .WithAll<GhostOwner>()
                         .WithAll<Player>()
                         .WithAll<Rank>())
            {
                if (reset.ValueRO.Wheels)
                {
                    var resetWheelsJob = new ResetWheelsJob
                    {
                        Target = entity
                    };

                    state.Dependency = resetWheelsJob.ScheduleParallel(state.Dependency);
                    reset.ValueRW.Wheels = false;
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
                foreach (var (ghostOwner, reset) in 
                         Query<RefRO<GhostOwner>,RefRW<Reset>>()
                             .WithAll<Player>()
                             .WithAll<Rank>())
                {
                    if (ghostOwner.ValueRO.NetworkId == request.Id)
                    {
                        reset.ValueRW.ResetVehicle();
                    }
                }
            }

            state.EntityManager.DestroyEntity(m_ResetCarQuery);
        }
    }
}