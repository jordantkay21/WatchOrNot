using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovieInfoUIController : MonoBehaviour
{
    public static MovieInfoUIController Instance;

    [Header("Panel")]
    public GameObject movieInfoPanel;

    [Header("Movie Info Components")]
    public TMP_Text titleText;
    public TMP_Text yearText;
    public RawImage posterImage;
    public TMP_Text summaryText;
    public TMP_Text genreText;
    public TMP_Text durationText;
    public Button watchTrailerButton;

    private MovieInfo currentMovie;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        watchTrailerButton.onClick.AddListener(OnWatchTrailerClicked);
    }
    public void Show(MovieInfo movie)
    {
        movieInfoPanel.SetActive(true);

        currentMovie = movie;

        titleText.text = movie.title;
        yearText.text = movie.year.ToString();
        posterImage.texture = movie.posterTexture;
        summaryText.text = movie.summary;
        genreText.text = movie.genres;
        durationText.text = movie.duration;

        watchTrailerButton.interactable = !string.IsNullOrEmpty(movie.trailerUrl);
    }

    private void OnWatchTrailerClicked()
    {
        if (currentMovie != null && !string.IsNullOrEmpty(currentMovie.trailerUrl))
            Application.OpenURL(currentMovie.trailerUrl);
    }

    public void Hide()
    {
        movieInfoPanel.SetActive(false);
    }
}
