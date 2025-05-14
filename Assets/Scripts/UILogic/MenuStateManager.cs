using UnityEngine;

public class MenuStateManager : MonoBehaviour
{
    public static MenuStateManager Instance;

    [Header("Panel Controllers")]
    public HeaderController header;
    public FooterController footer;
    public BodyPanelController body;

    [Header("Traversal Logic")]
    public bool isConnected;

    private IMenuState currentState;

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
        GoToHome();
    }

    public void ChangeState(IMenuState newState)
    {
        if(currentState != null) 
            currentState?.Exit();

        currentState = newState;
        currentState.Enter();
        currentState.UpdateHeader();
        currentState.UpdateFooter();
    }

    public void GoToHome() => ChangeState(new HomeMenuState(this));
    public void GoToFetchFromPlex() => ChangeState(new FetchFromPlexMenuState(this));
    public void GoToCatchFromCache() => ChangeState(new CatchFromCacheMenuState(this));
    public void GoToSettings() => ChangeState(new SettingsMenuState(this));
}
