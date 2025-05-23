using System;
using KayosTech.ReelDeal.Alpha.Logging;
using UnityEngine;

namespace KayosTech.ReelDeal.Alpha.Bridge.Manager
{
    /// <summary>
    /// Orchestrates the entire app session (auth state, UI boot, data flow)
    /// </summary>
    public class SessionManager : MonoBehaviour
    {
        private void Start()
        {
            LogManager.Log("Internal Message Example", LogLevel.Internal, "Test");
            LogManager.Log("Verbose Message Example", LogLevel.Verbose, "Test");
            LogManager.Log("Success Message Example", LogLevel.Success, "Test");
            LogManager.Log("Alert Message Example", LogLevel.Alert, "Test");
            LogManager.Log("Error Message Example", LogLevel.Error, "Test");
        }
    }
}
