using CounterStrikeSharp.API.Core;

using Config;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Storage;
using CounterStrikeSharp.API;
using System.Text;
using Newtonsoft.Json.Linq;
using CounterStrikeSharp.API.Modules.Utils;
namespace PlayerModelChanger;

public class PlayerModelChanger : BasePlugin, IPluginConfig<ModelConfig>
{
    public override string ModuleName => "Player Model Changer";
    public override string ModuleVersion => "1.0.2";

    public override string ModuleAuthor => "samyyc";
    public required ModelConfig Config { get; set; }
    public required IStorage Storage { get; set; }

    public bool Enable = true;
    public override void Load(bool hotReload)
    {
        switch (Config.StorageType) {
            case "sqlite":
                Storage = new SqliteStorage(ModuleDirectory);
                break;
            case "mysql":
                Storage = new MySQLStorage(
                    Config.MySQLIP,
                    Config.MySQLPort,
                    Config.MySQLUser,
                    Config.MySQLPassword,
                    Config.MySQLDatabase,
                    Config.MySQLTable
                );
                break;
        };

        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawnEvent);
        RegisterListener<Listeners.OnMapEnd>(() => Unload(true));

        Console.WriteLine($"Player Model Changer loaded {Config.ModelPaths.Count()} model(s) successfully.");

    }   

    public void OnConfigParsed(ModelConfig config)
    {
        var availableStorageType = new []{"sqlite", "mysql"};
        if (!availableStorageType.Contains(config.StorageType)) {
            throw new Exception($"Unknown storage type: {Config.StorageType}, available types: {string.Join(",", availableStorageType)}");
        }

        if (config.StorageType == "mysql") {
            if (config.MySQLIP == "") {
                throw new Exception("You must fill in the MySQL_IP");
            }
            if (config.MySQLPort == "") {
                throw new Exception("You must fill in the MYSQL_Port");
            }
            if (config.MySQLUser == "") {
                throw new Exception("You must fill in the MYSQL_User");
            }
            if (config.MySQLPassword == "") {
                throw new Exception("You must fill in the MYSQL_Password");
            }
            if (config.MySQLDatabase == "") {
                throw new Exception("You must fill in the MySQL_Database");
            }
        }
        Config = config;
    }

    [ConsoleCommand("playermodelchanger_sync_resourceprecacher", "Server only. Sync your resourceprecacher config. (add model paths after the original config)")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void Sync(CCSPlayerController? player, CommandInfo commandInfo) {

        var ConfigPath = Path.Join(ModuleDirectory, "../../configs/plugins/ResourcePrecacher/ResourcePrecacher.json");
        var JsonString = File.ReadAllText(ConfigPath, Encoding.UTF8);

        var jobject = JObject.Parse(JsonString);

        var array = (JArray)jobject["Resources"]!;
        foreach (var item in Config.ModelPaths.Values)
        {
            array.Add(item);
        }
        jobject["Resources"] = array;

        File.WriteAllText(ConfigPath, jobject.ToString());
    }

    [ConsoleCommand("playermodelchanger_enable", "Enable/Disable the plugin.")]
    [CommandHelper(minArgs: 1, usage: "[true/false]", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void Switch(CCSPlayerController? player, CommandInfo commandInfo) {
        var arg = commandInfo.GetArg(1);
        if (arg == "1" || arg == "true") {
            Enable = true;
            commandInfo.ReplyToCommand(Localizer["plugin.enable"]);
        } else if (arg == "0" || arg == "false") {
            Enable = false;
            commandInfo.ReplyToCommand(Localizer["plugin.disable"]);
        } else {
            commandInfo.ReplyToCommand(Localizer["command.incorrectusage"]);
        }
        
    }

    [ConsoleCommand("css_model", "Change your model.")]
    [CommandHelper(minArgs: 0, usage: "<model name> <all/ct/t>", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void ChangeModelCommand(CCSPlayerController? player, CommandInfo commandInfo) {

        if (commandInfo.ArgCount == 1) {
            var usingTModelName = Storage.GetPlayerTModel(player!.AuthorizedSteamID!.SteamId64);
            var usingCTModelName = Storage.GetPlayerCTModel(player!.AuthorizedSteamID!.SteamId64);
            if (usingTModelName == "") {
                commandInfo.ReplyToCommand(Localizer["player.notusingmodel.t"]);
            } else {
                commandInfo.ReplyToCommand(Localizer["player.currentmodel.t",usingTModelName]);
            }
             if (usingCTModelName == "") {
                commandInfo.ReplyToCommand(Localizer["player.notusingmodel.ct"]);
            } else {
                commandInfo.ReplyToCommand(Localizer["player.currentmodel.ct",usingCTModelName]);
            }
            commandInfo.ReplyToCommand(Localizer["command.model.hint1"]);
            commandInfo.ReplyToCommand(Localizer["command.model.hint2"]);
            commandInfo.ReplyToCommand(Localizer["command.model.hint3"]);
            return;
        }

        var modelName = commandInfo.GetArg(1);

        if (modelName != "@random" && !Config.ModelPaths.ContainsKey(modelName)) {
            commandInfo.ReplyToCommand(Localizer["command.model.notfound", modelName]);
            return;
        }

        var side = "all";
        if (commandInfo.ArgCount == 3) {
            side = commandInfo.GetArg(2).ToLower();
        }

        if (side == "all") {
            Storage.SetPlayerTModel(player!.AuthorizedSteamID!.SteamId64, modelName);
            Storage.SetPlayerCTModel(player!.AuthorizedSteamID!.SteamId64, modelName);
        } else if (side == "t") {
            Storage.SetPlayerTModel(player!.AuthorizedSteamID!.SteamId64, modelName);
        } else if (side == "ct") {
            Storage.SetPlayerCTModel(player!.AuthorizedSteamID!.SteamId64, modelName);
        } else {
            commandInfo.ReplyToCommand(Localizer["command.unknownside", side]);
            return;
        }
        commandInfo.ReplyToCommand(Localizer["command.model.success"]);
    }

    [ConsoleCommand("css_resetmodel", "Reset your model.")]
    [CommandHelper(minArgs: 0, usage: "<all/ct/t>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void ResetModelCommand(CCSPlayerController? player, CommandInfo commandInfo) {
        var side = "all";
        if (commandInfo.ArgCount == 2) {
            side = commandInfo.GetArg(1).ToLower();
        }

        if (side == "all") {
            Storage.SetPlayerTModel(player!.AuthorizedSteamID!.SteamId64, "");
            Storage.SetPlayerCTModel(player!.AuthorizedSteamID!.SteamId64, "");
        } else if (side == "t") {
            Storage.SetPlayerTModel(player!.AuthorizedSteamID!.SteamId64, "");
        } else if (side == "ct") {
            Storage.SetPlayerCTModel(player!.AuthorizedSteamID!.SteamId64, "");
        } else {
            commandInfo.ReplyToCommand(Localizer["command.unknownside", side]);
            return;
        }
        commandInfo.ReplyToCommand(Localizer["command.resetmodel.success"]);
    }

    [ConsoleCommand("css_models", "List all models.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void GetAllModelsCommand(CCSPlayerController? player, CommandInfo commandInfo) {
        commandInfo.ReplyToCommand(Localizer["command.models", Config.ModelPaths.Count()] + string.Join("   ", Config.ModelPaths.Keys));
    }


    // from https://github.com/Challengermode/cm-cs2-defaultskins/
    [GameEventHandler]
    public HookResult OnPlayerSpawnEvent(EventPlayerSpawn @event, GameEventInfo info) {
        
        if (!Enable) {
            return HookResult.Continue;
        }

          if(@event == null)
        {
            return HookResult.Continue;
        }

        CCSPlayerController player = @event.Userid;

        if (player == null
            || !player.IsValid
            || player.PlayerPawn == null
            || !player.PlayerPawn.IsValid
            || player.PlayerPawn.Value == null
            || !player.PlayerPawn.Value.IsValid)
        {
            return HookResult.Continue;
        }

        try
        {
            // TODO: Server crash if player connects, mp_swapteams and reconnect       
            CsTeam team = player.PendingTeamNum != player.TeamNum ? (CsTeam)player.PendingTeamNum : (CsTeam)player.TeamNum;

            // TODO: different models for CT and T? (may change database structure)

            if (player.AuthorizedSteamID == null) {
                // bot?
                return HookResult.Continue;
            }
            if (team != CsTeam.Terrorist && team != CsTeam.CounterTerrorist) {
                return HookResult.Continue;
            }
            
            var modelName = "";
            if (team == CsTeam.Terrorist) {
                modelName = Storage.GetPlayerTModel(player.AuthorizedSteamID.SteamId64);
            } else {
                modelName = Storage.GetPlayerCTModel(player.AuthorizedSteamID.SteamId64);
            }
            var modelPath = "";
            if (modelName == "@random") {
                var entry = Config.ModelPaths.ElementAt(Random.Shared.Next(Config.ModelPaths.Count()));
                modelPath = entry.Value;
                // player.PrintToChat(Localizer["random.using", entry.Key]);
            } else {
                modelPath = Config.ModelPaths.GetValueOrDefault(modelName, "");
            }

            if (modelPath != "") {
                SetModelNextServerFrame(player.PlayerPawn.Value, modelPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not set player model: {0}", ex);
        }
        
        return HookResult.Continue;
    }

    public static void SetModelNextServerFrame(CCSPlayerPawn playerPawn, string model)
    {
        Server.NextFrame(() =>
        {
            playerPawn.SetModel(model);
        });
    }
}
