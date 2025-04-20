using Unity.Entities.Racing.Common;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// System that monitors race state changes and logs them using the RaceLogger
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class RaceStateLoggerSystem : SystemBase
    {
        private RaceState m_PreviousState;
        
        protected override void OnCreate()
        {
            RequireForUpdate<Race>();
            m_PreviousState = RaceState.None;
        }

        protected override void OnUpdate()
        {
            var race = GetSingleton<Race>();
            
            // Only log when the state changes
            if (race.State != m_PreviousState)
            {
                LogRaceStateChange(m_PreviousState, race.State);
                m_PreviousState = race.State;
            }
        }
        
        private void LogRaceStateChange(RaceState previousState, RaceState newState)
        {
            // If the RaceLoggerManager is available, use it to log the state change
            if (RaceLoggerManager.Instance != null)
            {
                RaceLoggerManager.Instance.LogRaceStateChange(previousState, newState);
                return;
            }
            
            // Otherwise, log directly
            switch (newState)
            {
                case RaceState.None:
                    RaceLogger.Info($"Race State: {previousState} → None");
                    break;
                case RaceState.NotStarted:
                    RaceLogger.Info($"Race State: {previousState} → Not Started");
                    break;
                case RaceState.ReadyToRace:
                    RaceLogger.Gameplay($"Race State: {previousState} → Ready To Race");
                    break;
                case RaceState.StartingRace:
                    RaceLogger.Gameplay($"Race State: {previousState} → Starting Race");
                    break;
                case RaceState.CountDown:
                    RaceLogger.LogSection("RACE COUNTDOWN");
                    RaceLogger.Gameplay($"Race State: {previousState} → Count Down");
                    break;
                case RaceState.InProgress:
                    RaceLogger.LogSection("RACE STARTED");
                    RaceLogger.Success($"Race State: {previousState} → In Progress");
                    break;
                case RaceState.Finishing:
                    RaceLogger.Gameplay($"Race State: {previousState} → Finishing");
                    break;
                case RaceState.Finished:
                    RaceLogger.Success($"Race State: {previousState} → Finished");
                    break;
                case RaceState.Leaderboard:
                    RaceLogger.LogSection("RACE FINISHED");
                    RaceLogger.Success($"Race State: {previousState} → Leaderboard");
                    break;
                case RaceState.StartingRaceAutomatically:
                    RaceLogger.Gameplay($"Race State: {previousState} → Starting Race Automatically");
                    break;
            }
        }
    }
} 