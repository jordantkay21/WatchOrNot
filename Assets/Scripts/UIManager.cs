using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject movieInfoPanel;
    public RawImage posterImage;
    public TMP_Text titleText;
    public TMP_Text summaryText;
    public Button watchTrailerButton;

    public Button[] rankButtons;

    public GameObject resultsPanel;
    public TMP_Text resultsText;

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
    }

    public void ShowMovieInfo()
    {
        movieInfoPanel.SetActive(true);
    }

    public void HideMovieInfo()
    {
        movieInfoPanel.SetActive(false);
    }

    public void DisableRank(int rank)
    {
        if (rank - 1 < rankButtons.Length)
            rankButtons[rank - 1].interactable = false;
    }

    public void ShowError(string msg)
    {
        Debug.LogWarning(msg);
    }

    public void ShowResults(List<(int rank, MovieInfo movie)> ranked)
    {
        resultsPanel.SetActive(true);
        resultsText.text = string.Join("\n", ranked.Select(r => $"{r.rank}: {r.movie.title}"));
    }

}
