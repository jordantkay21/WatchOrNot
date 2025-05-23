using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using KayosTech.ReelDeal.Alpha.Bridge.Manager;
using KayosTech.ReelDeal.Alpha.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace KayosTech.ReelDeal.Alpha.Frontend.Handler
{
    public class MessageHandler : MonoBehaviour
    {
        [Header("Message Prefabs")] 
        [SerializeField] private UIMessage infoMessagePrefab;
        [SerializeField] private UIMessage successMessagePrefab;
        [SerializeField] private UIMessage alertMessagePrefab;
        [SerializeField] private UIMessage errorMessagePrefab;

        [Header("Parent Object")] 
        [SerializeField] private Transform messageParent;


        private void OnEnable()
        {
            LogManager.OnLogReceived += DisplayLog;
        }

        private void OnDisable()
        {
            LogManager.OnLogReceived -= DisplayLog;
        }

        private void DisplayLog(LogEntry log)
        {
            var newMsg = log.Level switch
            {
                LogLevel.Internal => null,
                LogLevel.Verbose => Instantiate(infoMessagePrefab, messageParent),
                LogLevel.Success => Instantiate(successMessagePrefab, messageParent),
                LogLevel.Alert => Instantiate(alertMessagePrefab, messageParent),
                LogLevel.Error => Instantiate(errorMessagePrefab, messageParent),
                _ => HandleUnknownLevel(log)
            };

            if (newMsg == null) return;

            newMsg.Initialize(log.Message, log.Level);

            //Force layout rebuild so it positions properly on first message
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)messageParent);

        }

        private static UIMessage HandleUnknownLevel(LogEntry log)
        {
            LogManager.Log(
                $"Unknown log level '{log.Level}' encountered. This log will not be shown in the UI.",
                LogLevel.Internal,
                "LogHandler"
                );

            return null;
        }
    }
}
