using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalRevealPanel : MonoBehaviour
{
    public static FinalRevealPanel Instance;

    public GameObject panel;
    public TMP_Text finalTitleText;
    public TMP_Text movieTitleText;
    public RawImage posterImage;
    public TMP_Text rankText;
    public Button replayButton;
    public Button quitButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowFinalMovie(MovieInfo finalMovie, int rankedIndex)
    {
        panel.SetActive(true);

        finalTitleText.text = "Your Movie Is:";
        movieTitleText.text = finalMovie.title;
        posterImage.texture = finalMovie.posterTexture;

        rankText.text = rankedIndex == 0
            ? "You picked your #1 favorite!"
            : $"You ranked this #{rankedIndex + 1}";

        replayButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();

        replayButton.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene(0));
        quitButton.onClick.AddListener(() => Application.Quit());
    }
}
