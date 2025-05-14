using UnityEngine;

public class CatchFromCacheMenuState : IMenuState
{
    private readonly MenuStateManager menu;

    public CatchFromCacheMenuState(MenuStateManager menu) => this.menu = menu;

    public void Enter()
    {
        menu.body.ShowCatchFromCachePanel(true);
    }

    public void Exit()
    {
        menu.body.ShowCatchFromCachePanel(false);
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
        menu.header.subMenuPanel.SetSubMenuTitle("Catch From Cache");
        
        menu.header.ShowButtonPanel(true);

        menu.header.ShowMessage(true);
        menu.header.messagePanel.SetMessage("Select a Playlist from your saved playlists");
    }
}
