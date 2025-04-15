using System;
using System.Text;
using UnityEngine;

public class PlexStorageManager 
{
    private const string NameKey = "server_name";
    private const string TokenKey = "plex_token";
    private const string IPKey = "plex_ip";
    private const string PortKey = "plex_port";

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
    #endregion
}
