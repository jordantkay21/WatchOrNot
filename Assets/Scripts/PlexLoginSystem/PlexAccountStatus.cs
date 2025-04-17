using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlexAccountStatus : MonoBehaviour
{
    public TMP_Text statusText;
    public TMP_Text nameText;
    public TMP_Text addressText;
    public TMP_Text playlistText;
    public TMP_Text MovieCountText;

    public Button connectionButton;
    public TMP_Text connectionButtonText;


    private void Start()
    {
        PlexAuthManager.Instance.OnServerDiscovered += (_, __) => UpdateStatusDisplay();

        UpdateStatusDisplay();

        connectionButton.onClick.AddListener(HandleConnection);
    }

    public void UpdateStatusDisplay()
    {
        if(SessionInfoManager.HasToken() && SessionInfoManager.HasServerInfo())
        {
            string name = SessionInfoManager.LoadName();

            statusText.text = $"Connected to {name}";
            connectionButtonText.text = "Disconnect";
        }
        else
        {
            statusText.text = "Not Connected to Plex Server";
            connectionButtonText.text = "Connect";
        }
    }

    private void HandleConnection()
    {
        if (SessionInfoManager.HasToken())
        {
            Debug.Log("Logging out of Plex...");
            SessionInfoManager.ClearAll();
            UpdateStatusDisplay();
        }
        else
        {
            Debug.Log("Starting Plex login...");
            PlexAuthManager.Instance.StartPlexLogin();
        }
    }
}
