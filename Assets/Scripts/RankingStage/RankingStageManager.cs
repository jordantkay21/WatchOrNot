using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingStageManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject rankingStagePanel;

    [Header("Ranking Panel")]
    public List<Button> rankButtons;
    public List<TMP_Text> rankButtonTexts;

    [Header("Movie Info Panel")]
    public TMP_Text titleText;
    public TMP_Text yearText;
    public RawImage posterImage;
    public TMP_Text summaryText;
    public TMP_Text genreText;
    public TMP_Text durationText;
    public Button watchTrailerButton;

    [Header("Other")]
    public TMP_Text messageText;

    private List<MovieInfo> moviesToRank;
    private Dictionary<int, MovieInfo> assignedRanks = new();
    private int currentMovieIndex = 0;

    private void Start()
    {
        rankingStagePanel.SetActive(false);
        watchTrailerButton.onClick.AddListener(OnWatchTrailerClicked);
    }

    private void OnEnable()
    {
        GameManager.OnRankingPhaseStarted += BeginRanking;
    }

    private void OnDisable()
    {
        GameManager.OnRankingPhaseStarted -= BeginRanking;
    }

    public void BeginRanking()
    {
        moviesToRank = new List<MovieInfo>(GameManager.Instance.SelectedMovies);
        assignedRanks.Clear();
        currentMovieIndex = 0;

        SetupRankButtons();
        ShowCurrentMovie();
        rankingStagePanel.SetActive(true);
        GameManager.Instance.SetPhase(GamePhase.Ranking);
    }

    private void SetupRankButtons()
    {
        for (int i = 0; i < rankButtons.Count; i++)
        {
            int rankNumber = i + 1;
            rankButtonTexts[i].text = "Unassigned";
            rankButtons[i].interactable = true;
            int capturedRank = rankNumber;
            rankButtons[i].onClick.RemoveAllListeners();
            rankButtons[i].onClick.AddListener(() => OnRankButtonClicked(capturedRank));
        }
    }

    private void ShowCurrentMovie()
    {
        if(currentMovieIndex >= moviesToRank.Count)
        {
            OnRankingComplete();
            return;
        }

        var movie = moviesToRank[currentMovieIndex];
        titleText.text = movie.title;
        yearText.text = movie.year.ToString();
        posterImage.texture = movie.posterTexture;
        summaryText.text = movie.summary;
        genreText.text = movie.genres;
        durationText.text = movie.duration;

        watchTrailerButton.interactable = !string.IsNullOrEmpty(movie.trailerUrl);

        messageText.text = $"Ranking {currentMovieIndex + 1} of {moviesToRank.Count}";
    }

    private void OnRankButtonClicked(int rank)
    {
        if (assignedRanks.ContainsKey(rank))
        {
            Debug.LogWarning($"Rank {rank} already assigned!");
            return;
        }

        var movie = moviesToRank[currentMovieIndex];
        assignedRanks[rank] = movie;

        //Update button text and disable
        rankButtonTexts[rank - 1].text = movie.title;
        rankButtons[rank - 1].interactable = false;

        currentMovieIndex++;
        ShowCurrentMovie();
    }

    private void OnWatchTrailerClicked()
    {
        var movie = moviesToRank[currentMovieIndex];
        if (!string.IsNullOrEmpty(movie.trailerUrl)) 
            Application.OpenURL(movie.trailerUrl);
    }

    private void OnRankingComplete()
    {
        messageText.text = "Ranking Complete!";
        Debug.Log($"[GameManager][OnRankingComplete] All Movies Ranked!");

        //May want to store the final result somewhere

        GameManager.Instance.SetPhase(GamePhase.RevealGameplay);

        // TODO: Move to reveal phase
    }
}
