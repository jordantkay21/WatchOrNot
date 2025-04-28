using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Proto_DeviceAuthUIController : MonoBehaviour
{
    public static Proto_DeviceAuthUIController Instance;

    [Header("UI Elements")]
    public GameObject plexConnectionPanel;
    public TMP_Text instructionText;
    public TMP_Text codeText;
    public TMP_Text statusText;

    private void Start()
    {
        PlexDataFetcher.Instance.OnCodeReceived += OnCodeReceived;
        PlexDataFetcher.Instance.OnTokenValidation += DeviceAuthorized;

        plexConnectionPanel.gameObject.SetActive(false);
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

    //Don't need
    public void BeginLogin()
    {
        plexConnectionPanel.SetActive(true);

        instructionText.gameObject.SetActive(true);
        statusText.gameObject.SetActive(false);
        codeText.gameObject.SetActive(false);

        instructionText.text = "Retrieving code from Plex.tv";
    }

    public void DeviceAuthorized()
    {
        plexConnectionPanel.SetActive(false);
    }

    private void OnCodeReceived(string code)
    {
        plexConnectionPanel.SetActive(true);

        instructionText.gameObject.SetActive(true);
        codeText.gameObject.SetActive(true);
        statusText.gameObject.SetActive(true);

        instructionText.text = "Visit https://plex.tv/link and enter the code below to continue.";
        codeText.text = code;
        statusText.text = "Waiting for link confirmation...";

    }
}
