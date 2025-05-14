using TMPro;
using UnityEngine;

namespace KayosMedia.WatchOrNot.Prototype
{
    public class DeviceAuthUIController : MonoBehaviour
    {
        public GameObject plexConnectionPanel;
        public TMP_Text instructionText;
        public TMP_Text codeText;
        public TMP_Text statusText;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            PlexNetworkingManager.OnCodeReceived += ShowCode;
            PlexNetworkingManager.OnTokenValidation += HidePanel;
            PlexNetworkingManager.OnErrorOccurred += ShowError;

            plexConnectionPanel.SetActive(false);
        }

        public void Show()
        {
            instructionText.text = "Retrieving code from Plex.tv...";
            codeText.text = "";
            statusText.text = "";
            plexConnectionPanel.SetActive(true);

            _ = PlexNetworkingManager.AuthorizeDeviceAsync();
        }

        private void ShowCode(string code)
        {
            codeText.gameObject.SetActive(true);

            instructionText.text = $"Visit https://plex.tv/link and enter the code below to continue.";
            codeText.text = code;
            statusText.text = "Waiting for link confirmation...";
        }

        private void ShowError(string message)
        {
            codeText.gameObject.SetActive(false);

            instructionText.text = "Error";
            statusText.text = message;
        }

        private void HidePanel(bool isValidated)
        {
            plexConnectionPanel.SetActive(false);
        }


    }
}