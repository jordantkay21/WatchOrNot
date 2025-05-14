using UnityEngine;

public class SettingsMenuState : IMenuState
{
    private readonly MenuStateManager menu;

    public SettingsMenuState(MenuStateManager menu) => this.menu = menu;

    public void Enter()
    {
        menu.body.ShowSettingsPanel(true);
    }

    public void Exit()
    {
        menu.body.ShowSettingsPanel(false);
        menu.header.DisableAll();
        menu.footer.DisableAll();
    }

    public void UpdateFooter()
    {
        menu.footer.ShowAppInfoPanel(true);
        menu.footer.ShowPlexInfoPanel(true);
    }

    public void UpdateHeader()
    {
        menu.header.ShowSubMenuTitle(true);
        menu.header.subMenuPanel.SetSubMenuTitle("Settings");
        menu.header.ShowButtonPanel(true);
    }
}
