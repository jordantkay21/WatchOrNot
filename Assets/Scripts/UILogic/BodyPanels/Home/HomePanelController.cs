using UnityEngine;
using TMPro;
using UnityEngine.UI;

[DefaultExecutionOrder(5)]
public class HomePanelController : MonoBehaviour
{
    [Header("SubPanels")]
    public WelcomeSubPanel welcomePanel;
    public PlexAuthSubPanel plexAuthPanel;
    public MainMenuSubPanel mainMenuPanel;
    public QuickPlaySubPanel quickPlayPanel;

    bool hasLastSession = false;

    public void InitilizePanel()
    {
        welcomePanel.gameObject.SetActive(true);
        mainMenuPanel.gameObject.SetActive(true);


        if (MenuStateManager.Instance.isConnected)
        {
            string username = "Username";
            welcomePanel.UpdateTitle($"Welcome {username}");
            plexAuthPanel.gameObject.SetActive(false);
        }
        else
        {
            welcomePanel.UpdateTitle($"Welcome");
            plexAuthPanel.gameObject.SetActive(true);
        }

        if (hasLastSession) quickPlayPanel.gameObject.SetActive(true);
        else quickPlayPanel.gameObject.SetActive(false);
    }

}
