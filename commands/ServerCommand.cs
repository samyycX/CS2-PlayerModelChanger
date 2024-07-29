using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace PlayerModelChanger;

public partial class PlayerModelChanger
{

    [ConsoleCommand("playermodelchanger_enable", "Enable/Disable the plugin.")]
    [ConsoleCommand("pmc_enable", "Enable/Disable the plugin.")]
    [CommandHelper(minArgs: 1, usage: "[true/false]", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void Switch(CCSPlayerController? player, CommandInfo commandInfo)
    {
        var arg = commandInfo.GetArg(1);
        if (arg == "1" || arg == "true")
        {
            Enable = true;
            commandInfo.ReplyToCommand(Localizer["plugin.enable"]);
        }
        else if (arg == "0" || arg == "false")
        {
            Enable = false;
            commandInfo.ReplyToCommand(Localizer["plugin.disable"]);
        }
        else
        {
            commandInfo.ReplyToCommand(Localizer["command.incorrectusage"]);
        }

    }

    [ConsoleCommand("playermodelchanger_resynccache", "Resync cache.")]
    [ConsoleCommand("pmc_resynccache", "Resync cache.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void ResyncCache(CCSPlayerController? player, CommandInfo commandInfo)
    {
        Service.ResyncCache();
        commandInfo.ReplyToCommand("Resynced.");
    }

}
