using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Localization;

namespace PlayerModelChanger.Services.Commands;

public class AdminCommand
{

    private ModelService _ModelService { get; init; }
    private ConfigurationService _ConfigurationService { get; init; }
    private IStringLocalizer _Localizer { get; init; }
    private DatabaseService _DatabaseService { get; init; }

    private PlayerModelChanger _Plugin { get; init; }

    public AdminCommand(
        ModelService modelService,
        ConfigurationService configurationService,
        IStringLocalizer localizer,
        DatabaseService databaseService,
        PlayerModelChanger plugin
    )
    {
        _ModelService = modelService;
        _ConfigurationService = configurationService;
        _Localizer = localizer;
        _DatabaseService = databaseService;
        _Plugin = plugin;

        _Plugin.AddCommand("css_modeladmin", "Model admin command", AdminModelCommand);
    }

    private void ShowAdminCommandHelp(CommandInfo commandInfo)
    {
        commandInfo.ReplyToCommand(_Localizer["command.modeladmin.hint"]);
    }



    public void AdminModelCommand(CCSPlayerController? caller, CommandInfo commandInfo)
    {
        if (!AdminManager.PlayerHasPermissions(caller, "@pmc/admin"))
        {
            commandInfo.ReplyToCommand(_Localizer["command.modeladmin.nopermission"]);
            return;
        }
        if (!AdminManager.PlayerInGroup(caller, "#pmc/admin"))
        {
            commandInfo.ReplyToCommand(_Localizer["command.modeladmin.nopermission"]);
            return;
        }
        if (commandInfo.ArgCount == 2 && commandInfo.GetArg(1) == "reload")
        {
            _ConfigurationService.Reload();
            commandInfo.ReplyToCommand(_Localizer["command.modeladmin.reloadsuccess"]);
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
                    () => _ModelService.SetAllModels("", "", true),
                    () => _ModelService.SetAllTModels("", true),
                    () => _ModelService.SetAllCTModels("", true)
                );
            }
            else
            {
                var steamid = ulong.Parse(target);
                _ModelService.SetPlayerModel(steamid, "", (Side)side, true);
            }
            commandInfo.ReplyToCommand(_Localizer["command.modeladmin.success"]);
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
            if (_ModelService.GetModel(modelIndex) == null)
            {
                commandInfo.ReplyToCommand(_Localizer["command.model.notfound", modelIndex]);
                return;
            }
            var target = commandInfo.GetArg(1);
            if (target == "all")
            {
                var steamids = _ModelService.GetAllPlayers();
                Utils.ExecuteSide(side,
                    () => _ModelService.SetAllModels(modelIndex, modelIndex, true),
                    () => _ModelService.SetAllTModels(modelIndex, true),
                    () => _ModelService.SetAllCTModels(modelIndex, true)
                );
            }
            else
            {
                var steamid = ulong.Parse(target);
                _ModelService.SetPlayerModel(steamid, modelIndex, (Side)side, true);
            }
            commandInfo.ReplyToCommand(_Localizer["command.modeladmin.success"]);
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
                commandInfo.ReplyToCommand(_Localizer["command.modeladmin.playernotfound"]);
                return;
            }
            var result = _ModelService.CheckAndReplaceModel(target);

            var tModelIndex = _DatabaseService.GetStorage().GetPlayerTModel(steamid) ?? "";
            var ctModelIndex = _DatabaseService.GetStorage().GetPlayerCTModel(steamid) ?? "";
            if (result.Item1)
            {
                commandInfo.ReplyToCommand(_Localizer["command.modeladmin.checkedinvalid", tModelIndex]);
            }
            if (result.Item2)
            {
                commandInfo.ReplyToCommand(_Localizer["command.modeladmin.checkedinvalid", ctModelIndex]);
            }
            commandInfo.ReplyToCommand(_Localizer["command.modeladmin.success"]);
        }
        else
        {
            ShowAdminCommandHelp(commandInfo);
        }
    }

}
