using System.Runtime.CompilerServices;
using CounterStrikeSharp.API.Core;
using Service;

namespace PlayerModelChanger;



public partial class PlayerModelChanger {

    private SimpleMenuManager? simpleMenuManager;
    private WasdMenuManager? wasdMenuManager;

    private ModelMenuData GenerateModelMenuData(CCSPlayerController player) {
        ModelMenuData data = new();
        foreach(var side in new string[]{"t", "ct", "all"}) {
            SingleModelMenuData singleData = new SingleModelMenuData();
            var currentModel = Service.GetPlayerModel(player, side);
            List<Model> models = Service.GetAllAppliableModels(player, side);
            singleData.title = Localizer["modelmenu.title", Localizer[$"side.{side}"], currentModel == null ? Localizer["model.none"] : currentModel.name];
            singleData.specialModelSelection.Add(Localizer["modelmenu.unset"], "");
            if (!Config.DisableRandomModel) {
                singleData.specialModelSelection.Add(Localizer["modelmenu.random"], "@random");
            }
            singleData.specialModelSelection.Add(Localizer["modelmenu.default"], "@default");

            foreach (var model in models)
            {   
                if (model.hideinmenu) {
                    continue;
                }
                singleData.modelSelection.Add(model.name, model.index);
            }

            data.data.Add(side, singleData);
        }
        return data;
    }

    private TeamMenuData GenerateTeamMenuData(CCSPlayerController player) {
        TeamMenuData data = new();
        data.title = Localizer["modelmenu.selectside"];
        foreach(var side in new string[]{"t", "ct", "all"}) {
            var playerModel = Service.GetPlayerModel(player, side);

            string selection = playerModel != null ? $"{Localizer["side."+side]}: {playerModel.name}" :  $"{Localizer["side."+side]}";
            data.selection.Add(selection, side);
        };
        return data;
    }

    private SimpleMenuManager GetSimpleMenuManager() {
        if (simpleMenuManager == null) {
            simpleMenuManager = new SimpleMenuManager(Config.MenuType, this);
        }
        return simpleMenuManager;
    }

    private WasdMenuManager GetWasdMenuManager() {
        if (wasdMenuManager == null) {
            wasdMenuManager = new WasdMenuManager(this);
        }
        return wasdMenuManager;
    }
    public void OpenSelectSideMenu(CCSPlayerController player) {
        var modelData = GenerateModelMenuData(player);
        var teamData = GenerateTeamMenuData(player);
        if (Config.MenuType == "interactive") {
            GetWasdMenuManager().OpenSelectSideMenu(player, teamData, modelData);
        } else {
            GetSimpleMenuManager().OpenSelectSideMenu(player, teamData, modelData);
        }
    }

    public void OpenSelectModelMenu(CCSPlayerController player, string side, Model? model) {
        var modelData = GenerateModelMenuData(player);
        if (Config.MenuType == "dynamic") {
            GetWasdMenuManager().OpenSelectModelMenu(player, side, modelData);
        } else {
            GetSimpleMenuManager().OpenSelectModelMenu(player, side, modelData);
        }
    }
}