using UnityEngine;
using Unity.Entities.Racing.Common;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Manages the debug logging system
    /// </summary>
    public class RaceLoggerManager : MonoBehaviour
    {
        public static RaceLoggerManager Instance { get; private set; }

        [SerializeField] private RaceLoggerSettings settings;
        
        [Tooltip("Print startup log when the game starts")]
        [SerializeField] private bool printStartupLog = true;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeLogger();
        }

        private void InitializeLogger()
        {
            if (settings != null)
            {
                settings.ApplySettings();
            }
            
            if (printStartupLog)
            {
                LogStartupInfo();
            }
        }

        private void LogStartupInfo()
        {
            RaceLogger.LogSection("RACE GAME STARTUP");
            RaceLogger.Info($"Game Version: {Application.version}");
            RaceLogger.Info($"Unity Version: {Application.unityVersion}");
            RaceLogger.Info($"Platform: {Application.platform}");
            RaceLogger.Info($"System Language: {Application.systemLanguage}");
            RaceLogger.LogSection("INITIALIZATION");
        }
        
        /// <summary>
        /// Log a state change in the race
        /// </summary>
        public void LogRaceStateChange(RaceState previousState, RaceState newState)
        {
            if (settings == null || !settings.logRaceState) return;
            
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
                default:
                    RaceLogger.Info($"Race State: {previousState} → {newState}");
                    break;
            }
        }
        
        /// <summary>
        /// Log a player action
        /// </summary>
        public void LogPlayerAction(string playerName, string action)
        {
            if (settings == null || !settings.logPlayerActions) return;
            RaceLogger.Gameplay($"Player '{playerName}': {action}");
        }
        
        /// <summary>
        /// Log a network event
        /// </summary>
        public void LogNetworkEvent(string eventName, string details = null)
        {
            if (settings == null || !settings.logNetwork) return;
            
            string message = string.IsNullOrEmpty(details) 
                ? $"Network: {eventName}" 
                : $"Network: {eventName} - {details}";
                
            RaceLogger.Network(message);
        }
    }
} 