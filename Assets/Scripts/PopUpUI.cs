using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpUI : MonoBehaviour
{
    public static PopUpUI Instance { get; private set; }

    [Header("General Popup")]
    public GameObject popupPanel;
    public TMP_Text messageText;
    public Button primaryButton;
    public TMP_Text primaryButtonText;
    public Button secondaryButton;
    public TMP_Text secondaryButtonText;

    [Header("Movie Info")]
    public GameObject movieInfoContainer;
    public TMP_Text titleText;
    public RawImage posterImage;
    public TMP_Text summaryText;
    public TMP_Text genreText;
    public TMP_Text durationText;
    public Button watchTrailerButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }

    public void ShowPopup(
        string message,
        string primaryLabel,
        Action onPrimary,
        string secondaryLabel = null,
        Action onSecondary = null,
        MovieInfo movie = null)
    {
        popupPanel.SetActive(true);
        messageText.text = message;

        SetupButtons(primaryLabel, onPrimary, secondaryLabel, onSecondary);
        SetupMovieInfo(movie);
    }

    private void SetupButtons(string primaryLabel, Action onPrimary, string secondaryLabel, Action onSecondary)
    {
        primaryButtonText.text = primaryLabel;
        primaryButton.onClick.RemoveAllListeners();
        primaryButton.onClick.AddListener(() =>
        {
            HidePopup();
            onPrimary?.Invoke();
        });

        primaryButton.gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(secondaryLabel))
        {
            secondaryButtonText.text = secondaryLabel;
            secondaryButton.onClick.RemoveAllListeners();
            secondaryButton.onClick.AddListener(() =>
            {
                HidePopup();
                onSecondary?.Invoke();
            });

            secondaryButton.gameObject.SetActive(true);
        }
        else
        {
            secondaryButton.gameObject.SetActive(false);
        }
    }

    private void SetupMovieInfo(MovieInfo movie)
    {
        bool showMovie = movie != null;
        movieInfoContainer?.SetActive(showMovie);

        if (!showMovie) return;

        titleText.text = $"{movie.title} ({movie.year})";
        posterImage.texture = movie.posterTexture;
        summaryText.text = movie.summary;
        genreText.text = $"GENRE: {movie.genres}";
        durationText.text = $"DURATION: {movie.duration}";

        watchTrailerButton.onClick.RemoveAllListeners();
        watchTrailerButton.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(movie.trailerUrl))
                Application.OpenURL(movie.trailerUrl);
        });
    }

}
