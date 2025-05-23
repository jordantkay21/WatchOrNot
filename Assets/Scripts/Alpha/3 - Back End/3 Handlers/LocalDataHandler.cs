using System;
using System.IO;
using System.Text;
using KayosTech.ReelDeal.Alpha.Bridge.Manager;
using NUnit;
using UnityEngine;

namespace KayosTech.ReelDeal.Alpha.Backend.Handler
{
    public static class LocalDataHandler
    {
        private static string logFilePath;
        private static StringBuilder logBuffer = new();

        public static void InitializeLogFile()
        {
            string folder = GetLogFolderPath();
            Directory.CreateDirectory(folder);


            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            logFilePath = Path.Combine(folder, $"log_{timestamp}.txt");

            AppendToLog($"[SESSION START] {DateTime.Now}");
        }

        public static void AppendToLog(string message)
        {
            logBuffer.AppendLine(message);
        }

        public static void SaveLogFile()
        {
            if (string.IsNullOrEmpty(logFilePath))
            {
                Debug.LogWarning("[LocalDataHandler] SaveLogFile called but logFilePath is null or empty.");
                return;
            }

            File.WriteAllText(logFilePath, logBuffer.ToString());
        }

        /// <summary>
        /// Returns the appropriate log folder path depending on platform
        /// </summary>
        private static string GetLogFolderPath()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ReelDealLogs");
#else
    return Path.Combine(Application.persistentDataPath, "ReelDealLogs");
#endif
        }
    }
}