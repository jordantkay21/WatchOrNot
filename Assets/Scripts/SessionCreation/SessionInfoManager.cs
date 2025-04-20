using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class ServerInfo
{
    public string name;
    public string product;
    public string provides;
    public string publicAddress;
    public string port;
    public string uri;

    public override string ToString()
    {
        return
            $"- Name: {name} " +
            $"\n - Product: {product} " +
            $"\n - Provides: {provides} " +
            $"\n - IP: {publicAddress} " +
            $"\n - Port: {port} " +
            $"\n - URI: {uri}";
    }
}

[Serializable]
public class PlaylistInfo
{
    public ServerInfo server;
    public string title;
    public string ratingKey;
    public string uri;
    public string movieCount = "leafCount";

    public override string ToString()
    {
        return
            $" <color=red>- Server: {server.name} </color>" +
            $"\n - Title: {title} " +
            $"\n - RatingKey {ratingKey} " +
            $"\n - URI: {uri} " +
            $"\n - movieCount = {movieCount}";
            
    }
}

[Serializable]
public class MovieInfo
{
    public PlaylistInfo playlist;
    public string title;
    public int year;
    public string ratingKey;
    public string thumbUrl;
    public Texture2D posterTexture;

    public string summary;
    public string trailerUrl;
    public string genres;
    public string duration;
    public string primaryExtraKey;

    public override string ToString()
    {
        return
            $"<color=red>- Playlist: {playlist.title} </color>" +
            $"\n - Title: {title}" +
            $"\n - Year: {year}" +
            $"\n - Rating Key: {ratingKey}" +
            $"\n - Summary: {summary} " +
            $"\n - Genres: {genres} " +
            $"\n - Duration: {duration} ";
    }
}

public class SessionInfoManager 
{
    private const string NameKey = "server_name";
    private const string TokenKey = "server_token";
    private const string IPKey = "server_ip";
    private const string PortKey = "server_port";
    private const string PlaylistName = "playlist_name";
    private const string PlaylistCount = "playlist_movieCount";

    private static readonly List<ServerInfo> serverList = new();
    private static ServerInfo currentServer;

    private static readonly List<PlaylistInfo> playlistList = new();
    private static PlaylistInfo currentPlaylist;

    private static readonly List<MovieInfo> movieInfoList = new();

    #region Wrapper Classes
    [Serializable]
    public class ServerInfoListWrapper
    {
        public List<ServerInfo> servers;
    }

    [Serializable]
    public class PlaylistInfoListWrapper
    {
        public List<PlaylistInfo> playlists;
    }

    [Serializable]
    public class MovieListWrapper
    {
        public List<MovieInfo> movies;
    }
    #endregion
    #region Save Methods
    public static void SaveName(string name)
    {
        PlayerPrefs.SetString(NameKey, name);
        PlayerPrefs.Save();
    }

    public static void SaveToken(string token)
    {
        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
        PlayerPrefs.SetString(TokenKey, encoded);
        PlayerPrefs.Save();
    }

    public static void SaveServer(string ip, int port)
    {
        PlayerPrefs.SetString(IPKey, ip);
        PlayerPrefs.SetInt(PortKey, port);
        PlayerPrefs.Save();
    }

    public static void SaveServerList()
    {
        ServerInfoListWrapper wrapper = new ServerInfoListWrapper { servers = serverList };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("server_list", json);
        PlayerPrefs.Save();
    }

    public static void SavePlaylistList()
    {
        PlaylistInfoListWrapper wrapper = new PlaylistInfoListWrapper { playlists = playlistList };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("playlist_list", json);
        PlayerPrefs.Save();

        Debug.Log($"[SessionInfoManager] [SavePlaylistList] | List of playlists have been saved.");
    }

    #endregion

    #region Load Methods

    public static string LoadToken()
    {
        if (!PlayerPrefs.HasKey(TokenKey)) return null;

        try
        {
            string encoded = PlayerPrefs.GetString(TokenKey);
            return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }
        catch
        {
            Debug.LogWarning("Failed to decode stored Plex Token.");
            return null;
        }
    }

