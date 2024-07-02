using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Menu;
using WASDSharedAPI;

namespace PlayerModelChanger;

public class WasdMenuManager {

    private PlayerModelChanger plugin;
    private IWasdMenuManager wasdMenuManager;
    public WasdMenuManager(PlayerModelChanger plugin) {
        this.plugin = plugin;
        wasdMenuManager = new PluginCapability<IWasdMenuManager>("wasdmenu:manager").Get();
        if (wasdMenuManager == null) {
            throw new Exception("Cannot find Wasd Menu API. Make sure you installed it.");
        }
    }

    public static PluginCapability<IWasdMenuManager> WasdMenuManagerCapability = new ("wasdmenu:manager");

    public void OpenSelectSideMenu(CCSPlayerController player, TeamMenuData teamMenuData, ModelMenuData modelMenuData) {
        IWasdMenu menu = wasdMenuManager.CreateMenu(teamMenuData.title);

        foreach(KeyValuePair<string, string> kv in teamMenuData.selection) {
            menu.Add(kv.Key, (player, option) => {
                wasdMenuManager.CloseMenu(player);
                plugin.AddTimer(0.1f, () => {
                    OpenSelectModelMenu(player, kv.Value, modelMenuData);
                });
            });
        }

        wasdMenuManager.OpenMainMenu(player, menu);
    }

    public void OpenSelectModelMenu(CCSPlayerController player, string side, ModelMenuData modelMenuData) {
        SingleModelMenuData data = modelMenuData.data[side];
        IWasdMenu menu = wasdMenuManager.CreateMenu(data.title);

        foreach(KeyValuePair<string, string> kv in data.specialModelSelection) {
            menu.Add(kv.Key, (player, option) => {
                plugin.Service.SetPlayerModelWithCheck(player, kv.Value, side);
            });
        }
        foreach(KeyValuePair<string, string> kv in data.modelSelection) {
            menu.Add(kv.Key, (player, option) => {
                plugin.Service.SetPlayerModelWithCheck(player, kv.Value, side);
            });
        }

        wasdMenuManager.OpenMainMenu(player, menu);

    }
}