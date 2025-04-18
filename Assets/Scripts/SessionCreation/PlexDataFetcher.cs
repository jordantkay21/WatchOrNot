using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class PlexDataFetcher : MonoBehaviour
{
    public static PlexDataFetcher Instance;

    public string clientIdentifier = "watch-or-not-client";
    public string product = "WatchOrNot";
    public string deviceName = "WatchOrNotGame";

    private string pinId;
    private string userCode;

    public event Action<string> OnCodeReceived;
    public event Action OnTokenValidation;
    public event Action OnTokenRequired;
    public event Action OnServerListBuilt;
    public event Action<string> OnErrorOccured;
    public event Action OnPlaylistBuilt;

    private const string PlexHeaders = "application/xml";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    #region Utility Methods
    private void AttachPlexHeaders(UnityWebRequest req)
    {
        req.SetRequestHeader("Accept", PlexHeaders);
        req.SetRequestHeader("X-Plex-Client-Identifier", clientIdentifier);
        req.SetRequestHeader("X-Plex-Product", product);
        req.SetRequestHeader("X-Plex-Version", "1.0");
        req.SetRequestHeader("X-Plex-Platform", Application.platform.ToString());
        req.SetRequestHeader("X-Plex-Device-Name", deviceName);
    }

    #endregion

    #region Device Authorization
    public void InspectToken()
    {
        string storedToken = SessionInfoManager.LoadToken();

        if (!string.IsNullOrEmpty(storedToken))
        {
            //Token stored - Validate Token
            StartCoroutine(ValidateToken(storedToken));
        }
        else
        {
            Debug.Log("[PlexDataFetcher] [ValidateToken] | Plex Token was not found");
            OnTokenRequired?.Invoke();
        }
    }
    IEnumerator ValidateToken(string token)
    {
        string url = "https://plex.tv/api/resources?includeHttps=1";

        UnityWebRequest req = UnityWebRequest.Get(url);
        AttachPlexHeaders(req);
        req.SetRequestHeader("X-Plex-Token", token);

        yield return req.SendWebRequest();

        if(req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[PlexDataFetcher] [ValidateToken] | Stored Plex token is still valid");
            OnTokenValidation?.Invoke();
        }
        else
        {
            Debug.LogWarning($"[PlexDataFetcher] [ValidateToken] | Stored token is invalid.");
            OnTokenRequired?.Invoke();
        }
    }
    public void AuthorizeDevice()
    {
        StartCoroutine(GetPin());
    }
    IEnumerator GetPin()
    {
        string url = "https://plex.tv/pins.xml?strong=true";
        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "");
        AttachPlexHeaders(req);

        yield return req.SendWebRequest();

        if(req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[PlexDataFetcher] [GetPin] - Failed to get PIN. \n {req.error}");
            OnErrorOccured?.Invoke(req.error);
            yield break;
        }

        var xml = req.downloadHandler.text;
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        Debug.Log($"[PlexDataFetcher] [GetPin] - Pin Request XML \n {xml}");

        var pinNode = doc.SelectSingleNode("//pin");

        pinId = pinNode["id"]?.InnerText;
        userCode = pinNode["code"]?.InnerText;

        Debug.Log($"[PlexDataFetcher] [GetPin] | 4 digit code received: {userCode}");

        OnCodeReceived?.Invoke(userCode);

        StartCoroutine(PollForAuth(pinId));
    }
    IEnumerator PollForAuth(string pin) 
    {
        string url = $"https://plex.tv/pins/{pin}.xml";
        float timeout = 600f; //expires in 600 seconds (10 minutes)
        float timer = 0f;

        while (timer < timeout)
        {
            UnityWebRequest req = UnityWebRequest.Get(url);
            AttachPlexHeaders(req);
            yield return req.SendWebRequest();

            if(req.result == UnityWebRequest.Result.Success)
            {
                var xml = req.downloadHandler.text;
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                Debug.Log($"[PlexDataFetcher] [PollForAuth] | Poll XML: \n {xml}");

                var pinNode = doc.SelectSingleNode("//pin");
                var token = pinNode["auth_token"]?.InnerText;

                if (!string.IsNullOrEmpty(token))
                {
                    Debug.Log($"[PlexAuthManager] [PollForAuth] | Plex Auth Token Recieved: {token}");
                    OnTokenValidation?.Invoke();
                    SessionInfoManager.SaveToken(token);
                    yield break;
                }
            }

            Debug.Log($"[PlexAuthManager] [PollForAuth] | Polling... still waiting for user to link Plex.");

            yield return new WaitForSeconds(2f);
            timer += 2f;
        }

        Debug.LogError("Plex token request timed out.");
        OnErrorOccured?.Invoke("Token request timed out.");
    }

    #endregion

    #region Server Selection
    public void BuildServerList(string token)
    {
        StartCoroutine(GetPlexServer(token));
    }
    IEnumerator GetPlexServer(string token)
    {
        string url = "https://plex.tv/api/resources?includeHttps=1";
        UnityWebRequest req = UnityWebRequest.Get(url);
        AttachPlexHeaders(req);
        req.SetRequestHeader("X-Plex-Token", token);

        yield return req.SendWebRequest();

        if(req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[PlexAuthManager] [GetPlexServer] | Failed to get devices \n {req.error}");
            yield break;
        }

        string response = req.downloadHandler.text;

        Debug.Log($"[PlexAuthManager] [GetPlexServer] | Device Response \n {response}");

        //Create list of servers
        var doc = new XmlDocument();
        doc.LoadXml(response);

        var deviceNodes = doc.SelectNodes("//Device");

        foreach (XmlNode device in deviceNodes)
        {
            if (device.Attributes["provides"]?.Value == "server")
            {
                var server = new ServerInfo();

                server.name = device.Attributes["name"]?.Value;
                server.product = device.Attributes["product"]?.Value;
                server.provides = device.Attributes["provides"]?.Value;
                server.publicAddress = device.Attributes["publicAddress"]?.Value;

                var connections = device.SelectNodes("Connection");

                foreach (XmlNode connection in connections)
                {
                    var address = connection.Attributes["address"]?.Value;

                    if(address == server.publicAddress)
                    {
                        server.port = connection.Attributes["port"]?.Value;
                        server.uri = connection.Attributes["uri"]?.Value;
                    }

                }

                SessionInfoManager.AddServer(server);

                Debug.Log($"[PlexDataFetcher] [GetPlexServer] | Successfully added the following server: \n {server}");
            }
        }

        if (SessionInfoManager.GetCachedServers().Count == 0)
            Debug.LogWarning("No valid Plex server found.");
        else
        {
            SessionInfoManager.SaveServerList();
            OnServerListBuilt?.Invoke();
        }


    }

    #endregion

    #region Playlist Selection


    public void BuildPlaylistList(string token, string serverUri)
    {
        StartCoroutine(GetPlexPlaylist(token, serverUri));
    }
    IEnumerator GetPlexPlaylist(string token, string serverUri)
    {
        string url = $"{serverUri}/playlists";
        UnityWebRequest req = UnityWebRequest.Get(url);
        AttachPlexHeaders(req);
        req.SetRequestHeader("X-Plex-Token", token);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[PlexAuthManager] [GetPlexPlaylist] | Failed to get playlist \n {req.error}");
            yield break;
        }

        string response = req.downloadHandler.text;

        Debug.Log($"[PlexAuthManager] [GetPlexPlaylist] | Server Response \n {response}");

        //Create list of playlists
        var doc = new XmlDocument();
        doc.LoadXml(response);

        var plNodes = doc.SelectNodes("//Playlist");

        foreach (XmlNode pl in plNodes)
        {
            PlaylistInfo playlist = new PlaylistInfo();

            playlist.server = SessionInfoManager.LoadCurrentServer();
            playlist.title = pl.Attributes["title"]?.Value;
            playlist.ratingKey = pl.Attributes["ratingKey"]?.Value;
            playlist.uri = pl.Attributes["uri"]?.Value;
            playlist.movieCount = pl.Attributes["leafCount"]?.Value;

            SessionInfoManager.AddPlaylist(playlist);
            Debug.Log($"[PlexDataFetcher] [GetPlexPlaylist] | Successfully added the following playlist: \n {playlist}");
                
        }

        SessionInfoManager.SavePlaylistList();
        OnPlaylistBuilt?.Invoke();
    }
    public void FetchPlaylistItems()
    {
        StartCoroutine(FetchPlaylistItemsCoroutine());
    }

    IEnumerator FetchPlaylistItemsCoroutine()
    {
        string token = SessionInfoManager.LoadToken();
        string serverUri = SessionInfoManager.LoadCurrentServer().uri;
        string plRatingKey = SessionInfoManager.LoadCurrentPlaylist().ratingKey;

        string itemUrl = $"{serverUri}/playlists/{plRatingKey}/items?X-Plex-Token={token}";

        UnityWebRequest itemsReq = UnityWebRequest.Get(itemUrl);
        AttachPlexHeaders(itemsReq);

        yield return itemsReq.SendWebRequest();

        if(itemsReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to get playlist items: {itemsReq.error}");
            yield break;
        }

        string response = itemsReq.downloadHandler.text;
        Debug.Log($"[PlexAuthManager] [FetchPlaylistItemsCoroutine] | Playlist Response \n {response}");

        XmlDocument itemsDoc = new XmlDocument();
        itemsDoc.LoadXml(response);
        XmlNodeList videoNodes = itemsDoc.GetElementsByTagName("Video");

        int total = videoNodes.Count;

        for (int i =0; i < total; i++)
        {
            XmlNode video = videoNodes[i];

            var genreNodes = video.SelectNodes("Genre");
            List<string> genreList = new List<string>();

            var durationAttr = video.Attributes["duration"];
            var ratingKeyAttr = video.Attributes["ratingKey"];
            var titleAttr = video.Attributes["title"];
            var yearAttr = video.Attributes["year"];
            var summaryAttr = video.Attributes["summary"];
            var primaryExtraKeyAttr = video.Attributes["primaryExtraKey"];
            var thumbAttr = video.Attributes["thumb"];

            TimeSpan durationTime;

            foreach (XmlNode genreNode in genreNodes)
            {
                if (genreNode.Attributes["tag"] != null)
                    genreList.Add(genreNode.Attributes["tag"].Value);
            }


            var movie = new MovieInfo
            {
                playlist = SessionInfoManager.LoadCurrentPlaylist(),
                ratingKey = ratingKeyAttr?.Value,
                title = titleAttr?.Value,
                summary = summaryAttr?.Value,
                year = int.Parse(yearAttr?.Value ?? "0"),
                thumbUrl = $"{serverUri}{thumbAttr?.Value}?X-Plex-Token={token}",
                genres = string.Join(",", genreList),
                primaryExtraKey = primaryExtraKeyAttr?.Value,
                
            };

            if (durationAttr != null && long.TryParse(durationAttr.Value, out long durationMs))
            {
                durationTime = TimeSpan.FromMilliseconds(durationMs);
                movie.duration = $"{(int)durationTime.TotalHours}h {durationTime.Minutes:D2}m";
            }
            else
            {
                movie.duration = "N/A";
            }

            SessionInfoManager.AddMovie(movie);

            Debug.Log($"[FlexDataFetcher][FetchPlaylistItemsCoroutine] | Movie Successfully Added to the Movie List. " +
                $"\n New Count is {SessionInfoManager.GetPlaylistInfo().Count}." +
                $"\n Added Movie: {movie.title}" +
                $"\n {movie}");
        }

        SessionInfoManager.SavePlaylistMovieList();
    }
    
    #endregion





}
