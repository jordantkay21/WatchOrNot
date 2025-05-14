using UnityEngine;
using UnityEngine.UI;

public class NavigationPanel : MonoBehaviour
{
    [Header("Button Components")]
    public Button homeButton;
    public Button fetchFromPlexButton;
    public Button catchFromCacheButton;
    public Button settingsButton;
    public Button playButton;

    public void Awake()
    {
        homeButton.onClick.AddListener(() => MenuStateManager.Instance.GoToHome());
        fetchFromPlexButton.onClick.AddListener(() => MenuStateManager.Instance.GoToFetchFromPlex());
        catchFromCacheButton.onClick.AddListener(() => MenuStateManager.Instance.GoToCatchFromCache());
        settingsButton.onClick.AddListener(() => MenuStateManager.Instance.GoToSettings());

    }

    public void ShowPlayButton()
    {
        playButton.gameObject.SetActive(true);
    }

    public void HidePlayButton()
    {
        playButton.gameObject.SetActive(false);
    }
}
