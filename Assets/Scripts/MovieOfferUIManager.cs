using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MovieOfferUIManager : MonoBehaviour
{
    public GameObject offerPanel;
    public GameObject movieInfoContainer;

    public TMP_Text titleText;
    public RawImage posterImage;
    public TMP_Text summaryText;
    public TMP_Text genreText;
    public TMP_Text durationText;
    public TMP_Text messageText;

    public Button primaryButton;
    public TMP_Text primaryButtonText;
    public Button secondaryButton;
    public TMP_Text secondaryButtonText;

    public Button watchTrailerButton;

    public void ShowOffer(
        string message,
        MovieInfo movie,
        string primaryLabel,
        string secondaryLabel,
        Action<bool> onChoice)
    {
        offerPanel.SetActive(true);
        messageText.text = message;

        //Handle movie info if provided
        bool hasMovie = movie != null;
        movieInfoContainer.SetActive(hasMovie);

        if (hasMovie)
        {
            titleText.text = $"{movie.title} ({movie.year}";
            posterImage.texture = movie.posterTexture;
            summaryText.text = movie.summary;
            genreText.text = $"GENRE: {movie.genres}";
            durationText.text = $"DURATION: {movie.duration}";

            watchTrailerButton.onClick.RemoveAllListeners();
            watchTrailerButton.onClick.AddListener(() => Application.OpenURL(movie.trailerUrl));
        }

        primaryButtonText.text = primaryLabel;
        secondaryButtonText.text = secondaryLabel;

        primaryButton.onClick.RemoveAllListeners();
        secondaryButton.onClick.RemoveAllListeners();

        primaryButton.onClick.AddListener(() =>
        {
            HideOffer();
            onChoice?.Invoke(false); //Decline Offer / Keep
        });

        secondaryButton.onClick.AddListener(() =>
        {
            HideOffer();
            onChoice?.Invoke(true); //Accept Offer / Switch
        });
    }

    public void HideOffer()
    {
        offerPanel.SetActive(false);
    }
}
