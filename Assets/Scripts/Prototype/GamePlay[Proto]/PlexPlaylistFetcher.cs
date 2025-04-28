using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class PlexPlaylistFetcher
{
    [System.Serializable]
    public class MovieListWrapper
    {
        public List<MovieInfo> movies;
    }

    private string plexToken;
    private string serverIP;
    private int port;
    private string playlistName;
    private string cacheFilePath;
    private string posterCachePath = Path.Combine(Application.persistentDataPath, "PosterCache");

    public PlexPlaylistFetcher(string token, string ip, int port, string playlistName)
    {
        this.plexToken = token;
        this.serverIP = ip;
        this.port = port;
        this.playlistName = playlistName;
        this.cacheFilePath = Path.Combine(Application.persistentDataPath, $"watch_or_not_{playlistName}_cache.json");

        if (!Directory.Exists(posterCachePath))
            Directory.CreateDirectory(posterCachePath);
    }

    public bool CacheExists()
    {
        return File.Exists(cacheFilePath);
    }

    public async Task<List<MovieInfo>> LoadFromCache(IProgress<float> progress = null, System.Action<string> statusCallback = null)
    {
        string json = File.ReadAllText(cacheFilePath);
        var movies = JsonUtility.FromJson<MovieListWrapper>(json).movies;

        int total = movies.Count;
        
        for (int i = 0; i < total; i++)
        {
            var movie = movies[i];

            statusCallback?.Invoke($"Loading poster: {movie.title}");

            if (movie.posterTexture == null)
                await LoadPosterAsync(movie);

            await TMDBMetadataFetcher.FetchMetadataAsync(movie);

            float percent = (float)(i + 1) / total;
            progress?.Report(percent);
        }

        //Re-Cache updated summary/trailer
        string updatedJson = JsonUtility.ToJson(new MovieListWrapper { movies = movies }, true);
        File.WriteAllText(cacheFilePath, updatedJson);

        return movies;
    }

    public async Task<List<MovieInfo>> FetchFromPlex(bool loadPosters = true, IProgress<float> progress = null, System.Action<string> statusCallback = null)
    {
        //Phase 1: Fetch Playlists
        statusCallback?.Invoke("Fetching playlists from Plex...");
        string playlistsUrl = $"http://{serverIP}:{port}/playlists?X-Plex-Token={plexToken}";
        UnityWebRequest playlistReq = UnityWebRequest.Get(playlistsUrl);
        await playlistReq.SendWebRequest();

        if (playlistReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to get playlists: " + playlistReq.error);
            return null;
        }

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(playlistReq.downloadHandler.text);
        XmlNodeList nodes = doc.GetElementsByTagName("Playlist");

        string playlistKey = null;
        foreach (XmlNode node in nodes)
        {
            if(node.Attributes["title"].Value == playlistName)
            {
                playlistKey = node.Attributes["ratingKey"].Value;
                break;
            }
        }

        if(playlistKey == null)
        {
            Debug.LogError("Playlist not found.");
            return null;
        }

        //Phase 2: Fetch Playlist Items
        statusCallback?.Invoke("Fetching playlist items...");
        string itemsUrl = $"http://{serverIP}:{port}/playlists/{playlistKey}/items?X-Plex-Token={plexToken}";
        UnityWebRequest itemsReq = UnityWebRequest.Get(itemsUrl);
        await itemsReq.SendWebRequest();

        if (itemsReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failled to get playlist items: {itemsReq.error}");
            return null;
        }

        XmlDocument itemsDoc = new XmlDocument();
        itemsDoc.LoadXml(itemsReq.downloadHandler.text);
        XmlNodeList videoNodes = itemsDoc.GetElementsByTagName("Video");

        List<MovieInfo> movies = new List<MovieInfo>();
        int total = videoNodes.Count;

        //Phase 3: Process each movie and load posters
        for(int i =0; i < total; i++)
        {
            XmlNode video = videoNodes[i];
            
            var genreNodes = video.SelectNodes("Genre");
            List<string> genreList = new List<string>();

            foreach (XmlNode genreNode in genreNodes)
            {
                if (genreNode.Attributes["tag"] != null)
                    genreList.Add(genreNode.Attributes["tag"].Value);
            }


            var movie = new MovieInfo
            {
                title = video.Attributes["title"].Value,
                year = int.Parse(video.Attributes["year"].Value),
                ratingKey = video.Attributes["ratingKey"].Value,
                thumbUrl = $"http://{serverIP}:{port}{video.Attributes["thumb"].Value}?X-Plex-Token={plexToken}",
                genres = string.Join(", ", genreList),
            };

            var durationAttr = video.Attributes["duration"];
            if(durationAttr != null && long.TryParse(durationAttr.Value, out long durationMs))
            {
                TimeSpan durationTime = TimeSpan.FromMilliseconds(durationMs);
                movie.duration = $"{(int)durationTime.TotalHours}h {durationTime.Minutes:D2}m";
            }
            else
            {
                movie.duration = "Unknown";
            }

            statusCallback?.Invoke($"Processing movie {i + 1} of {total}: {movie.title}");

            if (loadPosters)
                await LoadPosterAsync(movie);

            await TMDBMetadataFetcher.FetchMetadataAsync(movie);

            movies.Add(movie);

            //Report progress based on movie processing
            progress?.Report((float)(i + 1) / total);
        }

        //Cache teh result as JSON
        string json = JsonUtility.ToJson(new MovieListWrapper { movies = movies }, true);
        File.WriteAllText(cacheFilePath, json);

        return movies;
    }

    private async Task LoadPosterAsync(MovieInfo movie)
    {
        string fileName = $"{SanitizeFileName(movie.title)}_{movie.year}.png";
        string filePath = Path.Combine(posterCachePath, fileName);

        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            movie.posterTexture = texture;
            return;
        }


        //Not Cached download from Plex
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(movie.thumbUrl);
        var op = req.SendWebRequest();

        while (!op.isDone)
            await Task.Yield();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = ((DownloadHandlerTexture)req.downloadHandler).texture;
            movie.posterTexture = tex;

            //save to disk
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
        }
        else
            Debug.LogError($"Failed to load poster for {movie.title} : {req.error}");
    }

    private string SanitizeFileName(string input)
    {
        foreach(char c in Path.GetInvalidFileNameChars())
        {
            input = input.Replace(c, '_');
        }

        return input;
    }
}
