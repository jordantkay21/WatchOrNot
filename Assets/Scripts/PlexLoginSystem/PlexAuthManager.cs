using System;
using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class PlexAuthManager : MonoBehaviour
{
    [Serializable]
    private class PlexPinResponse
    {
        public int id;
        public string code;
        public string client_identifier;
        public string auth_token;
    }

    public static PlexAuthManager Instance;

    public string clientIdentifier = "watch-or-not-client"; //Must be unique
    public string product = "WatchOrNot";
    public string deviceName = "WatchOrNotGame";

    private string pinId;
    private string userCode;

    public event Action<string> OnCodeReceived;
    public event Action<string> OnTokenReceived;
    public event Action<string, int> OnServerDiscovered;
    public event Action<string> OnErrorOccured;

    private const string PlexHeaders = "application/json";


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

    public void InspectToken()
    {
        string storedToken = SessionInfoManager.LoadToken();

        if (!string.IsNullOrEmpty(storedToken))
        {
            //You have a saved token - validate it
            StartCoroutine(ValidateToken(storedToken));
        }
        else
        {
            //Begin login flow via plex.tv/link
            StartPlexLogin();
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
            Debug.Log("Stored Plex token is still valid.");
            OnTokenReceived?.Invoke(token);
            StartCoroutine(GetPlexServer(token));
        }
        else
        {
            Debug.LogWarning("Stored token is invalid. Starting new login...");
            StartPlexLogin();
        }
    }

    public void StartPlexLogin()
    {
        StartCoroutine(GetPin());
        PlexLoginUI.Instance.BeginLogin();
    }

    IEnumerator GetPin()
    {
        string url = "https://plex.tv/pins.xml?strong=true";
        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "");
        AttachPlexHeaders(req);

        yield return req.SendWebRequest();

        if(req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to get PIN: {req.error}");
            OnErrorOccured?.Invoke(req.error);
            yield break;
        }

        var xml = req.downloadHandler.text;
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        Debug.Log($"PlexAuthManager:GetPin - Pin Request XML \n {xml}");

        var pinNode = doc.SelectSingleNode("//pin");

        pinId = pinNode["id"]?.InnerText;
        userCode = pinNode["code"]?.InnerText;

        Debug.Log($"PlexAuthManager:GetPin - 4 digit code received: {userCode}");

        OnCodeReceived?.Invoke(userCode);

        StartCoroutine(PollForAuth(pinId));
    }

    IEnumerator PollForAuth(string pin)
    {
        string url = $"https://plex.tv/pins/{pin}.xml";
        float timeout = 600f; //expires in 10 mins
        float timer = 0f;

        while (timer < timeout)
        {
            UnityWebRequest req = UnityWebRequest.Get(url);
            AttachPlexHeaders(req);
            yield return req.SendWebRequest();

            if(req.result == UnityWebRequest.Result.Success)
            {
                var xml = req.downloadHandler.text;
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(xml);

                Debug.Log($"PlexAuthManager:PollForAuth - Poll XML: {xml}");

                var pinNode = doc.SelectSingleNode("//pin");
                var token = pinNode["auth_token"]?.InnerText;

                if (!string.IsNullOrEmpty(token))
                {
                    Debug.Log($"PlexAuthManager:PollForAuth - Plex Auth Token Recieved: {token}");
                    OnTokenReceived?.Invoke(token);
                    SessionInfoManager.SaveToken(token);
                    StartCoroutine(GetPlexServer(token));
                    yield break;
                }
            }

            Debug.Log($"PlexAuthManager:PollForAuth - Polling... still waiting for user to link Plex.");

            yield return new WaitForSeconds(2f);
            timer += 2f;
        }

        Debug.LogError("Plex token request timed out.");
        OnErrorOccured?.Invoke("Token request timed out.");
    }

    IEnumerator GetPlexServer(string token)
    {
        string url = "https://plex.tv/api/resources?includeHttps=1";
        UnityWebRequest req = UnityWebRequest.Get(url);
        AttachPlexHeaders(req);
        req.SetRequestHeader("X-Plex-Token", token);

        Debug.Log($"PlexAuthManager:PlexAuthManager - [URL] {req.url}");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to get servers: {req.error}");
            yield break;
        }

        string response = req.downloadHandler.text;
        Debug.Log($"Server response: {response}");

        // Optionally parse response for first server IP + port
        var doc = new XmlDocument();
        doc.LoadXml(response);

        var deviceNodes = doc.SelectNodes("//Device");

        foreach (XmlNode device in deviceNodes)
        {
            if (device.Attributes["provides"]?.Value == "server")
            {
                string serverName = device.Attributes["name"]?.Value;
                SessionInfoManager.SaveName(serverName);

                var connections = device.SelectNodes("Connection");

                foreach (XmlNode connection in connections)
                {
                    var local = connection.Attributes["local"]?.Value;
                    var address = connection.Attributes["address"]?.Value;
                    var portAttr = connection.Attributes["port"]?.Value;

                    if (local == "1" && !string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(portAttr))
                    {
                        int port = int.Parse(portAttr);

                        Debug.Log($"PlexAuthManager:GetPlexServer - Found {serverName} Plex Server : {address}:{port}");
                        SessionInfoManager.SaveServer(address, port);


                        OnServerDiscovered?.Invoke(address, port);
                        yield break;
                    }
                }
            }


        }

        Debug.LogWarning("No valid Plex server found.");
        OnErrorOccured?.Invoke("No valid Plex server found.");

    }

    private void AttachPlexHeaders(UnityWebRequest req)
    {
        req.SetRequestHeader("Accept", PlexHeaders);
        req.SetRequestHeader("X-Plex-Client-Identifier", clientIdentifier);
        req.SetRequestHeader("X-Plex-Product", product);
        req.SetRequestHeader("X-Plex-Version", "1.0");
        req.SetRequestHeader("X-Plex-Platform", Application.platform.ToString());
        req.SetRequestHeader("X-Plex-Device-Name", deviceName);
    }


}
