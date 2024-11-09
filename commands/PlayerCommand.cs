using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace PlayerModelChanger;

public partial class PlayerModelChanger
{

    [ConsoleCommand("css_model", "Show your model.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void ChangeModelCommand(CCSPlayerController player, CommandInfo commandInfo)
    {

        if (commandInfo.ArgCount == 1)
        {
            var TModel = Service.GetPlayerModelName(player, CsTeam.Terrorist);
            var CTModel = Service.GetPlayerModelName(player, CsTeam.CounterTerrorist);
            commandInfo.ReplyToCommand(Localizer["player.currentmodel", Localizer["side.t"], TModel]);
            commandInfo.ReplyToCommand(Localizer["player.currentmodel", Localizer["side.ct"], CTModel]);
            commandInfo.ReplyToCommand(Localizer["command.model.hint1"]);
            commandInfo.ReplyToCommand(Localizer["command.model.hint2"]);
            return;
        }

        if (Config.DisablePlayerSelection)
        {
            return;
        }

        var modelIndex = commandInfo.GetArg(1);

        if (modelIndex != "@random" && !Service.ExistModel(modelIndex))
        {
            var model = Service.FindModel(modelIndex);
            if (model == null)
            {
                commandInfo.ReplyToCommand(Localizer["command.model.notfound", modelIndex]);
                return;
            }
            else
            {
                modelIndex = model.Index;
            }
        }

        Side? side = Side.All;
        if (commandInfo.ArgCount == 3)
        {
            side = commandInfo.GetArg(2).ToSide();
            if (side == null)
            {
                commandInfo.ReplyToCommand(Localizer["command.unknownside", "null"]);
                return;
            }
        }
        var defaultModel = DefaultModelManager.GetPlayerDefaultModel(player!, (Side)side!);
        if (defaultModel != null && defaultModel.force)
        {
            commandInfo.ReplyToCommand(Localizer["model.nochangepermission"]);
            return;
        }

        Service.SetPlayerModelWithCheck(player, modelIndex, (Side)side!);
    }

    [ConsoleCommand("css_md", "Select models.")]
    [ConsoleCommand("css_models", "Select models.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void GetAllModelsCommand(CCSPlayerController player, CommandInfo commandInfo)
    {

        if (Config.DisablePlayerSelection)
        {
            return;
        }
        if (commandInfo.ArgCount == 1)
        {
            OpenSelectSideMenu(player);
            return;
        }
        var side = commandInfo.GetArg(1).ToSide();
        if (side == null)
        {
            commandInfo.ReplyToCommand(Localizer["command.unknownside", "null"]);
            return;
        }

        OpenSelectModelMenu(player, (Side)side!, Service.GetPlayerModel(player, (Side)side!));

    }

    [ConsoleCommand("css_mesh", "Select models.")]
    [ConsoleCommand("css_mg", "Select models.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void MeshgroupCommand(CCSPlayerController player, CommandInfo commandInfo)
    {
        OpenSelectMeshgroupMenu(player);
    }
}
