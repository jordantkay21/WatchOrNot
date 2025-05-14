using UnityEngine;
using UnityEngine.UI;

public class MainMenuSubPanel : MonoBehaviour
{
    [Header("Main Menu Sub-Panel Components")]
    public Button fetchFromPlexButton;
    public Button catchFromCacheButton;
    public Button settingButton;

    public void Awake()
    {
        fetchFromPlexButton.onClick.AddListener(() => MenuStateManager.Instance.GoToFetchFromPlex());
        catchFromCacheButton.onClick.AddListener(() => MenuStateManager.Instance.GoToCatchFromCache());
        settingButton.onClick.AddListener(() => MenuStateManager.Instance.GoToSettings());
    }
}
