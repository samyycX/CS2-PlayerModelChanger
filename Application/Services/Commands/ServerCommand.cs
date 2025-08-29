using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Localization;
using PlayerModelChanger.Models;
using PlayerModelChanger.Services;

namespace PlayerModelChanger;

public class ServerCommand
{

    private PlayerModelChanger _Plugin { get; init; }

    private GlobalVars _GlobalVars { get; init; }

    private IStringLocalizer _Localizer { get; init; }

    private ModelCacheService _ModelCacheService { get; init; }

    public ServerCommand(PlayerModelChanger plugin, GlobalVars globalVars, IStringLocalizer localizer, ModelCacheService modelCacheService)
    {
        _Plugin = plugin;
        _GlobalVars = globalVars;
        _Localizer = localizer;
        _ModelCacheService = modelCacheService;
    }


    [ConsoleCommand("playermodelchanger_enable", "Enable/Disable the plugin.")]
    [ConsoleCommand("pmc_enable", "Enable/Disable the plugin.")]
    [CommandHelper(minArgs: 1, usage: "[true/false]", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void Switch(CCSPlayerController? player, CommandInfo commandInfo)
    {
        var arg = commandInfo.GetArg(1);
        if (arg == "1" || arg == "true")
        {
            _GlobalVars.Enable = true;
            commandInfo.ReplyToCommand(_Localizer["plugin.enable"]);
        }
        else if (arg == "0" || arg == "false")
        {
            _GlobalVars.Enable = false;
            commandInfo.ReplyToCommand(_Localizer["plugin.disable"]);
        }
        else
        {
            commandInfo.ReplyToCommand(_Localizer["command.incorrectusage"]);
        }

    }

    [ConsoleCommand("playermodelchanger_resynccache", "Resync cache.")]
    [ConsoleCommand("pmc_resynccache", "Resync cache.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void ResyncCache(CCSPlayerController? player, CommandInfo commandInfo)
    {
        _ModelCacheService.ResyncCache();
        commandInfo.ReplyToCommand("Resynced.");
    }

}
