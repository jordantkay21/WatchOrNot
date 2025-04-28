using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerSelectionUIController : MonoBehaviour
{
    public static ServerSelectionUIController Instance;

    public GameObject serverSelectionPanel;

    public TMP_Text messageText;

    public Button connectionButton;
    public TextMeshProUGUI connectionButtonText;

    public Button retrieveServerButton;
    public TextMeshProUGUI retrieveServerButtonText;
    
    public Button selectPlaylistButton;
    public TextMeshProUGUI selectPlaylistButtonText;
    
    public TMP_Dropdown serverDropdown;

    private bool _isConnected;

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

    private void Start()
    {
        //Bind UI actions
        connectionButton.onClick.AddListener(OnConnectionButtonClicked);
        retrieveServerButton.onClick.AddListener(OnRetrieveServerButtonClicked);
        selectPlaylistButton.onClick.AddListener(AdvancePhase);

        //Subscribe to fetcher events
        PlexDataFetcher.Instance.OnTokenValidation += OnTokenReceived;
        PlexDataFetcher.Instance.OnTokenRequired += OnTokenRequired;
        PlexDataFetcher.Instance.OnServerListBuilt += UpdateServerDropdown;
        PlexDataFetcher.Instance.OnErrorOccured += OnError;

        selectPlaylistButton.gameObject.SetActive(false);

        //Setup UI based on current session state
        PlexDataFetcher.Instance.InspectToken();
    }


    private void UpdateRetrieveServerButton()
    {
        retrieveServerButton.gameObject.SetActive(true);

        if(Proto_SessionInfoManager.LoadSavedServers().Count > 0)
        {
            retrieveServerButtonText.text = "Refresh Servers";
            UpdateServerDropdown();
        }
        else
        {
            retrieveServerButtonText.text = "Retrieve Servers";
        }
    }

    private void UpdateServerDropdown()
    {
        List<ServerInfo> servers = Proto_SessionInfoManager.LoadSavedServers();

        List<string> options = new List<string> { "-- Select a server --" };
        options.AddRange(servers.Select(s => $"{s.name} ({s.publicAddress})"));
        serverDropdown.ClearOptions();
        serverDropdown.AddOptions(options);
        serverDropdown.value = 0;
        serverDropdown.RefreshShownValue();


        serverDropdown.onValueChanged.RemoveAllListeners();
        serverDropdown.onValueChanged.AddListener(index =>
        {
            if (index == 0) return;

            var realIndex = index - 1;

            if (realIndex < servers.Count)
            {
                Proto_SessionInfoManager.SetCurrentServer(servers[realIndex]);
                Debug.Log($"[PlexSessionUIController] [UpdateServerDropdown] | Selected server: {servers[realIndex].name}");
                selectPlaylistButton.gameObject.SetActive(true);
            }
        });
    }

    private void OnConnectionButtonClicked()
    {
        if (_isConnected)
        {
            //Disconnect
            Proto_SessionInfoManager.ClearAll();
            _isConnected = false;

            OnTokenRequired();
            Debug.Log($"[PlexSessionUIController] [OnConnectionButtonClicked] | Disconnected from Plex");
        }
        else
        {
            //Connect (initiate device authentication flow)
            PlexDataFetcher.Instance.AuthorizeDevice();
            CollectingToken();
            Debug.Log($"[PlexSessionUIController] [OnConnectionButtonClicked] | Initiating Device Authentication");
        }
    }

    private void OnRetrieveServerButtonClicked()
    {
        if (!Proto_SessionInfoManager.HasToken())
        {
            Debug.LogWarning("$[PlexSessionUIController][OnConnectionButtonClicked] | No Token available. Cannot retrieve servers.");
            return;
        }

        //Rebuild server list
        Proto_SessionInfoManager.LoadSavedServers().Clear();
        string token = Proto_SessionInfoManager.LoadToken();
        PlexDataFetcher.Instance.BuildServerList(token);
    }

    private void OnTokenReceived()
    {
        _isConnected = true;
        connectionButtonText.text = "Disconnect";

        UpdateRetrieveServerButton();
    }

    private void CollectingToken()
    {
        messageText.gameObject.SetActive(false);

        connectionButtonText.text = "Connecting...";
    }

    private void OnTokenRequired()
    {
        string message = "Device Connection Required";
        _isConnected = false;

        messageText.text = message;
        connectionButtonText.text = "Connect";
        retrieveServerButton.gameObject.SetActive(false);
        serverDropdown.gameObject.SetActive(false);
    }

    private void AdvancePhase()
    {
        serverSelectionPanel.SetActive(false);
        PlaylistSelectionUIController.Instance.StartSelectionPhase();
    }
    private void OnError(string message)
    {

    }
}
