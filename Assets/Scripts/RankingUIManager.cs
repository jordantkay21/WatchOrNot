using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RankingUIManager : MonoBehaviour
{
    public GameObject movieInfoPanel;
    public RawImage posterImage;
    public TMP_Text titleText;
    public TMP_Text summaryText;
    public TMP_Text genreText;
    public TMP_Text durationText;
    public Button watchTrailerButton;

    public GameObject rankButtonPanel;
    public Button[] rankButtons;

    public GameObject resultsPanel;
    public Transform resultsListContainer;
    public GameObject resultEntryPrefab;

    private Dictionary<int, GameObject> resultEntries = new();

    public void DisplayMovie(MovieInfo movie)
    {
        titleText.text = $"{movie.title} ({movie.year})";
        posterImage.texture = movie.posterTexture;
        summaryText.text = $"{movie.summary}";


        if (!string.IsNullOrEmpty(movie.trailerUrl))
        {
            watchTrailerButton.gameObject.SetActive(true);
            watchTrailerButton.onClick.RemoveAllListeners();
            watchTrailerButton.onClick.AddListener(() =>
            {
                Application.OpenURL(movie.trailerUrl);
            });
        }
        else
        {
            watchTrailerButton.gameObject.SetActive(false);
        }

        genreText.text = $"GENRE: {movie.genres}";
        durationText.text = $"DURATION: {movie.duration}";
    }

    public void ShowMovieInfo()
    {
        movieInfoPanel.SetActive(true);
    }
    public void ShowResults(List<(int rank, MovieInfo movie)> ranked)
    {
        resultsPanel.SetActive(true);

        foreach (Transform child in resultsListContainer)
            Destroy(child.gameObject);

        resultEntries.Clear();

        foreach (var entry in ranked)
        {
            GameObject resultGO = Instantiate(resultEntryPrefab, resultsListContainer);
            var texts = resultGO.GetComponentsInChildren<TMP_Text>();

            var rankText = texts.First(t => t.name == "RankText");
            var titleText = texts.First(t => t.name == "TitleText");

            rankText.text = entry.rank.ToString();
            titleText.text = entry.movie.title;

            resultEntries[entry.rank] = resultGO;
        }
    }

    public void ShowRankButtons()
    {
        rankButtonPanel.SetActive(true);
    }

    public void HideRankButtons()
    {
        rankButtonPanel.SetActive(false);
    }

    public void HideMovieInfo()
    {
        movieInfoPanel.SetActive(false);
    }

    public void SetRankLabel(int rank, string movieTitle)
    {
        if (rank - 1 >= rankButtons.Length) return;

        var btn = rankButtons[rank - 1];
        btn.interactable = false;

        var label = btn.GetComponentInChildren<TMP_Text>();
        label.text = $"{rank}\n{movieTitle}";
    }

    public void ShowError(string msg)
    {
        Debug.LogWarning(msg);
    }

    public void CrossOutByTitle(string title)
    {
        foreach (var entry in resultEntries.Values)
        {
            var texts = entry.GetComponentsInChildren<TMP_Text>();
            var titleText = texts.FirstOrDefault(t => t.name == "TitleText");

            if (titleText != null && titleText.text == title)
            {
                titleText.fontStyle |= FontStyles.Bold;
                titleText.fontStyle |= FontStyles.Strikethrough;
                break;
            }
        }
    }
}
