using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Service;
using System.Text;

namespace PlayerModelChanger;

public partial class PlayerModelChanger {
    public void OpenSelectSideMenu(CCSPlayerController player) {
        ModelMenu sideMenu = new ModelMenu(Config, Localizer["modelmenu.selectside"]);
        BaseMenu menu = sideMenu.GetMenu();

        var playerTModel = Service.GetPlayerModel(player, "t");
        var playerCTModel = Service.GetPlayerModel(player, "ct");
        var playerAllModel = Service.GetPlayerModel(player, "all");

        string tSelection = playerTModel != null ? $"{Localizer["side.t"]}: {playerTModel.name}" :  $"{Localizer["side.t"]}";
        string ctSelection = playerCTModel != null ? $"{Localizer["side.ct"]}: {playerCTModel.name}" :  $"{Localizer["side.ct"]}";
        string allSelection = playerAllModel != null ? $"{Localizer["side.all"]}: {playerAllModel.name}" :  $"{Localizer["side.all"]}";
        menu.AddMenuOption(tSelection, (player, option) => HandleSelectSideMenu(player, option, "t", playerTModel));
        menu.AddMenuOption(ctSelection, (player, option) => HandleSelectSideMenu(player, option, "ct", playerCTModel));
        menu.AddMenuOption(allSelection, (player, option) => HandleSelectSideMenu(player, option, "all", playerAllModel));

        menu.PostSelectAction = PostSelectAction.Close;
        sideMenu.OpenMenu(this, player);
    }

    public void HandleSelectSideMenu(CCSPlayerController player, ChatMenuOption option, string side, Model? currentModel) {
        
        AddTimer(0.01f, () => {
            OpenSelectModelMenu(player, side, currentModel);
        });

    }
    public void OpenSelectModelMenu(CCSPlayerController player, string side, Model? currentModel) {
        ModelMenu modelMenu = new ModelMenu(Config, Localizer["modelmenu.title", Localizer[$"side.{side}"], currentModel == null ? Localizer["model.none"] : currentModel.name]);
        BaseMenu menu = modelMenu.GetMenu();
        
        List<Model> models = Service.GetAllAppliableModels(player, side);

        menu.AddMenuOption(Localizer["modelmenu.unset"], (player, option) => HandleSelectModelMenu(player, "", side));
        if (!Config.DisableRandomModel) {
            menu.AddMenuOption(Localizer["modelmenu.random"], (player, option) => HandleSelectModelMenu(player, "@random", side));
        }        
        menu.AddMenuOption(Localizer["modelmenu.default"], (player, option) => HandleSelectModelMenu(player, "@default", side));
        foreach (var model in models)
        {   
            if (model.hideinmenu) {
                continue;
            }
            menu.AddMenuOption($"{model.name}", (player, option) => HandleSelectModelMenu(player, model.index, side));
        }

        if (menu.MenuOptions.Count == 0) {
            player.PrintToChat(Localizer["modelmenu.nomodel"]);
            return;
        }
        modelMenu.OpenMenu(this, player);
    }

    private void HandleSelectModelMenu(CCSPlayerController player, string modelIndex, string side) {
        Service.SetPlayerModelWithCheck(player, modelIndex, side);
    }

}
