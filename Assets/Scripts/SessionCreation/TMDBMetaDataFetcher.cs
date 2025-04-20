using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class TMDBMetadataFetcher
{
    private const string apiKey = "3d1fa5b3aa21c8c23a43b759e8f46190";
    private static readonly string cacheDir = Path.Combine(Application.persistentDataPath, "MetadataCache");

    public static async Task FetchMetadataAsync(MovieInfo movie)
    {
        string cachePath = Path.Combine(cacheDir, $"{Sanitize(movie.title)}_{movie.year}.json");

        if (!Directory.Exists(cacheDir))
            Directory.CreateDirectory(cacheDir);

        if (File.Exists(cachePath))
        {
            string cachedJson = File.ReadAllText(cachePath);
            ParseTMDBJson(cachedJson, movie);
            return;
        }

        try
        {
            string query = $"https://api.themoviedb.org/3/search/movie?api_key={apiKey}&query={Uri.EscapeDataString(movie.title)}&year={movie.year}";
            string searchJson = await GetAsync(query);
            int id = ExtractMovieId(searchJson);

            if (id == -1) return;

            string detailJson = await GetAsync($"https://api.themoviedb.org/3/movie/{id}?api_key={apiKey}");
            string videoJson = await GetAsync($"https://api.themoviedb.org/3/movie/{id}/videos?api_key={apiKey}");

            string combined = $"{{\"details\":{detailJson},\"videos\":{videoJson}}}";
            File.WriteAllText(cachePath, combined);

            ParseTMDBJson(combined, movie);
        }
        catch (Exception e)
        {
            Debug.LogError($"TMDB fetch error for {movie.title}: {e.Message}");
        }
    }

    private static async Task<string> GetAsync(string url)
    {
        using HttpClient client = new HttpClient();
        return await client.GetStringAsync(url);
    }

    private static int ExtractMovieId(string searchJson)
    {
        var result = JsonUtility.FromJson<TMDBSearchResultWrapper>(searchJson);
        return result.results.Length > 0 ? result.results[0].id : -1;
    }

    private static void ParseTMDBJson(string json, MovieInfo movie)
    {
        var root = JsonUtility.FromJson<TMDBCombined>(json);
        movie.summary = root.details?.overview ?? "";

        foreach (var v in root.videos?.results ?? Array.Empty<TMDBVideo>())
        {
            if (v.site == "YouTube" && v.type == "Trailer")
            {
                movie.trailerUrl = $"https://www.youtube.com/watch?v={v.key}";
                break;
            }
        }
    }

    private static string Sanitize(string input)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            input = input.Replace(c, '_');
        return input;
    }

    // Internal TMDB types
    [Serializable] private class TMDBSearchResultWrapper { public TMDBSearchResult[] results; }
    [Serializable] private class TMDBSearchResult { public int id; }
    [Serializable] private class TMDBCombined { public TMDBDetails details; public TMDBVideoWrapper videos; }
    [Serializable] private class TMDBDetails { public string overview; }
    [Serializable] private class TMDBVideoWrapper { public TMDBVideo[] results; }
    [Serializable] private class TMDBVideo { public string site, key, type; }
}
