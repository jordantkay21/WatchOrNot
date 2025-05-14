using UnityEngine;

public class FooterController : MonoBehaviour
{
    [Header("Footer Panels")]
    public AppInfoPanel appInfoPanel;
    public PlexInfoPanel plexInfoPanel;
    public SessionInfoPanel sessionInfoPanel;


    public void DisableAll()
    {
        ShowAppInfoPanel(false);
        ShowPlexInfoPanel(false);
        ShowSessionInfoPanel(false);
    }

    public void ShowAppInfoPanel(bool isEnabled) => appInfoPanel.gameObject.SetActive(isEnabled);
    public void ShowPlexInfoPanel(bool isEnabled) => plexInfoPanel.gameObject.SetActive(isEnabled);
    public void ShowSessionInfoPanel(bool isEnabled) => sessionInfoPanel.gameObject.SetActive(isEnabled);
}
