using UnityEngine;

public class FetchFromPlexMenuState : IMenuState
{
    private readonly MenuStateManager menu;

    public FetchFromPlexMenuState(MenuStateManager menu) => this.menu = menu;

    public void Enter()
    {
        menu.body.ShowFetchFromPlexPanel(true);
    }

    public void Exit()
    {
        menu.body.ShowFetchFromPlexPanel(false);
        menu.header.DisableAll();
        menu.footer.DisableAll();
    }

    public void UpdateFooter()
    {
        menu.footer.ShowAppInfoPanel(true);

        if (menu.isConnected)
            menu.footer.ShowPlexInfoPanel(true);
        else
            menu.footer.ShowPlexInfoPanel(false);
    }

    public void UpdateHeader()
    {
        menu.header.ShowSubMenuTitle(true);
        menu.header.subMenuPanel.SetSubMenuTitle("Fetch From Plex");
        menu.header.ShowButtonPanel(true);
        menu.header.ShowMessage(true);
        menu.header.messagePanel.SetMessage("Select a Playlist from your selected Plex Server");

    }
}
