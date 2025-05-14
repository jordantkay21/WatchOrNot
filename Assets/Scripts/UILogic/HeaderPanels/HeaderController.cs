using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeaderController : MonoBehaviour
{
    [Header("Header Panels")]
    public MainMenuPanel mainMenuPanel;
    public SubMenuPanel subMenuPanel;
    public MessagePanel messagePanel;
    public NavigationPanel navPanel;

    public void DisableAll()
    {
        ShowMainMenuTitle(false);
        ShowSubMenuTitle(false);
        ShowMessage(false);
        ShowButtonPanel(false);
    }

    public void ShowMainMenuTitle(bool isEnabled) => mainMenuPanel.gameObject.SetActive(isEnabled);
    public void ShowSubMenuTitle(bool isEnabled) => subMenuPanel.gameObject.SetActive(isEnabled);
    public void ShowMessage(bool isEnabled) => messagePanel.gameObject.SetActive(isEnabled);
    public void ShowButtonPanel(bool isEnabled) => navPanel.gameObject.SetActive(isEnabled);
}
