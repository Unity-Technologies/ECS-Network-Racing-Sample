using System;
using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Provides debug logging with color-coded outputs for game events
    /// </summary>
    public static class RaceLogger
    {
        private const string PREFIX = "[RACE]";
        
        // Log colors (using Unity's rich text format)
        private const string COLOR_INFO = "cyan";
        private const string COLOR_SUCCESS = "green";
        private const string COLOR_WARNING = "yellow";
        private const string COLOR_ERROR = "red";
        private const string COLOR_GAMEPLAY = "magenta";
        private const string COLOR_NETWORK = "blue";
        private const string COLOR_PHYSICS = "teal";

        private static bool s_LoggingEnabled = true;

        /// <summary>
        /// Enable or disable all logging
        /// </summary>
        public static bool LoggingEnabled
        {
            get => s_LoggingEnabled;
            set => s_LoggingEnabled = value;
        }

        /// <summary>
        /// Logs a regular informational message (cyan)
        /// </summary>
        public static void Info(string message)
        {
            if (!s_LoggingEnabled) return;
            Debug.Log($"{PREFIX} <color={COLOR_INFO}>{message}</color>");
        }

        /// <summary>
        /// Logs a success message (green)
        /// </summary>
        public static void Success(string message)
        {
            if (!s_LoggingEnabled) return;
            Debug.Log($"{PREFIX} <color={COLOR_SUCCESS}>{message}</color>");
        }

        /// <summary>
        /// Logs a warning message (yellow)
        /// </summary>
        public static void Warning(string message)
        {
            if (!s_LoggingEnabled) return;
            Debug.LogWarning($"{PREFIX} <color={COLOR_WARNING}>{message}</color>");
        }

        /// <summary>
        /// Logs an error message (red)
        /// </summary>
        public static void Error(string message)
        {
            if (!s_LoggingEnabled) return;
            Debug.LogError($"{PREFIX} <color={COLOR_ERROR}>{message}</color>");
        }

        /// <summary>
        /// Logs a gameplay event (magenta)
        /// </summary>
        public static void Gameplay(string message)
        {
            if (!s_LoggingEnabled) return;
            Debug.Log($"{PREFIX} <color={COLOR_GAMEPLAY}>{message}</color>");
        }

        /// <summary>
        /// Logs a network event (blue)
        /// </summary>
        public static void Network(string message)
        {
            if (!s_LoggingEnabled) return;
            Debug.Log($"{PREFIX} <color={COLOR_NETWORK}>{message}</color>");
        }

        /// <summary>
        /// Logs a physics event (teal)
        /// </summary>
        public static void Physics(string message)
        {
            if (!s_LoggingEnabled) return;
            Debug.Log($"{PREFIX} <color={COLOR_PHYSICS}>{message}</color>");
        }

        /// <summary>
        /// Logs a formatted message with the specified color
        /// </summary>
        public static void CustomColor(string message, string hexColor)
        {
            if (!s_LoggingEnabled) return;
            Debug.Log($"{PREFIX} <color={hexColor}>{message}</color>");
        }
        
        /// <summary>
        /// Creates a labeled section in the log to group related messages
        /// </summary>
        public static void LogSection(string sectionName)
        {
            if (!s_LoggingEnabled) return;
            Debug.Log($"{PREFIX} ========== <color={COLOR_INFO}>{sectionName}</color> ==========");
        }
    }
} 