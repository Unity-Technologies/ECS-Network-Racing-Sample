using Unity.Burst;
using Unity.Entities.Racing.Common;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Evaluate and sets the game's timer, and timer type.
    /// </summary>
    [BurstCompile]
    public partial struct UpdateTimerJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref Race race)
        {
            if (race.CurrentTimer > 0)
            {
                race.CurrentTimer -= DeltaTime;
            }
            else
            {
                race.TimerFinished = true;
            }

            // Cache last state to execute the behavior just once
            if (race.State != race.LastState)
            {
                switch (race.State)
                {
                    case RaceState.NotStarted:
                        race.CurrentTimer = race.CountDownTimer;
                        break;
                    case RaceState.ReadyToRace:
                        race.CurrentTimer = race.PlayersReadyTimer;
                        break;
                    case RaceState.StartingRace:
                        race.CurrentTimer = race.IntroRaceTimer;
                        break;
                    case RaceState.CountDown:
                        race.CurrentTimer = race.CountDownTimer;
                        break;
                    case RaceState.Finishing:
                        race.CurrentTimer = race.PlayersInRace > 1 ? race.FinalTimer : 0;
                        break;
                    case RaceState.Finished:
                        race.CurrentTimer = race.CelebrationIdleTimer;
                        break;
                    case RaceState.Leaderboard:
                        race.CurrentTimer = race.LeaderboardTimer;
                        break;
                }

                race.TimerFinished = false;
                race.LastState = race.State;
            }
        }
    }

    /// <summary>
    /// Runs the job to update and evaluate the game's timer.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct UpdateTimerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var updateTimerJob = new UpdateTimerJob
            {
                DeltaTime = state.WorldUnmanaged.Time.DeltaTime
            };
            state.Dependency = updateTimerJob.ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }
    }
}