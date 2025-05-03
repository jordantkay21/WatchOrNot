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
            $"\n - Summary: {summary} " +
            $"\n - Genres: {genres} " +
            $"\n - Duration: {duration} " +
            $"\n - Rating Key: {ratingKey}" +
            $"\n - Trailer URL : {trailerUrl}";
    }
}

public class SessionInfoManager
{
    private const string TokenKey = "plex_token";
    private const string ServerCacheFile = "servers.json";
    private const string PlaylistCacheFile = "playlists.json";

    private static List<ServerInfo> servers = new List<ServerInfo>();
    private static List<PlaylistInfo> playlists = new List<PlaylistInfo>();

    private static PlaylistInfo currentPlaylist;
    private static ServerInfo currentServer;
    private static List<MovieInfo> currentMovies = new List<MovieInfo>();


    #region Token Management

    public static bool HasToken() => PlayerPrefs.HasKey(TokenKey);

    public static void SaveToken(string token)
    {
        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
        PlayerPrefs.SetString(TokenKey, encoded);
        PlayerPrefs.Save();
    }

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
            Debug.LogWarning($"[SessionInfoManager][LoadToken()] Failed to decode stored Plex Token.");
            return null;
        }
    }

    #endregion

    #region Server Management

    public static bool HasCurrentServer() => currentServer != null;
    public static void SetCurrentServer(ServerInfo server) => currentServer = server;
    public static ServerInfo GetCurrentServer() => currentServer;

    public static void SetServers(List<ServerInfo> newServers)
    {
        servers = newServers;
        SaveJson(ServerCacheFile, servers);
    }

    public static List<ServerInfo> GetServers()
    {
        if (servers.Count == 0)
            servers = LoadJson<List<ServerInfo>>(ServerCacheFile) ?? new();
        
        return servers;
    }

    #endregion

    #region Playlist Management

    public static bool HasCurrentPlaylist() => currentPlaylist != null;
    public static void SetCurrentPlaylist(PlaylistInfo playlist) => currentPlaylist = playlist;
    public static PlaylistInfo GetCurrentPlaylist() => currentPlaylist;

    public static void SetPlaylists(List<PlaylistInfo> newPlaylists)
    {
        playlists = newPlaylists;
        SaveJson(PlaylistCacheFile, playlists);
    }

    public static List<PlaylistInfo> GetPlaylists()
    {
        if (playlists.Count == 0)
            playlists = LoadJson<List<PlaylistInfo>>(PlaylistCacheFile) ?? new();

        return playlists;
    }

    #endregion

    #region Movie Management

    public static List<MovieInfo> GetMovies() => currentMovies;

    public static void SetMovies(List<MovieInfo> movies)
    {
        currentMovies.Clear();

        currentMovies = movies;
    }


    #endregion

    #region Poster & Trailer Paths
    public static string GetPosterPath(MovieInfo movie)
    {
        string fileName = $"{Sanitize(movie.title)}_{movie.year}.png";
        return Path.Combine(Application.persistentDataPath, "PosterCache", fileName);
    }
    public static void SavePoster(MovieInfo movie, Texture2D tex)
    {
        string path = GetPosterPath(movie);

        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllBytes(path, tex.EncodeToPNG());
    }

    public static Texture2D LoadPoster(MovieInfo movie)
    {
        string path = GetPosterPath(movie);

        if (!File.Exists(path))
            return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            return tex;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SessionInfoManager][LoadPoster] Failed to load poster for {movie.title}: {ex.Message}");
            return null;
        }
    }

    public static string GetTrailerPath(MovieInfo movie)
    {
        string fileName = $"{Sanitize(movie.title)}_{movie.year}.mp4";
        return Path.Combine(Application.persistentDataPath, "Trailers", fileName);
    }
    #endregion

    #region Utility Methods

    private static void SaveJson<T>(string fileName, T data)
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, fileName);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }
        catch(Exception ex)
        {
            Debug.LogError($"[SessionInfoManager][SaveJson()] Failed to save JSON {fileName}: \n {ex.Message}");
        }
    }

    private static T LoadJson<T>(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(path)) return default;

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SessionInfoManager][LoadJson()] Failed to load JSON {fileName} : \n {ex.Message}");
            return default;
        }
    }

    private static string GetPlaylistMovieFile(string title, string movieCount)
    {
        string safeTitle = Sanitize(title);
        return $"playlist_{safeTitle}_{movieCount}.json";
    }

    private static string Sanitize(string input)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            input = input.Replace(c, '_');
        return input;
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.Save();
        servers.Clear();
        playlists.Clear();
        currentPlaylist = null;
        currentServer = null;
        currentMovies = null;
    }

    #endregion
}
