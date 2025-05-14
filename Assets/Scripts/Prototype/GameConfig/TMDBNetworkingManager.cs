using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

namespace KayosMedia.WatchOrNot.Prototype
{
    [Serializable]
    public class TMDbSearchResult
    {
        public List<TMDbMovieResult> results;
    }

    [Serializable]
    public class TMDbMovieResult
    {
        public int id;
        public string title;
        public int year;
    }
    [Serializable]
    public class TMDbVideoResult
    {
        public List<TMDbVideoInfo> results;
    }

    [Serializable]
    public class TMDbVideoInfo
    {
        public string site;
        public string type;
        public string key;
    }

    public static class TMDBNetworkingManager
    {
        private static readonly string apiKey = "3d1fa5b3aa21c8c23a43b759e8f46190";
        private static readonly string baseUrl = "https://api.themoviedb.org/3";

        /// <summary>
        /// Searches TMDB for the movie, finds the official trailer, and returns a YouTube URL
        /// </summary>
        public static async Task<string> FetchTrailerUrlAsync(string movieTitle, int year)
        {
            try
            {
                int movieId = await SearchMovieAsync(movieTitle, year);

                if (movieId == -1)
                {
                    Debug.LogWarning($"[TMDBNetworkingManager][FetchTrailerUrlAsync] Movie '{movieTitle} ({year})' not found on TMDB.");
                    return null;
                }

                string youtubeKey = await GetTrailerYoutubeKeyAsync(movieId);
                if (string.IsNullOrEmpty(youtubeKey))
                {
                    Debug.LogWarning($"[TMDBNetworkingManager][FetchTrailerUrlAsync] No trailer found for movie ID {movieId}.");
                    return null;
                }

                return $"https://www.youtube.com/watch?v={youtubeKey}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TMDBNetworkingManager][FetchTrailerUrlAsync] Error fetching trailer: {ex.Message}");
                return null;
            }
        }

        private static async Task<int> SearchMovieAsync(string title, int year)
        {
            using HttpClient client = new HttpClient();
            string query = Uri.EscapeDataString(title);
            string url = $"{baseUrl}/search/movie?api_key={apiKey}&query={query}&year={year}";

            //Debug.Log($"[TMDbNetworkManager][SearchMovieAsync] Searching movie on TMDB using url: \n {url}");

            var response = await client.GetStringAsync(url);
            var searchResult = JsonUtility.FromJson<TMDbSearchResult>(response);

            //Debug.Log($"[TMDbNetworkManager][SearchMovieAsync] Movie Search Result: \n response: {response}");

            if (searchResult.results != null && searchResult.results.Count > 0)
                return searchResult.results[0].id;

            return -1;


        }

        private static async Task<string> GetTrailerYoutubeKeyAsync(int movieId)
        {
            using HttpClient client = new HttpClient();
            string url = $"{baseUrl}/movie/{movieId}/videos?api_key={apiKey}";

            //Debug.Log($"[TMDbNetworkManager][GetTrailerYoutubeAsync] Searching movie on TMDB using url: \n {url}");

            var response = await client.GetStringAsync(url);
            var videoResult = JsonUtility.FromJson<TMDbVideoResult>(response);

            //Debug.Log($"[TMDbNetworkManager][GetTrailerYoutubeAsync] Movie Search Result: \n response: {response}");

            if (videoResult.results != null)
            {
                foreach (var video in videoResult.results)
                {
                    if (video.site == "YouTube" && video.type == "Trailer")
                        return video.key;
                }
            }

            return null;
        }
    }
}