using UnityEngine;

public class BodyPanelController : MonoBehaviour
{
    [Header("Body Panels")]
    public HomePanelController homePanel;
    public FetchFromPlexPanelController fetchFromPlexPanel;
    public CatchFromCachePanelController catchFromCachePanel;
    public SettingPanelController settingsPanel;

    public void ShowHomePanel(bool isEnabled) => homePanel.gameObject.SetActive(isEnabled);
    public void ShowFetchFromPlexPanel(bool isEnabled) => fetchFromPlexPanel.gameObject.SetActive(isEnabled);
    public void ShowCatchFromCachePanel(bool isEnabled) => catchFromCachePanel.gameObject.SetActive(isEnabled);
    public void ShowSettingsPanel(bool isEnabled) => settingsPanel.gameObject.SetActive(isEnabled);
}