    public static List<ServerInfo> LoadSavedServers()
    {
        if (!PlayerPrefs.HasKey("server_list")) return new List<ServerInfo>();

        string json = PlayerPrefs.GetString("server_list");

        try
        {
            ServerInfoListWrapper wrapper = JsonUtility.FromJson<ServerInfoListWrapper>(json);
            return wrapper?.servers ?? new List<ServerInfo>();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SessionInfoManager] [LoadServers] | Failed to deserialize server list: \n {ex.Message}");
            return new List<ServerInfo>();
        }
    }

    public static List<PlaylistInfo> LoadSavedPlaylists()
    {
        if (!PlayerPrefs.HasKey("playlist_list")) return new List<PlaylistInfo>();

        string json = PlayerPrefs.GetString("playlist_list");

        try
        {
            PlaylistInfoListWrapper wrapper = JsonUtility.FromJson<PlaylistInfoListWrapper>(json);
            return wrapper?.playlists ?? new List<PlaylistInfo>();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SessionInfoManager] [LoadServers] | Failed to deserialize playlist list: \n {ex.Message}");
            return new List<PlaylistInfo>();
        }
    }

    public static string LoadIP() => PlayerPrefs.GetString(IPKey, "localhost");
    public static int LoadPort() => PlayerPrefs.GetInt(PortKey, 32400);
    public static string LoadName() => PlayerPrefs.GetString(NameKey, "local server");

    public static ServerInfo LoadCurrentServer() => currentServer;
    public static PlaylistInfo LoadCurrentPlaylist() => currentPlaylist;

    #endregion

    #region Utility Methods

    public static bool HasToken() => PlayerPrefs.HasKey(TokenKey);
    public static bool HasServerInfo() => PlayerPrefs.HasKey(IPKey) && PlayerPrefs.HasKey(PortKey);
    public static bool HasCurrentServer() => currentServer != null;
    public static void AddServer(ServerInfo server) => serverList.Add(server);
    public static void AddPlaylist(PlaylistInfo playlist) => playlistList.Add(playlist);
    public static void SetCurrentServer(ServerInfo server) => currentServer = server;
    public static void SetCurrentPlaylist(PlaylistInfo playlist) => currentPlaylist = playlist;
    public static List<ServerInfo> GetCachedServers() => serverList;
    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(IPKey);
        PlayerPrefs.DeleteKey(PortKey);
        PlayerPrefs.Save();
    }
    #endregion
    public static void AddMovie(MovieInfo movie) => movieInfoList.Add(movie);
    public static List<MovieInfo> GetPlaylistInfo() => movieInfoList;

    private static string SanitizeFileName(string input)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            input = input.Replace(c, '_');
        return input;
    }
    private static string GetPlaylistCacheFilePath(string title, string movieCount)
    {
        string safeTitle = SanitizeFileName(title);
        string fileName = $"playlist_{safeTitle}_{movieCount}.json";
        return Path.Combine(Application.persistentDataPath, fileName);
    }


    public static void SavePlaylistMovieList()
    {
        string path = GetPlaylistCacheFilePath(currentPlaylist.title, movieInfoList.Count.ToString());

        MovieListWrapper wrapper = new MovieListWrapper { movies = movieInfoList };
        string json = JsonUtility.ToJson(wrapper, true);

        File.WriteAllText(path, json);
        Debug.Log($"[SessionInfoManager] [SavePlaylistMovieList] | Saved movie list to: {path}");
    }

    public static List<MovieInfo> LoadPlaylistMovieList(string title, string movieCount)
    {
        string path = GetPlaylistCacheFilePath(title, movieCount);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SessionInfoManager] [LoadPlaylistMovieList] | File not found: {path}");
            return new List<MovieInfo>();
        }

        string json = File.ReadAllText(path);
        try
        {
            MovieListWrapper wrapper = JsonUtility.FromJson<MovieListWrapper>(json);
            return wrapper?.movies ?? new List<MovieInfo>();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SessionInfoManager] [LoadPlaylistMovieList] | Failed to parse file: {e.Message}");
            return new List<MovieInfo>();
        }
    }

    public static void SavePoster(MovieInfo movie, Texture2D tex)
    {
        string filename = $"{ SanitizeFileName(movie.title)}_{movie.year}.png";
        string path = Path.Combine(Application.persistentDataPath, "PosterCache", filename);

        if (!Directory.Exists(Path.GetDirectoryName(path)))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        File.WriteAllBytes(path, tex.EncodeToPNG());
    }

    public static string GetTrailerPath(MovieInfo movie)
    {
        string filename = $"{SanitizeFileName(movie.title)}_{movie.year}.mp4";
        return Path.Combine(Application.persistentDataPath, "Trailers", filename);
    }

    public static void SetTrailerPath(MovieInfo movie, string path)
    {
        movie.trailerUrl = path;
    }

    public static string GetPosterPath(MovieInfo movie)
    {
        string fileName = $"{SanitizeFileName(movie.title)}_{movie.year}.png";
        return Path.Combine(Application.persistentDataPath, "PosterCache", fileName);
    }

    public static void ClearPlaylistInfo()
    {
        movieInfoList.Clear();
    }
}
