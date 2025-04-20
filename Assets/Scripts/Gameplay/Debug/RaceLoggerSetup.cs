using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Helper MonoBehaviour to create the RaceLogger prefab and setup in the scene.
    /// Add this to a GameObject in your scene to enable logging.
    /// </summary>
    [DefaultExecutionOrder(-1000)] // Ensure this runs very early in initialization
    public class RaceLoggerSetup : MonoBehaviour
    {
        [Header("Logger Settings Asset")]
        [Tooltip("The settings for the race logger. Create one using right-click > Create > Racing > Debug > Logger Settings")]
        [SerializeField] private RaceLoggerSettings loggerSettings;
        
        [Header("UI Settings")]
        [Tooltip("Show a UI panel with the most recent log messages")]
        [SerializeField] private bool showLogUI = false;
        
        [Tooltip("Maximum number of log entries to display in the UI")]
        [SerializeField] private int maxLogEntries = 10;
        
        private RaceLoggerManager m_LoggerManager;
        
        private void Awake()
        {
            // Create the logger manager object
            GameObject loggerManagerObj = new GameObject("RaceLoggerManager");
            m_LoggerManager = loggerManagerObj.AddComponent<RaceLoggerManager>();
            
            // Set the settings if available
            if (loggerSettings != null)
            {
                var managerField = m_LoggerManager.GetType().GetField("settings", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (managerField != null)
                {
                    managerField.SetValue(m_LoggerManager, loggerSettings);
                }
            }
            
            // Make sure it persists across scenes
            DontDestroyOnLoad(loggerManagerObj);
            
            // Create the logger system in the default world
            if (World.DefaultGameObjectInjectionWorld != null)
            {
                World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<RaceStateLoggerSystem>();
                
                // Log successful initialization
                RaceLogger.LogSection("LOGGER INITIALIZED");
                RaceLogger.Success("Race Logger initialized successfully");
                RaceLogger.Info($"Game Version: {Application.version}");
                RaceLogger.Info($"Unity Version: {Application.unityVersion}");
                RaceLogger.Info($"Platform: {Application.platform}");
            }
        }
    }
} 