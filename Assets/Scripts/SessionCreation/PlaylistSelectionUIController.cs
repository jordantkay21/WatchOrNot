using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaylistSelectionUIController : MonoBehaviour
{
    public static PlaylistSelectionUIController Instance;

    public GameObject PlaylistSelectionPanel;

    public Button retrievePlaylistButton;

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

        //Bind UI actions
        retrievePlaylistButton.onClick.AddListener(OnRetrievePlaylistButtonClicked);

    }

    public void StartSelectionPhase()
    {
        PlaylistSelectionPanel.SetActive(true);
    }
    private void OnRetrievePlaylistButtonClicked()
    {
        if (!SessionInfoManager.HasToken())
        {
            Debug.LogWarning("$[PlaylistSelectionUIController][OnRetrievePlaylistButtonClicked] | No Token available. Cannot retrieve playlist.");
            return;
        }

        if (!SessionInfoManager.HasCurrentServer())
        {
            Debug.LogWarning("$[PlaylistSelectionUIController][OnRetrievePlaylistButtonClicked] | No Server available. Cannot retrieve playlist.");
            return;
        }

        //Rebuild Playlist List
        SessionInfoManager.LoadSavedPlaylists().Clear();

        string token = SessionInfoManager.LoadToken();
        string serverUri = SessionInfoManager.LoadCurrentServer().uri;

        PlexDataFetcher.Instance.BuildPlaylistList(token, serverUri);

    }

    private void UpdatePlaylistDropdown()
    {
        List<PlaylistInfo> playlists = SessionInfoManager.LoadSavedPlaylists();

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
                SessionInfoManager.SetCurrentPlaylist(playlists[realIndex]);
                Debug.Log($"[PlaylistSelectionUIController][UpdatePlaylistDropdown] | Selected playlist: { playlists[realIndex]}");
                PlexDataFetcher.Instance.FetchPlaylistItems();
            }
        });

    }
}
