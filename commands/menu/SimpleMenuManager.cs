using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

namespace PlayerModelChanger;
class ModelMenu
{

    private BaseMenu menu;

    public ModelMenu(PlayerModelChanger plugin, String menuType, string title)
    {
        if (menuType == "chat")
        {
            menu = new ChatMenu(title);
        }
        else
        {
            menu = new CenterHtmlMenu(title, plugin);
        }
    }

    public BaseMenu GetMenu()
    {
        return menu;
    }
    public void OpenMenu(BasePlugin plugin, CCSPlayerController player)
    {
        if (menu is ChatMenu)
        {
            MenuManager.OpenChatMenu(player, (ChatMenu)menu);
        }
        else if (menu is CenterHtmlMenu)
        {
            MenuManager.OpenCenterHtmlMenu(plugin, player, (CenterHtmlMenu)menu);
        }
    }
}

public class SimpleMenuManager
{

    private String menuType;
    private PlayerModelChanger plugin;
    public SimpleMenuManager(String menuType, PlayerModelChanger plugin)
    {
        this.menuType = menuType;
        this.plugin = plugin;
    }

    public void OpenSelectSideMenu(CCSPlayerController player, TeamMenuData teamMenuData, ModelMenuData modelMenuData)
    {


        ModelMenu sideMenu = new ModelMenu(plugin, menuType, teamMenuData.Title);
        BaseMenu menu = sideMenu.GetMenu();
        foreach (KeyValuePair<string, string> kv in teamMenuData.Selections)
        {
            menu.AddMenuOption(kv.Key, (player, option) => HandleSelectSideMenu(player, option, kv.Value, teamMenuData, modelMenuData));
        }

        menu.PostSelectAction = PostSelectAction.Close;
        sideMenu.OpenMenu(plugin, player);
    }

    public void HandleSelectSideMenu(CCSPlayerController player, ChatMenuOption option, string side, TeamMenuData teamMenuData, ModelMenuData modelMenuData)
    {

        plugin.AddTimer(0.01f, () =>
        {
            OpenSelectModelMenu(player, side, modelMenuData);
        });

    }
    public void OpenSelectModelMenu(CCSPlayerController player, string side, ModelMenuData modelMenuData)
    {
        SingleModelMenuData data = modelMenuData.Data[side];
        ModelMenu modelMenu = new ModelMenu(plugin, menuType, data.Title);
        BaseMenu menu = modelMenu.GetMenu();

        foreach (KeyValuePair<string, string> kv in data.SpecialModelSelections)
        {
            menu.AddMenuOption(kv.Key, (player, option) => HandleSelectModelMenu(player, kv.Value, side));
        }
        foreach (KeyValuePair<string, string> kv in data.ModelSelections)
        {
            menu.AddMenuOption(kv.Key, (player, option) => HandleSelectModelMenu(player, kv.Value, side));
        }
        menu.Open(player);
    }

    private void HandleSelectModelMenu(CCSPlayerController player, string modelIndex, string side)
    {
        plugin.Service.SetPlayerModelWithCheck(player, modelIndex, side);
    }

}
