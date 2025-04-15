using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlexLoginUI : MonoBehaviour
{
    public static PlexLoginUI Instance;

    [Header("UI Elements")]
    public GameObject plexConnectionPanel;
    public TMP_Text instructionText;
    public TMP_Text codeText;
    public TMP_Text statusText;

    private void Start()
    {
        PlexAuthManager.Instance.OnCodeReceived += OnCodeReceived;
        PlexAuthManager.Instance.OnTokenReceived += OnTokenReceived;
        PlexAuthManager.Instance.OnServerDiscovered += OnServerDiscovered;
        PlexAuthManager.Instance.OnErrorOccured += (error) =>
        {
            statusText.text = $"Error: {error}";
        };
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void BeginLogin()
    {
        plexConnectionPanel.SetActive(true);
    }

    private void OnCodeReceived(string code)
    {
        instructionText.gameObject.SetActive(true);
        statusText.gameObject.SetActive(true);
        codeText.gameObject.SetActive(true);

        instructionText.text = "Visit https://plex.tv/link and enter the code below to continue.";
        codeText.text = code;
        statusText.text = "Waiting for link confirmation. . .";


        Debug.Log($"PlexLoginUI:OnCodeReceived - Code Retrieved {code}");
    }

    private void OnTokenReceived(string token)
    {
        instructionText.gameObject.SetActive(true);
        statusText.gameObject.SetActive(true);
        codeText.gameObject.SetActive(false);

        instructionText.text = "Token receieved!";
        statusText.text = "Storing Server Information...";

        Debug.Log($"PlexLoginUI:OnTokenReceived - Token Received {token}");
    }

    private void OnServerDiscovered(string ip, int port)
    {
        instructionText.gameObject.SetActive(true);
        statusText.gameObject.SetActive(true);
        codeText.gameObject.SetActive(false);

        instructionText.text = $"Found Plex Server at {ip}:{port}";
        statusText.text = "Storing Server Information...";

        Debug.Log($"PlexLoginUI:OnServerDiscovered - Found Plex Server at {ip}:{port}");
    }
}
