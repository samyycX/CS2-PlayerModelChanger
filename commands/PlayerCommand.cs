using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Service;

namespace PlayerModelChanger;

public partial class PlayerModelChanger {

    [ConsoleCommand("css_model", "Show your model.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void ChangeModelCommand(CCSPlayerController? player, CommandInfo commandInfo) {

        if (commandInfo.ArgCount == 1) {
            var TModel = Service.GetPlayerModelName(player, CsTeam.Terrorist);
            var CTModel = Service.GetPlayerModelName(player, CsTeam.CounterTerrorist);
            commandInfo.ReplyToCommand(Localizer["player.currentmodel", Localizer["side.t"], TModel]);
            commandInfo.ReplyToCommand(Localizer["player.currentmodel", Localizer["side.ct"], CTModel]);
            commandInfo.ReplyToCommand(Localizer["command.model.hint1"]);
            commandInfo.ReplyToCommand(Localizer["command.model.hint2"]);
            return;
        }

        var modelIndex = commandInfo.GetArg(1);

        if (modelIndex != "@random" && !Service.ExistModel(modelIndex)) {
            var model = Service.FindModel(modelIndex);
            if (model == null) {
                commandInfo.ReplyToCommand(Localizer["command.model.notfound", modelIndex]);
                return;
            } else {
                modelIndex = model.index;
            }
        }

        var side = "all";
        if (commandInfo.ArgCount == 3) {
            side = commandInfo.GetArg(2).ToLower();
            if (side.ToLower() != "t" || side.ToLower() != "ct") {
                commandInfo.ReplyToCommand(Localizer["command.unknownside", side]);
                return;
            }
        }
        var defaultModel = DefaultModelManager.GetPlayerDefaultModel(player!, side);
        if (defaultModel != null && defaultModel.force) {
            commandInfo.ReplyToCommand(Localizer["model.nochangepermission"]);
            return;
        }
        
        Service.SetPlayerModelWithCheck(player, modelIndex, side);
    }

    [ConsoleCommand("css_md", "Select models.")]
    [ConsoleCommand("css_models", "Select models.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void GetAllModelsCommand(CCSPlayerController? player, CommandInfo commandInfo) {
        
        if (commandInfo.ArgCount == 1) {
          OpenSelectSideMenu(player);
          return;
        }

        string side = commandInfo.GetArg(1);
        OpenSelectModelMenu(player, side, Service.GetPlayerModel(player, side));


          

       
    }

}
