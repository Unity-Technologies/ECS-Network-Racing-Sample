using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SetPlayersReady : ISystem
    {
        private EntityQuery m_PlayerReadyQuery;

        public void OnCreate(ref SystemState state)
        {
            m_PlayerReadyQuery = state.GetEntityQuery(ComponentType.ReadOnly<PlayersReadyRPC>(),
                ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>());
            state.RequireForUpdate(m_PlayerReadyQuery);
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Set RaceState to ready to race
            var race = GetSingletonRW<Race>().ValueRW;
            if (race.State is RaceState.Lobby or RaceState.StartingRaceAutomatically)
            {
                race.SetRaceState(RaceState.ReadyToRace);
                SetSingleton(race);
            }

            // Change all the players state
            var changePlayerStateJob = new ChangePlayerStateJob
            {
                CurrentState = PlayerState.Lobby,
                TargetState = PlayerState.ReadyToRace
            };
            state.Dependency = changePlayerStateJob.ScheduleParallel(state.Dependency);
            state.EntityManager.DestroyEntity(m_PlayerReadyQuery);
        }
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(UpdateTimerSystem))]
    public partial struct PlayerReadyCountdownController : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;
            if (race.State != RaceState.ReadyToRace)
                return;

            race.CurrentTimer -= Time.DeltaTime;
            if (race.CurrentTimer <= 0)
            {
                // Reset timer
                race.SetRaceState(RaceState.StartingRace);

                // Change all the players state
                var changePlayerStateJob = new ChangePlayerStateJob
                {
                    CurrentState = PlayerState.ReadyToRace,
                    TargetState = PlayerState.StartingRace
                };
                state.Dependency = changePlayerStateJob.ScheduleParallel(state.Dependency);
            }

            SetSingleton(race);
        }
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct CancelPlayersReady : ISystem
    {
        private EntityQuery m_CancelPlayerReadyQuery;

        public void OnCreate(ref SystemState state)
        {
            m_CancelPlayerReadyQuery = state.GetEntityQuery(ComponentType.ReadOnly<CancelPlayerReadyRPC>(),
                ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>());
            state.RequireForUpdate(m_CancelPlayerReadyQuery);
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Set RaceState to Wait for players
            var race = GetSingletonRW<Race>().ValueRW;
            race.SetRaceState(RaceState.Lobby);
            SetSingleton(race);

            // Change all the players state
            var changePlayerStateJob = new ChangePlayerStateJob
            {
                CurrentState = PlayerState.ReadyToRace,
                TargetState = PlayerState.Lobby
            };
            state.Dependency = changePlayerStateJob.ScheduleParallel(state.Dependency);
            state.EntityManager.DestroyEntity(m_CancelPlayerReadyQuery);
        }
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ResetCarSystem : ISystem
    {
        private EntityQuery m_ResetCarQuery;

        public void OnCreate(ref SystemState state)
        {
            m_ResetCarQuery = state.GetEntityQuery(ComponentType.ReadOnly<ResetCarRPC>(),
                ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>());
            state.RequireForUpdate(m_ResetCarQuery);
        }

        public void OnDestroy(ref SystemState state) { }

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