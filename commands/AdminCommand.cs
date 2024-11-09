using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace PlayerModelChanger;

public partial class PlayerModelChanger
{
    private void ShowAdminCommandHelp(CommandInfo commandInfo)
    {
        commandInfo.ReplyToCommand(Localizer["command.modeladmin.hint"]);
    }

    [ConsoleCommand("css_modeladmin", "Model admin command")]
    [RequiresPermissionsOr("@pmc/admin", "#pmc/admin")]
    public void AdminModelCommand(CCSPlayerController? caller, CommandInfo commandInfo)
    {
        if (commandInfo.ArgCount == 2 && commandInfo.GetArg(1) == "reload")
        {
            ReloadConfig();
            commandInfo.ReplyToCommand(Localizer["command.modeladmin.reloadsuccess"]);
            return;
        }
        if (commandInfo.ArgCount <= 2)
        {
            ShowAdminCommandHelp(commandInfo);
            return;
        }


        var type = commandInfo.GetArg(2);
        if (type == "reset")
        {
            if (commandInfo.ArgCount <= 3)
            {
                ShowAdminCommandHelp(commandInfo);
                return;
            }
            Side? side = commandInfo.GetArg(3).ToSide();
            if (side == null)
            {
                ShowAdminCommandHelp(commandInfo);
                return;
            }

            var target = commandInfo.GetArg(1);
            if (target == "all")
            {
                Utils.ExecuteSide(side,
                    () => Service.SetAllModels("", "", true),
                    () => Service.SetAllTModels("", true),
                    () => Service.SetAllCTModels("", true)
                );
            }
            else
            {
                var steamid = ulong.Parse(target);
                Service.SetPlayerModel(steamid, "", (Side)side, true);
            }
            commandInfo.ReplyToCommand(Localizer["command.modeladmin.success"]);
        }
        else if (type == "set")
        {
            if (commandInfo.ArgCount <= 4)
            {
                ShowAdminCommandHelp(commandInfo);
                return;
            }
            var side = commandInfo.GetArg(3).ToSide();
            if (side == null)
            {
                ShowAdminCommandHelp(commandInfo);
                return;
            }
            var modelIndex = commandInfo.GetArg(4);
            if (Service.GetModel(modelIndex) == null)
            {
                commandInfo.ReplyToCommand(Localizer["command.model.notfound", modelIndex]);
                return;
            }
            var target = commandInfo.GetArg(1);
            if (target == "all")
            {
                var steamids = Service.GetAllPlayers();
                Utils.ExecuteSide(side,
                    () => Service.SetAllModels(modelIndex, modelIndex, true),
                    () => Service.SetAllTModels(modelIndex, true),
                    () => Service.SetAllCTModels(modelIndex, true)
                );
            }
            else
            {
                var steamid = ulong.Parse(target);
                Service.SetPlayerModel(steamid, modelIndex, (Side)side, true);
            }
            commandInfo.ReplyToCommand(Localizer["command.modeladmin.success"]);
        }
        else if (type == "check")
        {
            if (commandInfo.ArgCount <= 2)
            {
                ShowAdminCommandHelp(commandInfo);
                return;
            }

            var steamid = ulong.Parse(commandInfo.GetArg(1));
            var target = Utilities.GetPlayerFromSteamId(steamid);
            if (target == null)
            {
                commandInfo.ReplyToCommand(Localizer["command.modeladmin.playernotfound"]);
                return;
            }
            var result = Service.CheckAndReplaceModel(target);

            var tModelIndex = Service._Storage.GetPlayerTModel(steamid) ?? "";
            var ctModelIndex = Service._Storage.GetPlayerCTModel(steamid) ?? "";
            if (result.Item1)
            {
                commandInfo.ReplyToCommand(Localizer["command.modeladmin.checkedinvalid", tModelIndex]);
            }
            if (result.Item2)
            {
                commandInfo.ReplyToCommand(Localizer["command.modeladmin.checkedinvalid", ctModelIndex]);
            }
            commandInfo.ReplyToCommand(Localizer["command.modeladmin.success"]);
        }
        else
        {
            ShowAdminCommandHelp(commandInfo);
        }
    }

}
