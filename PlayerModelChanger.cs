using CounterStrikeSharp.API.Core;

using Config;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Data;
using CounterStrikeSharp.API;
using System.Text;
using Newtonsoft.Json.Linq;
namespace PlayerModelChanger;

public class PlayerModelChanger : BasePlugin, IPluginConfig<ModelConfig>
{
    public override string ModuleName => "Player Model Changer";
    public override string ModuleVersion => "1.0.0";
    public ModelConfig Config { get; set; }
    public PlayerData Data { get; set; } = new PlayerData();
    public override void Load(bool hotReload)
    {

        Data.initialize(ModuleDirectory);

        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawnEvent);
        RegisterListener<Listeners.OnMapEnd>(() => Unload(true));

        Console.WriteLine($"Player Model Changer loaded {Config.ModelPaths.Count()} model(s) successfully.");

    }   

    public void OnConfigParsed(ModelConfig config)
    {
        Config = config;
    }

    [ConsoleCommand("playermodelchanger_sync_resourceprecacher", "Server only. Sync your resourceprecacher config. (add model paths after the original config)")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void Sync(CCSPlayerController? player, CommandInfo commandInfo) {

        var ConfigPath = Path.Join(ModuleDirectory, "../../configs/plugins/ResourcePrecacher/ResourcePrecacher.json");
        var JsonString = File.ReadAllText(ConfigPath, Encoding.UTF8);

        var jobject = JObject.Parse(JsonString);

        var array = (JArray)jobject["Resources"];
        foreach (var item in Config.ModelPaths.Values)
        {
            array.Add(item);
        }
        jobject["Resources"] = array;

        File.WriteAllText(ConfigPath, jobject.ToString());
    }

    [ConsoleCommand("css_model", "Change your model.")]
    [CommandHelper(minArgs: 0, usage: "<model name>", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void ChangeModelCommand(CCSPlayerController? player, CommandInfo commandInfo) {

        if (commandInfo.ArgCount == 1) {
            var usingModelName = Data.GetPlayerModel(player.AuthorizedSteamID.SteamId64);
            if (usingModelName == "") {
                commandInfo.ReplyToCommand($"You are currently not using any models.");
            } else {
                commandInfo.ReplyToCommand($"You are using model: {usingModelName}");
            }
            commandInfo.ReplyToCommand($"Type '!model <model name>' to change your model.");
            commandInfo.ReplyToCommand($"Type '!models' for a list of all available models.");
            return;
        }

        var modelName = commandInfo.GetArg(1);

        if (!Config.ModelPaths.ContainsKey(modelName)) {
            commandInfo.ReplyToCommand($"Model Name {modelName} not found.");
            return;
        }

        Data.SetPlayerModel(player.AuthorizedSteamID.SteamId64, modelName);
        commandInfo.ReplyToCommand($"Your model will be set after next spawn.");
    }

    [ConsoleCommand("css_models", "List all models.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void GetAllModelsCommand(CCSPlayerController? player, CommandInfo commandInfo) {
        commandInfo.ReplyToCommand($"Available models ({Config.ModelPaths.Count()}): "+ string.Join("   ", Config.ModelPaths.Keys));
    }


    // from https://github.com/Challengermode/cm-cs2-defaultskins/
    [GameEventHandler]
    public HookResult OnPlayerSpawnEvent(EventPlayerSpawn @event, GameEventInfo info) {
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
            // CsTeam team = player.PendingTeamNum != player.TeamNum ? (CsTeam)player.PendingTeamNum : (CsTeam)player.TeamNum;

            // TODO: different models for CT and T? (may change database structure)

            if (player.AuthorizedSteamID == null) {
                // bot?
                return HookResult.Continue;
            }
            var modelName = Data.GetPlayerModel(player.AuthorizedSteamID.SteamId64);
            var modelPath = Config.ModelPaths.GetValueOrDefault(modelName, "");

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
