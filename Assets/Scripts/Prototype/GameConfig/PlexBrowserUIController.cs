using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KayosMedia.WatchOrNot.Prototype
{
    public class PlexBrowserUIController : MonoBehaviour
    {
        public DeviceAuthUIController deviceAuthPopup;

        [Header("Panels & UI Elements")]
        public GameObject gameConfigPanel;
        public GameObject deviceLinkPanel;
        public GameObject playlistSelectionPanel;
        public GameObject itemDisplayPanel;

        [Header("Device Auth UI")]
        public Button linkButton;
        public Button unlinkButton;

        [Header("Server UI")]
        public TMP_Dropdown serverDropdown;
        public Button serverRefreshButton;

        [Header("Playlist UI")]
        public TMP_Dropdown playlistDropdown;
        public Button playlistRefreshButton;

        [Header("Playback UI")]
        public Button playButton;

        [Header("Movie Display")]
        public Transform playlistDisplayPanel;
        public GameObject movieInfoContainerPrefab;

        [Header("Status Bar")]
        public TMP_Text statusBarText;


        private List<ServerInfo> servers = new();
        private List<PlaylistInfo> playlists = new();
        private List<MovieInfo> movies = new List<MovieInfo>();

        private void Start()
        {
            PlexNetworkingManager.OnTokenValidation += OnTokenValidated;
            PlexNetworkingManager.OnStatusUpdate += UpdateStatus;
            PlexNetworkingManager.OnErrorOccurred += Debug.LogWarning;
            GameManager.OnSessionCleared += SetToConnectionNeeded;

            linkButton.onClick.AddListener(() => 
            {
                Debug.Log($"[PlexBrowserUIController][linkButton] Link Button Pressed");
                deviceAuthPopup.Show(); 
            });
            unlinkButton.onClick.AddListener(() => SessionInfoManager.Clear());

            playButton.onClick.AddListener(OnPlayClicked);
            serverRefreshButton.onClick.AddListener(async () => await RefreshServersAsync());
            playlistRefreshButton.onClick.AddListener(async () => await RefreshPlaylistsAsync());

            playlistDropdown.onValueChanged.AddListener(async index => await OnPlaylistSelected(index));
            serverDropdown.onValueChanged.AddListener(async index => await OnServerSelected(index));

            gameConfigPanel.SetActive(true);
            itemDisplayPanel.gameObject.SetActive(false);
            playButton.gameObject.SetActive(false);

            //Validate token and attempt preload
            _ = PlexNetworkingManager.ValidateStoredTokenAsync();
        }

        private void OnTokenValidated(bool isValidated)
        {
            if (isValidated)
            {
                UpdateStatus("Token Successfully Validated");
                SetToDeviceConnected();
            }
            else
            {
                UpdateStatus("Token Needed");
                SetToConnectionNeeded();
            }

        }

        private void SetToDeviceConnected()
        {
            playlistSelectionPanel.SetActive(true);
            deviceLinkPanel.SetActive(false);
            unlinkButton.gameObject.SetActive(true);
            _ = RefreshServersAsync();
        }

        private void SetToConnectionNeeded()
        {
            playlistSelectionPanel.SetActive(false);
            deviceLinkPanel.SetActive(true);
            unlinkButton.gameObject.SetActive(false);
        }

        private async Task RefreshServersAsync()
        {
            UpdateStatus("Fetching Servers...");
            servers = await PlexNetworkingManager.FetchServersAsync();
            UpdateStatus("Servers Loaded");

            UpdateServerDropdown();
        }

        private async Task RefreshPlaylistsAsync()
        {
            if (!SessionInfoManager.HasCurrentServer())
            {
                Debug.LogWarning($"[PlexBrowserUIController][RefreshPlaylistsAsync] No server currently selected");
                return;
            }

            UpdateStatus("Fetching Playlists...");
            playlists = await PlexNetworkingManager.FetchPlaylistAsync(SessionInfoManager.GetCurrentServer());
            UpdateStatus("Playlists Loaded");

            UpdatePlaylistDropdown();
        }

        private void UpdateServerDropdown()
        {
            serverDropdown.ClearOptions();

            List<string> options = new List<string> { "-- Select Server --" };
            options.AddRange(servers.ConvertAll(s => $"{s.name} ({s.publicAddress})"));

            serverDropdown.AddOptions(options);
            serverDropdown.value = 0;
            serverDropdown.RefreshShownValue();
        }
        private void UpdatePlaylistDropdown()
        {
            playlistDropdown.ClearOptions();

            var options = new List<string> { "-- Select Playlist --" };
            options.AddRange(playlists.ConvertAll(p => $"{p.title} ({p.movieCount})"));

            playlistDropdown.AddOptions(options);
            playlistDropdown.value = 0;
            playlistDropdown.RefreshShownValue();
        }

        public async Task OnServerSelected(int index)
        {
            if (index == 0) return;

            var selectedServer = servers[index - 1];
            SessionInfoManager.SetCurrentServer(selectedServer);

            await RefreshPlaylistsAsync();
        }

        private async Task OnPlaylistSelected(int index)
        {
            if (index == 0) return;

            var selectedPlaylist = playlists[index - 1];
            SessionInfoManager.SetCurrentPlaylist(selectedPlaylist);

            UpdateStatus("Fetching Movies...");
            movies = await PlexNetworkingManager.FetchPlaylistItemsAsync(selectedPlaylist);

            UpdateStatus($"Loaded {movies.Count} Movies");
            DisplayPlaylist(movies);
        }

        private void OnPlayClicked()
        {
            if (!SessionInfoManager.HasCurrentPlaylist())
            {
                Debug.LogWarning("[PlexBrowserUIController][OnPlayClicked] No playlist selected");
                return;
            }

            if (movies.Count == 0)
            {
                Debug.Log("[PlexBrowserUIController][OnPlayClicked] No cache found. Should fetch online.");
            }

            Debug.Log("[PlexBrowserUIController][OnPlayClicked] LET THE GAMES BEGIN!");

            // Step 1: Randomly select 12 movies
            GameManager.Instance.SelectRandomMovies(movies);

            //Step 2: Move to Ranking Phase
            GameManager.Instance.StartRanking();

            //Step 3: Hide the current browser panel
            gameConfigPanel.SetActive(false);
            itemDisplayPanel.SetActive(false);
            playButton.gameObject.SetActive(false);

        }

        private void DisplayPlaylist(List<MovieInfo> movies)
        {
            foreach (Transform child in playlistDisplayPanel)
                Destroy(child.gameObject);

            foreach (var movie in movies)
            {
                GameObject item = Instantiate(movieInfoContainerPrefab, playlistDisplayPanel);

                var titleText = item.transform.Find("MovieTitleText")?.GetComponent<TMP_Text>();
                if (titleText) titleText.text = movie.title;

                var yearText = item.transform.Find("MovieYearText")?.GetComponent<TMP_Text>();
                if (yearText) yearText.text = movie.year.ToString();

                var poster = item.transform.Find("MoviePosterImage")?.GetComponent<RawImage>();
                if (poster && movie.posterTexture)
                    poster.texture = movie.posterTexture;
            }

            itemDisplayPanel.gameObject.SetActive(true);
            playButton.gameObject.SetActive(true);
            Debug.Log($"[PlexBrowserUIController][DisplayPlaylist] Displayed {movies.Count} movies");
        }

        private void UpdateStatus(string message)
        {
            if (statusBarText != null)
                statusBarText.text = message;
        }
    }
}