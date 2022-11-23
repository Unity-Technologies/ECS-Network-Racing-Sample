using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.GraphicsIntegration;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using Unity.Entities.Racing.Common;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    [WithAll(typeof(VehicleChassis))]
    public partial struct DisableSmoothingJob : IJobEntity
    {
        private void Execute(ref PhysicsGraphicalSmoothing physicsGraphicalSmoothing, in PlayerAspect playerAspect)
        {
            if (playerAspect.Reset.Transform)
                physicsGraphicalSmoothing.ApplySmoothing = 0;
        }
    }
    
    [BurstCompile]
    [UpdateAfter(typeof(UpdateTimerSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]

    public partial struct IntroRaceSystem : ISystem
    {
        private bool m_PlayersSpawned;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
            state.RequireForUpdate<SpawnPoint>();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingletonRW<Race>().ValueRW;

            if (race.State != RaceState.StartingRace)
                return;

            race.CurrentTimer -= SystemAPI.Time.DeltaTime;

            // Change state before the timer finishes to handle the transitions better
            if (race.CurrentTimer < 4.5f && !m_PlayersSpawned && race.CurrentTimer >2)
            {
                // After waiting for the countdown, we move the cars to the starting point
                var spawnPointBuffer = GetSingletonBuffer<SpawnPoint>();
                var index = 0;
                foreach (var player in Query<PlayerAspect>())
                {
                    player.SetTargetTransform(spawnPointBuffer[index].TrackPosition, spawnPointBuffer[index].TrackRotation);
                    index++;
                    player.ResetVehicle();
                    player.ResetLapProgress();
                }
            }

            if (race.CurrentTimer < 0)
            {
                race.SetRaceState(RaceState.CountDown);
                m_PlayersSpawned = false;

                // Set Players in the Race
                var playerCount = 0;

                // Change state of the players and count players in race
                foreach (var player in Query<RefRW<Player>>())
                {
                    if (player.ValueRO.State == PlayerState.StartingRace)
                    {
                        player.ValueRW.State = PlayerState.Countdown;
                        playerCount++;
                    }
                }

                race.PlayersInRace = playerCount;
            }
     
            SystemAPI.SetSingleton(race);
        }
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(UpdateTimerSystem))]
    [UpdateBefore(typeof(IntroRaceSystem))]
    public partial struct RaceCountdownSystem : ISystem
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

            if (race.State != RaceState.CountDown)
                return;

            race.CurrentTimer -= SystemAPI.Time.DeltaTime;
            if (race.CurrentTimer < 0)
            {
                race.SetRaceState(RaceState.InProgress);
                race.InitialTime = SystemAPI.Time.ElapsedTime;

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

    [BurstCompile]    
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct CheckPlayersState : ISystem
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

            if (race.State is not RaceState.InProgress)
                return;

            var playersInRace = 0;
            foreach (var car in Query<PlayerAspect>())
            {
                if (car.LapProgress.InRace)
                {
                    playersInRace++;
                }
            }

            if (playersInRace > 0)
                return;

            race.State = RaceState.Lobby;
            SetSingleton(race);
        }
    }
}