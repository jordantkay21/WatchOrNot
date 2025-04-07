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

    [SerializeField] RankingUIManager RankingUiManager;

    private RankingSystem rankingSystem;
    private MovieInfo currentMovie;


    async void Start()
    {
        PlexPlaylistFetcher fetcher = new PlexPlaylistFetcher(plexToken, plexIp, plexPort, playlistName);
        var progress = new Progress<float>(p => LoadingUIManager.Instance.UpdateProgress(p));
        System.Action<string> status = msg => LoadingUIManager.Instance.UpdateStatus(msg);

        RankingUiManager.HideMovieInfo();
        LoadingUIManager.Instance.Show();

        if (fetcher.CacheExists())
        {
            loadedMovies = await fetcher.LoadFromCache(progress, status);
            Debug.Log($"{loadedMovies.Count} movies loaded from cache.");
        }
        else
        {
            loadedMovies = await fetcher.FetchFromPlex(true,progress, status);
            Debug.Log($"{loadedMovies?.Count} movies loaded from Plex.");
        }

        LoadingUIManager.Instance.Hide();

        if (loadedMovies == null || loadedMovies.Count == 0)
        {
            Debug.LogError("No Movies loaded from playlist");
            return;
        }

        RankingUiManager.ShowMovieInfo();

        rankingSystem = new RankingSystem(loadedMovies);
        ShowNextMovie();

        for (int i = 0; i < RankingUiManager.rankButtons.Length; i++)
        {
            int rank = i + 1;
            RankingUiManager.rankButtons[i].onClick.AddListener(() => OnRankButtonClicked(rank));
        }
    }

    private void OnRankButtonClicked(int rank)
    {
        if (rankingSystem.AssignRank(currentMovie, rank))
        {
            RankingUiManager.SetRankLabel(rank, currentMovie.title);
            ShowNextMovie();
        }
        else
        {
            RankingUiManager.ShowError($"Rank {rank} is already used!");
        }
    }

    public void ShowNextMovie()
    {
        currentMovie = rankingSystem.GetNextMovie();
        if(currentMovie == null)
        {
            RankingUiManager.HideMovieInfo();
            RankingUiManager.HideRankButtons();
            RankingUiManager.ShowResults(rankingSystem.GetRankedResults());
        }
        else
        {
            RankingUiManager.DisplayMovie(currentMovie);
        }


    }
}
