using System;
using System.Collections.Generic;
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
    public string ratingKey;
    public string title;
    public string movieCount = "leafCount";
}

[Serializable]
public class MovieInfo
{
    public string title;
    public int year;
    public string ratingKey;
    public string thumbUrl;
    public Texture2D posterTexture;

    public string summary;
    public string trailerUrl;
    public string genres;
    public string duration;
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

    public static string LoadIP() => PlayerPrefs.GetString(IPKey, "localhost");
    public static int LoadPort() => PlayerPrefs.GetInt(PortKey, 32400);
    public static string LoadName() => PlayerPrefs.GetString(NameKey, "local server");

    #endregion

    #region Utility Methods

    public static bool HasToken() => PlayerPrefs.HasKey(TokenKey);
    public static bool HasServerInfo() => PlayerPrefs.HasKey(IPKey) && PlayerPrefs.HasKey(PortKey);

    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(IPKey);
        PlayerPrefs.DeleteKey(PortKey);
        PlayerPrefs.Save();
    }

    public static void AddServer(ServerInfo server)
    {
        serverList.Add(server);
    }
    public static void SetCurrentServer(ServerInfo server)
    {
        currentServer = server;
    }

    public static ServerInfo GetCurrentServer()
    {
        return currentServer;
    }

    public static List<ServerInfo> GetCachedServers()
    {
        return serverList;
    }
    #endregion

    #region Wrapper Classes
    [Serializable]
    public class ServerInfoListWrapper
    {
        public List<ServerInfo> servers;
    }
    #endregion
}
