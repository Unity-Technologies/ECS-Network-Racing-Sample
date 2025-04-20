using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Scriptable object to configure the RaceLogger from the Unity Editor
    /// </summary>
    [CreateAssetMenu(fileName = "RaceLoggerSettings", menuName = "Racing/Debug/Logger Settings")]
    public class RaceLoggerSettings : ScriptableObject
    {
        [Header("Logging Settings")]
        [Tooltip("Enable or disable all logging")]
        public bool loggingEnabled = true;
        
        [Header("Log Categories")]
        [Tooltip("Log game initialization and startup events")]
        public bool logInitialization = true;
        
        [Tooltip("Log network and connection events")]
        public bool logNetwork = true;
        
        [Tooltip("Log player actions and inputs")]
        public bool logPlayerActions = true;
        
        [Tooltip("Log race state changes")]
        public bool logRaceState = true;
        
        [Tooltip("Log vehicle physics events")]
        public bool logPhysics = false;
        
        [Tooltip("Log details about code performance")]
        public bool logPerformance = false;
        
        [Header("Advanced Settings")]
        [Tooltip("Log call stack for errors")]
        public bool logStackTraceForErrors = true;
        
        [Tooltip("Automatically save logs to file")]
        public bool saveLogsToFile = false;
        
        [Tooltip("Path for log files (relative to persistent data path)")]
        public string logFilePath = "Logs";
        
        private void OnEnable()
        {
            ApplySettings();
        }
        
        /// <summary>
        /// Apply the settings to the RaceLogger
        /// </summary>
        public void ApplySettings()
        {
            RaceLogger.LoggingEnabled = loggingEnabled;
        }
    }
} 