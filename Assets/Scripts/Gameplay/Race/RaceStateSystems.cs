using Dots.Racing;
using Unity.Burst;
using Unity.Entities.Racing.Common;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Play the race intro
    /// Place cars at each starting point
    /// Set race state.
    /// </summary>
    [BurstCompile]
    [UpdateAfter(typeof(UpdateTimerSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct RaceIntroSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
            state.RequireForUpdate<SpawnPoint>(); // TODO: Separate lobby and race spawn points
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;

            if (!race.IsRaceStarting)
            {
                return;
            }

            // we move the cars to the starting point
            var spawnPointBuffer = GetSingletonBuffer<SpawnPoint>();
            var index = 0;
            foreach (var player in Query<PlayerAspect>())
            {
                player.SetTargetTransform(spawnPointBuffer[index].TrackPosition, spawnPointBuffer[index].TrackRotation);
                index++;
                player.ResetVehicle();
                player.ResetLapProgress();
            }

            if (race.TimerFinished)
            {
                race.SetRaceState(RaceState.CountDown);
                var playersInRace = 0;
                foreach (var player in Query<RefRO<Player>>())
                {
                    if (player.ValueRO.State == PlayerState.StartingRace)
                    {
                        playersInRace++;
                    }
                }

                race.PlayersInRace = playersInRace;

                // Change state of the players and count players in race
                var changePlayerStateJob = new ChangePlayerStateJob
                {
                    CurrentState = PlayerState.StartingRace,
                    TargetState = PlayerState.Countdown
                };
                state.Dependency = changePlayerStateJob.ScheduleParallel(state.Dependency);
            }

            SetSingleton(race);
        }
    }

    /// <summary>
    /// Change the state for the race when countdown has finished
    /// </summary>
    [BurstCompile]
    [UpdateBefore(typeof(RaceIntroSystem))]
    [UpdateAfter(typeof(UpdateTimerSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct RaceCountdownSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;

            if (!race.IsCountDown)
            {
                return;
            }

            if (race.TimerFinished)
            {
                race.SetRaceState(RaceState.InProgress);
                race.InitialTime = state.WorldUnmanaged.Time.ElapsedTime;

                // Change all the players state
                var changePlayerStateJob = new ChangePlayerStateJob
                {
                    CurrentState = PlayerState.Countdown,
                    TargetState = PlayerState.Race
                };
                state.Dependency = changePlayerStateJob.ScheduleParallel(state.Dependency);
            }

            SetSingleton(race);
        }
    }

    /// <summary>
    /// Handles the events when the Player Finishes the race
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateBefore(typeof(UpdateTimerSystem))]
    public partial struct RacePlayerMonitorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;
            if (!race.IsInProgress)
            {
                return;
            }

            var playersFinished = 0;
            foreach (var car in Query<PlayerAspect>())
            {
                if (car.Player.HasFinished)
                {
                    playersFinished++;
                }
            }

            if (playersFinished <= 0)
            {
                return;
            }

            race.PlayersFinished = playersFinished;
            race.SetRaceState(RaceState.Finishing);
            SetSingleton(race);
        }
    }

    /// <summary>
    /// Update Finish Timers in the Server
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(UpdateTimerSystem))]
    public partial struct RaceFinishSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;
            if (!race.IsFinishing)
            {
                return;
            }

            if (race.TimerFinished || race.DidAllPlayersFinish)
            {
                // Force all the players who still in RACE to FINISHED
                var forcePlayersToFinishJob = new ChangePlayerStateJob
                {
                    CurrentState = PlayerState.Race,
                    TargetState = PlayerState.Finished
                };
                state.Dependency = forcePlayersToFinishJob.ScheduleParallel(state.Dependency);
                race.SetRaceState(RaceState.Finished);
            }

            SetSingleton(race);
        }
    }

    /// <summary>
    /// Update the car's timer celebration idle.
    /// Set the car's state to Finish if timer is over.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(UpdateTimerSystem))]
    public partial struct PlayerCelebrationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;
            if (!race.IsInProgress)
            {
                return;
            }

            foreach (var player in Query<PlayerAspect>())
            {
                if (player.FinishedCelebration)
                {
                    player.SetFinishedRace();
                }
                else if (player.IsCelebrating)
                {
                    player.ReduceCelebrationTimer(state.WorldUnmanaged.Time.DeltaTime);
                }
            }
        }
    }
}