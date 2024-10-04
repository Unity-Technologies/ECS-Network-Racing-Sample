using Unity.Burst;
using Unity.Entities.Racing.Common;
using Unity.NetCode;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Evaluates current race state and set player state
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SetPlayerReadySystem : ISystem
    {
        private EntityQuery m_PlayerReadyQuery;

        public void OnCreate(ref SystemState state)
        {
            m_PlayerReadyQuery = state.GetEntityQuery(ComponentType.ReadOnly<PlayersReadyRPC>(),
                ComponentType.ReadOnly<ReceiveRpcCommandRequest>());
            state.RequireForUpdate(m_PlayerReadyQuery);
            state.RequireForUpdate<Race>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Set RaceState to ready to race
            var race = GetSingletonRW<Race>().ValueRW;
            if (race.CanJoinRace)
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

    /// <summary>
    /// Evaluates the timer for going from lobby to race
    /// Set state for race component.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(UpdateTimerSystem))]
    public partial struct LobbyToRaceCountdownSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;
            if (!race.IsReadyToRace)
                return;
            
            if (race.TimerFinished)
            {
                // Reset timer
                race.StartRace();

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
    
    /// <summary>
    /// Returns the state from Race to Lobby 
    /// That cancels going to the race
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct CancelPlayersReady : ISystem
    {
        private EntityQuery m_CancelPlayerReadyQuery;

        public void OnCreate(ref SystemState state)
        {
            m_CancelPlayerReadyQuery = state.GetEntityQuery(ComponentType.ReadOnly<CancelPlayerReadyRPC>(),
                ComponentType.ReadOnly<ReceiveRpcCommandRequest>());
            state.RequireForUpdate(m_CancelPlayerReadyQuery);
            state.RequireForUpdate<Race>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Set RaceState to Wait for players
            var race = GetSingletonRW<Race>().ValueRW;
            race.CancelRaceStart();
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
    
    /// <summary>
    /// Moves the cars after race has finished to the Lobby.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct TeleportPlayerToLobbySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();

            // If the race is not finished, skip this
            if (race.NotStarted)
            {
                return;
            }

            var spawnPointBuffer = GetSingletonBuffer<SpawnPoint>();
            var index = 0;
            foreach (var (car, reset) 
                     in Query<RefRO<Player>, RefRW<Reset>>()
                         .WithAll<GhostOwner>()
                         .WithAll<Player>()
                         .WithAll<Rank>())
            {
                if (car.ValueRO.HasFinished)
                {
                    reset.ValueRW.ResetVehicle();
                    reset.ValueRW.SetTargetTransform(spawnPointBuffer[index].LobbyPosition, spawnPointBuffer[index].LobbyRotation);
                }
                index++;
            }
        }
    }
}