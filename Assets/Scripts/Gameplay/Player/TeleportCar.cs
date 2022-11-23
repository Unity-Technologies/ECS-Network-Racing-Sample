using Dots.Racing;
using Unity.Burst;
using Unity.Entities.Racing.Common;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct TeleportPlayersJob : IJobEntity
    {
        private void Execute([EntityIndexInQuery] int index, ref PlayerAspect player)
        {
            // If there is no target entity we teleport all players
            if (!player.Reset.Transform)
            {
                return;
            }

            player.Transform.WorldPosition = player.Reset.TargetPosition;
            player.Transform.WorldRotation = player.Reset.TargetRotation;
            player.ResetVelocity();
            player.SetPlayerTransformReady();
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

    [BurstCompile]
    [UpdateBefore(typeof(TeleportCarSystem))]
    public partial struct ResetWheelsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

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

    [BurstCompile]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial struct DisableSmoothingSystem : ISystem
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
            var disableSmoothingJob = new DisableSmoothingJob();
            disableSmoothingJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial struct TeleportCarSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var teleportPlayersJob = new TeleportPlayersJob();
            state.Dependency = teleportPlayersJob.ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial struct TeleportPlayerToLobbySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();

            // If the race is not finished, skip this
            if (!race.IsFinished())
            {
                return;
            }

            var spawnPointBuffer = GetSingletonBuffer<SpawnPoint>();
            var index = 0;
            foreach (var car in Query<PlayerAspect>())
            {
                if (car.Player.State == PlayerState.Finished && car.LapProgress.InRace)
                {
                    car.CountdownTeleportTimer(Time.DeltaTime);

                    if (car.LapProgress.TimerToMovePlayer <= 0) // TODO: maybe redo these if statements
                    {
                        car.ResetVehicle();
                        car.SetTargetTransform(spawnPointBuffer[index].LobbyPosition,
                            spawnPointBuffer[index].LobbyRotation);
                    }
                }
                else if (race.State is RaceState.Leaderboard && car.LapProgress.InRace)
                {
                    car.ResetVehicle();
                    car.SetTargetTransform(spawnPointBuffer[index].LobbyPosition,
                        spawnPointBuffer[index].LobbyRotation);
                }

                index++;
            }
        }
    }
}