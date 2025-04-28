using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaylistSelectionUIController : MonoBehaviour
{
    public static PlaylistSelectionUIController Instance;

    public GameObject PlaylistSelectionPanel;

    public TextMeshProUGUI messageText;
    public TextMeshProUGUI serverText;
    public Button retrievePlaylistButton;
    public Button playButton;

    public TMP_Dropdown playlistDropdown; 

    private void Awake()
    {
        if(Instance == null)
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
        PlexDataFetcher.Instance.OnPlaylistBuilt += UpdatePlaylistDropdown;
        PlexDataFetcher.Instance.OnPlaylistItemsFetched += (_) => playButton.gameObject.SetActive(true);

        //Bind UI actions
        retrievePlaylistButton.onClick.AddListener(OnRetrievePlaylistButtonClicked);

    }

    public void StartSelectionPhase()
    {
        PlaylistSelectionPanel.SetActive(true);

        messageText.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);

        serverText.text = Proto_SessionInfoManager.LoadCurrentServer().name;

        OnRetrievePlaylistButtonClicked();

    }
    private void OnRetrievePlaylistButtonClicked()
    {
        if (!Proto_SessionInfoManager.HasToken())
        {
            Debug.LogWarning("$[PlaylistSelectionUIController][OnRetrievePlaylistButtonClicked] | No Token available. Cannot retrieve playlist.");
            return;
        }

        if (!Proto_SessionInfoManager.HasCurrentServer())
        {
            Debug.LogWarning("$[PlaylistSelectionUIController][OnRetrievePlaylistButtonClicked] | No Server available. Cannot retrieve playlist.");
            return;
        }

        //Rebuild Playlist List
        Proto_SessionInfoManager.LoadSavedPlaylists().Clear();

        string token = Proto_SessionInfoManager.LoadToken();
        string serverUri = Proto_SessionInfoManager.LoadCurrentServer().uri;

        PlexDataFetcher.Instance.BuildPlaylistList(token, serverUri);

    }

    private void UpdatePlaylistDropdown()
    {
        List<PlaylistInfo> playlists = Proto_SessionInfoManager.LoadSavedPlaylists();

        Debug.Log($"[PlaylistSelectionUIController] [UpdatePlaylistDropdown] | Retrieved list of playlists from SessionInfoManager. List count = {playlists.Count}");

        List<string> options = new List<string> { "-- Select a Playlist--" };
        options.AddRange(playlists.Select(pl => $"{pl.title} ({pl.movieCount})"));

        playlistDropdown.ClearOptions();
        playlistDropdown.AddOptions(options);
        playlistDropdown.value = 0;
        playlistDropdown.RefreshShownValue();

        playlistDropdown.onValueChanged.RemoveAllListeners();
        playlistDropdown.onValueChanged.AddListener(index =>
        {
            if (index == 0) return;

            var realIndex = index - 1;

            if (realIndex < playlists.Count)
            {
                PlaylistDisplayUIController.Instance.Hide();  
                Proto_SessionInfoManager.SetCurrentPlaylist(playlists[realIndex]);
                Debug.Log($"[PlaylistSelectionUIController][UpdatePlaylistDropdown] | Selected playlist: { playlists[realIndex]}");
                PlexDataFetcher.Instance.FetchPlaylistItems();

            }
        });

    }

    private void AdvancePhase()
    {

    }
}
