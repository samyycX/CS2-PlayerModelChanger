using Config;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Plugin.Host;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using Service;
using System.Text;

namespace PlayerModelChanger;
class ModelMenu {

    private BaseMenu menu;

    public ModelMenu(PlayerModelChanger plugin, String menuType, string title) {
        if (menuType == "chat") {
            menu = new ChatMenu(title);
        } else {
            menu = new CenterHtmlMenu(title, plugin);
        }
    }

    public BaseMenu GetMenu() {
        return menu;
    }
    public void OpenMenu(BasePlugin plugin, CCSPlayerController player) {
        if (menu is ChatMenu) {
            MenuManager.OpenChatMenu(player, (ChatMenu) menu);
        } else if (menu is CenterHtmlMenu) {
            MenuManager.OpenCenterHtmlMenu(plugin, player, (CenterHtmlMenu) menu);
        }
    }
}

public class SimpleMenuManager {

    private String menuType;
    private PlayerModelChanger plugin;
    public SimpleMenuManager(String menuType, PlayerModelChanger plugin) {
        this.menuType = menuType;
        this.plugin = plugin;
    }

    public void OpenSelectSideMenu(CCSPlayerController player, TeamMenuData teamMenuData, ModelMenuData modelMenuData) {
        

        ModelMenu sideMenu = new ModelMenu(plugin, menuType, teamMenuData.title);
        BaseMenu menu = sideMenu.GetMenu();
        foreach (KeyValuePair<string,  string> kv in teamMenuData.selection) {
            menu.AddMenuOption(kv.Key, (player, option) => HandleSelectSideMenu(player, option, kv.Value, teamMenuData, modelMenuData));
        }

        menu.PostSelectAction = PostSelectAction.Close;
        sideMenu.OpenMenu(plugin, player);
    }

    public void HandleSelectSideMenu(CCSPlayerController player, ChatMenuOption option, string side, TeamMenuData teamMenuData, ModelMenuData modelMenuData) {
        
        plugin.AddTimer(0.01f, () => {
            OpenSelectModelMenu(player, side, modelMenuData);
        });

    }
    public void OpenSelectModelMenu(CCSPlayerController player, string side, ModelMenuData modelMenuData) {
        SingleModelMenuData data = modelMenuData.data[side];
        ModelMenu modelMenu = new ModelMenu(plugin, menuType, data.title);
        BaseMenu menu = modelMenu.GetMenu();

        foreach (KeyValuePair<string,string> kv in data.specialModelSelection) {
            menu.AddMenuOption(kv.Key, (player, option) => HandleSelectModelMenu(player, kv.Value, side));
        }
        foreach (KeyValuePair<string,string> kv in data.modelSelection) {
            menu.AddMenuOption(kv.Key, (player, option) => HandleSelectModelMenu(player, kv.Value, side));
        }
        menu.Open(player);
    }

    private void HandleSelectModelMenu(CCSPlayerController player, string modelIndex, string side) {
        plugin.Service.SetPlayerModelWithCheck(player, modelIndex, side);
    }

}
