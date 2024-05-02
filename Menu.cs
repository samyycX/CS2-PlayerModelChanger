using Config;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;

namespace PlayerModelChanger;

public class ModelMenu {

    private BaseMenu menu;

    public ModelMenu(ModelConfig config, string title) {
        if (config.MenuType == "chat") {
            menu = new ChatMenu(title);
        } else {
            menu = new CenterHtmlMenu(title);
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

