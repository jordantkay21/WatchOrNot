using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public static class PlexNetworkingManager
{
    public static event Action<string> OnCodeReceived;
    public static event Action OnTokenValidated;
    public static event Action<string> OnStatusUpdate;
    public static event Action<string> OnErrorOccurred;

    private static string pinID;
    private static string userCode;

    private const string clientIdentifier = "watch-or-not-client";
    private const string product = "WatchOrNot";
    private const string deviceName = "WatchOrNotGame";

    private static void AttachHeaders(HttpRequestMessage request)
    {
        request.Headers.Add("Accept", "application/xml");
        request.Headers.Add("X-Plex-Client-Identifier", clientIdentifier);
        request.Headers.Add("X-Plex-Product", product);
        request.Headers.Add("X-Plex-Version", "1.0");
        request.Headers.Add("X-Plex-Platform", Application.platform.ToString());
        request.Headers.Add("X-Plex-Device-Name", deviceName);
    }

    #region Token Configuration
    public static async Task AuthorizeDeviceAsync()
    {
        try
        {
            using HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://plex.tv/pins.xml?strong=true");
            AttachHeaders(request);

            var response = await client.SendAsync(request);
            var xml = await response.Content.ReadAsStringAsync();

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var pinNode = doc.SelectSingleNode("//pin");
            pinID = pinNode["id"]?.InnerText;
            userCode = pinNode["code"]?.InnerText;

            Debug.Log($"[PlexNetworkingManager][AuthorizeDeviceAsync] Device Code: {userCode}");

            OnCodeReceived?.Invoke(userCode);

            await PollForAuthAsync(pinID);
        }
        catch (Exception ex)
        {
            OnErrorOccurred?.Invoke(ex.Message);
        }
    } 

    private static async Task PollForAuthAsync(string pin)
    {
        float timeout = 600f;
        float interval = 2f;
        float elapsed = 0f;

        using HttpClient client = new HttpClient();

        while (elapsed < timeout)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://plex.tv/pins/{pin}.xml");
                AttachHeaders(request);

                var response = await client.SendAsync(request);
                string xml = await response.Content.ReadAsStringAsync();

                var doc = new XmlDocument();
                doc.LoadXml(xml);
                var token = doc.SelectSingleNode("//pin")?["auth_token"]?.InnerText;

                if (!string.IsNullOrEmpty(token))
                {
                    Debug.Log($"[PlexNetworkingManager][PollForAuthAsync] Token received: {token}");
                    SessionInfoManager.SaveToken(token);
                    OnTokenValidated?.Invoke();
                    return;
                }
            }
            catch(Exception ex)
            {
                Debug.LogWarning($"[PlexNetworkingManager][PollForAuthAsync] Polling Failed: {ex.Message}");
            }

            await Task.Delay((int)(interval * 1000));
            elapsed += interval;
        }

        OnErrorOccurred?.Invoke("Authorization timed out.");
    }

    public static async Task ValidateStoredTokenAsync()
    {
        string token = SessionInfoManager.LoadToken();
        if (string.IsNullOrEmpty(token))
        {
            OnErrorOccurred?.Invoke("No token found.");
            return;
        }

        try
        {
            using HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://plex.tv/api/resources?includeHttps=1");
            AttachHeaders(request);
            request.Headers.Add("X-Plex-Token", token);

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                OnTokenValidated?.Invoke();
            }
            else
            {
                OnErrorOccurred?.Invoke("Token validation failed.");
            }
        }
        catch(Exception ex)
        {
            OnErrorOccurred?.Invoke(ex.Message);
        }
    }
    #endregion

    #region Fetch Logic

    public static async Task<List<ServerInfo>> FetchServersAsync()
    {
        try
        {
            string token = SessionInfoManager.LoadToken();
            if (string.IsNullOrEmpty(token)) throw new Exception("No Plex Token Found.");

            using HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://plex.tv/api/resources?includeHttps=1");
            AttachHeaders(request);
            request.Headers.Add("X-Plex-Token", token);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string xml = await response.Content.ReadAsStringAsync();

            Debug.Log($"[PlexNetworkingManager][FetchServersAsync] Server XML Retrieved: \n {xml}");

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var devices = doc.SelectNodes("//Device");

            List<ServerInfo> servers = new List<ServerInfo>();
            foreach (XmlNode device in devices)
            {
                if (device.Attributes["provides"]?.Value != "server") continue;

                var server = new ServerInfo
                {
                    name = device.Attributes["name"]?.Value,
                    product = device.Attributes["product"]?.Value,
                    provides = device.Attributes["provides"]?.Value,
                    publicAddress = device.Attributes["publicAddress"]?.Value
                };

                var connection = device.SelectSingleNode("Connection");
                if (connection != null)
                {
                    server.uri = connection.Attributes["uri"]?.Value;
                    server.port = connection.Attributes["port"]?.Value;
                }

                servers.Add(server);

                Debug.Log($"[PlexNetworkingManager][FetchServersAsync] Successfully added the following Server: \n {server}");
            }

            SessionInfoManager.SetServers(servers);
            return servers;
        }
        catch (Exception ex)
        {
            OnErrorOccurred?.Invoke($"[PlexNetworkingManager][FetchServersAsync] FetchServersAsync error: {ex.Message}");
            return new List<ServerInfo>();
        }
    }

    public static async Task<List<PlaylistInfo>> FetchPlaylistAsync(ServerInfo server)
    {
        try
        {
            string token = SessionInfoManager.LoadToken();
            if (string.IsNullOrEmpty(token)) throw new Exception("No Plex Token Found.");

            using HttpClient client = new HttpClient();
            string url = $"{server.uri}/playlists";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AttachHeaders(request);
            request.Headers.Add("X-Plex-Token", token);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string xml = await response.Content.ReadAsStringAsync();

            Debug.Log($"[PlexNetworkingManager][FetchPlaylistAsync] Playlists XML Retrieved: \n {xml}");

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var playlistsXml = doc.SelectNodes("//Playlist");

            List<PlaylistInfo> playlists = new List<PlaylistInfo>();
            foreach (XmlNode pl in playlistsXml)
            {
                var playlist = new PlaylistInfo
                {
                    server = server,
                    title = pl.Attributes["title"]?.Value,
                    ratingKey = pl.Attributes["ratingKey"]?.Value,
                    uri = pl.Attributes["uri"]?.Value,
                    movieCount = pl.Attributes["leafCount"]?.Value
                };

                playlists.Add(playlist);

                Debug.Log($"[PlexNetworkingManager][FetchPlaylistAsync] Successfully added the following Playlist: \n {playlist}");
            }

            SessionInfoManager.SetPlaylists(playlists);
            return playlists;
        }
        catch (Exception ex)
        {
            OnErrorOccurred?.Invoke($"[PlexNetworkingManager][FetchPlaylistAsync] Error occured fetching playlists: {ex.Message}");
            return new List<PlaylistInfo>();
        }
    }

    public static async Task<List<MovieInfo>> FetchPlaylistItemsAsync(PlaylistInfo playlist)
    {
        List<MovieInfo> movies = new List<MovieInfo>();

        try
        {
            string token = SessionInfoManager.LoadToken();
            if (string.IsNullOrEmpty(token)) throw new Exception("No Plex Token Found.");

            using HttpClient client = new HttpClient();
            string url = $"{playlist.server.uri}/playlists/{playlist.ratingKey}/items?X-Plex-Token={token}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AttachHeaders(request);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string xml = await response.Content.ReadAsStringAsync();

            Debug.Log($"[PlexNetworkingManager][FetchPlaylistItemsAsync] Playlist Items XML Retrieved: \n {xml}");

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var videos = doc.SelectNodes("//Video");

            int completedCount = 0;
            int totalCount = videos.Count;

            foreach (XmlNode video in videos)
            {
                var movie = new MovieInfo
                {
                    playlist = playlist,
                    title = video.Attributes["title"]?.Value,
                    year = int.Parse(video.Attributes["year"]?.Value ?? "0"),
                    ratingKey = video.Attributes["ratingKey"]?.Value,
                    thumbUrl = $"{playlist.server.uri}{video.Attributes["thumb"]?.Value}?X-Plex-Token={token}",
                    summary = video.Attributes["summary"]?.Value,
                    primaryExtraKey = video.Attributes["primaryExtraKey"]?.Value
                };

                //Genres
                var genreNodes = video.SelectNodes("Genre");
                List<string> genreList = new List<string>();

                foreach (XmlNode genreNode in genreNodes)
                    genreList.Add(genreNode.Attributes["tag"]?.Value);

                movie.genres = string.Join(",", genreList);

                //Duration
                if (long.TryParse(video.Attributes["duration"]?.Value, out long durationMs))
                {
                    TimeSpan duration = TimeSpan.FromMilliseconds(durationMs);
                    movie.duration = $"{(int)duration.TotalHours}h {duration.Minutes:D2}m";
                }
                else
                {
                    movie.duration = "N/A";
                }

                movies.Add(movie);

                // Download poster and trailer for this movie
                await DownloadPosterAndTrailerAsync(movie);

                //Update Loading progress after each movie
                completedCount++;
                float progress = (float)completedCount / totalCount;
                MainThreadDispatcher.Enqueue(() => OnStatusUpdate?.Invoke($"Loading {completedCount}/{totalCount} movies | {movie.title} | ({progress:P0})"));

                Debug.Log($"[PlexNetworkingManager][FetchPlaylistItemsAsync] Successfully added the following movie: \n {movie}");
            }

            SessionInfoManager.SetMovies(movies);
        }
        catch (Exception ex)
        {
            OnErrorOccurred?.Invoke($"[PlexNetworkingManager][FetchPlaylistItemsAsync] Error occured fetching playlist items: {ex.Message}");
        }

            return movies;
    }
    #endregion

    #region Movie Asset Logic

    private static async Task DownloadPosterAndTrailerAsync(MovieInfo movie)
    {
        Task<Texture2D> posterTask = DownloadPosterTextureAsync(movie);
        Task<string> trailerTask = TMDBNetworkingManager.FetchTrailerUrlAsync(movie.title, movie.year);

        await Task.WhenAll(posterTask, trailerTask);

        movie.posterTexture = posterTask.Result;
        movie.trailerUrl = trailerTask.Result;
    }

    public static async Task<Texture2D> DownloadPosterTextureAsync(MovieInfo movie)
    {

        //first try to load from cache
        var cachedTex = SessionInfoManager.LoadPoster(movie);
        if (cachedTex != null)
            return cachedTex;

        //Otherwise download
        if (string.IsNullOrEmpty(movie.thumbUrl))
            return null;

        try
        {
            using HttpClient client = new HttpClient();
            var bytes = await client.GetByteArrayAsync(movie.thumbUrl);

            Texture2D tex = new Texture2D(2, 2); // Size will auto-correct
            tex.LoadImage(bytes);
            return tex;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PlexNetworkingManager][DownloadPosterTextureAsync] Failed to download image: {ex.Message}");
            return null;
        }
    }
    #endregion
}
