using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlexAccountStatus : MonoBehaviour
{
    public TMP_Text statusText;
    public Button ConnectButton;
    public Button DisconnectButton;


    private void Start()
    {
        PlexAuthManager.Instance.OnServerDiscovered += (_, __) => UpdateStatusDisplay();
        UpdateStatusDisplay();

        DisconnectButton.onClick.AddListener(() =>
        {
            PlexStorageManager.ClearAll();
            UpdateStatusDisplay();
        });

        ConnectButton.onClick.AddListener(() =>
        {
            PlexStorageManager.ClearAll();
            PlexAuthManager.Instance.StartPlexLogin();
        });
    }

    public void UpdateStatusDisplay()
    {
        if(PlexStorageManager.HasToken() && PlexStorageManager.HasServerInfo())
        {
            string name = PlexStorageManager.LoadName();

            statusText.text = $"Connected to {name}";

            DisconnectButton.gameObject.SetActive(true);
            ConnectButton.gameObject.SetActive(false);
        }
        else
        {
            statusText.text = "Not Connected to Plex Server";

            DisconnectButton.gameObject.SetActive(false);
            ConnectButton.gameObject.SetActive(true);
        }
    }
}
