using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using WASDSharedAPI;

namespace PlayerModelChanger;

public class WasdMenuManager
{

    private PlayerModelChanger _Plugin;
    private IWasdMenuManager _WasdMenuManager;
    public WasdMenuManager(PlayerModelChanger plugin)
    {
        this._Plugin = plugin;
        var wasdMenuManager = new PluginCapability<IWasdMenuManager>("wasdmenu:manager").Get();
        if (wasdMenuManager == null)
        {
            throw new Exception("Cannot find Wasd Menu API. Make sure you installed it.");
        }
        _WasdMenuManager = wasdMenuManager;
    }

    public static PluginCapability<IWasdMenuManager> WasdMenuManagerCapability = new("wasdmenu:manager");

    public void OpenSelectSideMenu(CCSPlayerController player, TeamMenuData teamMenuData, ModelMenuData modelMenuData)
    {
        IWasdMenu menu = _WasdMenuManager.CreateMenu(teamMenuData.Title);

        foreach (KeyValuePair<string, string> kv in teamMenuData.Selections)
        {
            var subMenu = GetSelectModelMenu(player, kv.Value, modelMenuData);
            subMenu.Prev = menu.Add(kv.Key, (player, option) =>
            {
                _WasdMenuManager.OpenSubMenu(player, subMenu);
            });
        }

        _WasdMenuManager.OpenMainMenu(player, menu);
    }

    public void OpenSelectModelMenu(CCSPlayerController player, string side, ModelMenuData modelMenuData)
    {


        _WasdMenuManager.OpenMainMenu(player, GetSelectModelMenu(player, side, modelMenuData));

    }

    public IWasdMenu GetSelectModelMenu(CCSPlayerController player, string side, ModelMenuData modelMenuData)
    {
        SingleModelMenuData data = modelMenuData.Data[side];

        IWasdMenu menu = _WasdMenuManager.CreateMenu(data.Title);
        foreach (KeyValuePair<string, string> kv in data.SpecialModelSelections)
        {
            menu.Add(kv.Key, (player, option) =>
            {
                _Plugin.Service.SetPlayerModelWithCheck(player, kv.Value, side);
            });
        }
        foreach (KeyValuePair<string, string> kv in data.ModelSelections)
        {
            menu.Add(kv.Key, (player, option) =>
            {
                _Plugin.Service.SetPlayerModelWithCheck(player, kv.Value, side);
            });
        }
        return menu;
    }
}
