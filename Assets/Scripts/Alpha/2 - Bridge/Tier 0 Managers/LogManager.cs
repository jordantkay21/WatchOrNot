using System;
using System.Runtime.CompilerServices;
using KayosTech.ReelDeal.Alpha.Backend.Handler;
using KayosTech.ReelDeal.Alpha.Logging;
using TMPro;
using UnityEngine;

namespace KayosTech.ReelDeal.Alpha.Logging
{

    [System.Serializable]
    public enum LogLevel
    {
        Internal, //Backend use only - to file/console
        Verbose, //Developer-readable, can show to player in dev builds
        Success, //Player-visible
        Alert, //Player-visible warning
        Error //Player-visible error
    }

    public static class LogUtility
    {
        public static string GetColorHex(LogLevel level)
        {
            return level switch
            {
                LogLevel.Verbose => "#4FC3F7",
                LogLevel.Success => "#32D475",
                LogLevel.Alert => "#FFB347",
                LogLevel.Error => "#E94F4F",
                _ => "#CCCCCC"
            };
        }
    }

    public struct LogEntry
    {
        public string ScriptName;
        public string MethodName;
        public LogLevel Level;
        public string Tag;
        public string Message;
        public DateTime Timestamp;

        public string Formatted => $"[{ScriptName}] [{MethodName}] | [{Level}] [{Tag}]" +
                                   $"\n {Timestamp:HH:mm:ss.fff} " +
                                   $"\n  <b>{Message}</b>";
    }
}


namespace KayosTech.ReelDeal.Alpha.Bridge.Manager
{
    /// <summary>
    /// Captures log messages and routes them to both visual and persistent outputs
    /// </summary>
    public class LogManager : MonoBehaviour
    {
        public static event Action<LogEntry> OnLogReceived;

        private void Awake()
        {
            LocalDataHandler.InitializeLogFile();
            OnLogReceived += HandleFileLog;
        }

        private void OnApplicationQuit()
        {
            LocalDataHandler.SaveLogFile();
        }

        public static void Log(
            string message,
            LogLevel level = LogLevel.Verbose,
            string tag = "",
            [CallerMemberName] string callingMethod = "",
            [CallerFilePath] string callingFile = "")
        {
            string scriptName = System.IO.Path.GetFileNameWithoutExtension(callingFile);

            LogEntry entry = new LogEntry
            {
                ScriptName = scriptName,
                MethodName = callingMethod,
                Level = level,
                Tag = tag,
                Message = message, 
                Timestamp = DateTime.Now
            };

            string color = LogUtility.GetColorHex(entry.Level);
            string formatted = $"<color={color}>{entry.Formatted}</color>";

            //Console Logging
            switch (level)
            {
                case LogLevel.Internal:
                    Debug.Log($"{formatted}");
                    break;
                case LogLevel.Verbose:
                    Debug.Log($"{formatted}");
                    break;
                case LogLevel.Success:
                    Debug.Log($"{formatted}");
                    break;
                case LogLevel.Alert:
                    Debug.LogWarning($"{formatted}");
                    break;
                case LogLevel.Error:
                    Debug.LogError($"{formatted}");
                    break;
                default:
                    Debug.Log($"[UNKNOWN LEVEL] {formatted}");
                    break;
            }

            OnLogReceived?.Invoke(entry);
        }

        private static void HandleFileLog(LogEntry entry)
        {
            LocalDataHandler.AppendToLog(entry.Formatted);
        }
    }
}
