using UnityEngine;

public class HomeMenuState : IMenuState
{
    private readonly MenuStateManager menu;

    public HomeMenuState(MenuStateManager menu) => this.menu = menu;

    public void Enter()
    {
        menu.body.ShowHomePanel(true);
        menu.body.homePanel.InitilizePanel();
    }

    public void Exit()
    {
        menu.body.ShowHomePanel(false);
        menu.header.DisableAll();
        menu.footer.DisableAll();
    }

    public void UpdateFooter()
    {
        menu.footer.ShowAppInfoPanel(true);

        if(menu.isConnected)
            menu.footer.ShowPlexInfoPanel(true);
        else
            menu.footer.ShowPlexInfoPanel(false);
    }

    public void UpdateHeader()
    {
        menu.header.ShowMainMenuTitle(true);
    }
}
