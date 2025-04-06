using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string plexToken = "YOUR_PLEX_TOKEN";
    public string plexIp = "SERVER_IP";
    public int plexPort = 32400;
    public string playlistName = "PLAYLIST_NAME";
    public List<MovieInfo> loadedMovies;

    [SerializeField] UIManager uiManager;

    private RankingSystem rankingSystem;
    private MovieInfo currentMovie;


    async void Start()
    {
        PlexPlaylistFetcher fetcher = new PlexPlaylistFetcher(plexToken, plexIp, plexPort, playlistName);

        if (fetcher.CacheExists())
        {
            loadedMovies = await fetcher.LoadFromCache();
            Debug.Log($"{loadedMovies.Count} movies loaded from cache.");
        }
        else
        {
            loadedMovies = await fetcher.FetchFromPlex();
            Debug.Log($"{loadedMovies?.Count} movies loaded from Plex.");
        }

        if (loadedMovies == null || loadedMovies.Count == 0)
        {
            Debug.LogError("No Movies loaded from playlist");
            return;
        }

        rankingSystem = new RankingSystem(loadedMovies);
        ShowNextMovie();

        for (int i = 0; i < uiManager.rankButtons.Length; i++)
        {
            int rank = i + 1;
            uiManager.rankButtons[i].onClick.AddListener(() => OnRankButtonClicked(rank));
        }
    }

    private void OnRankButtonClicked(int rank)
    {
        if (rankingSystem.AssignRank(currentMovie, rank))
        {
            uiManager.DisableRank(rank);
            ShowNextMovie();
        }
        else
        {
            uiManager.ShowError($"Rank {rank} is already used!");
        }
    }

    public void ShowNextMovie()
    {
        currentMovie = rankingSystem.GetNextMovie();
        if(currentMovie == null)
        {
            uiManager.ShowResults(rankingSystem.GetRankedResults());
        }
        else
        {
            uiManager.DisplayMovie(currentMovie);
        }


    }
}
