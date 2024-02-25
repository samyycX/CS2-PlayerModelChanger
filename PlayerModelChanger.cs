using CounterStrikeSharp.API.Core;

using Config;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Storage;
using CounterStrikeSharp.API;
using System.Text;
using Newtonsoft.Json.Linq;
using CounterStrikeSharp.API.Modules.Utils;
using Service;
using CounterStrikeSharp.API.Modules.Menu;
namespace PlayerModelChanger;

public class PlayerModelChanger : BasePlugin, IPluginConfig<ModelConfig>
{
    public override string ModuleName => "Player Model Changer";
    public override string ModuleVersion => "1.0.3";

    public override string ModuleAuthor => "samyyc";
    public required ModelConfig Config { get; set; }
    public required ModelService Service { get; set; }

    public bool Enable = true;
    public override void Load(bool hotReload)
    {
        IStorage Storage = null;
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
        if (Storage == null) {
            throw new Exception("Failed to initialize storage. Please check your config");
        }
        this.Service = new ModelService(Config, Storage, Localizer);

        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawnEvent);
        RegisterListener<Listeners.OnMapEnd>(() => Unload(true));

        Console.WriteLine($"Player Model Changer loaded {Service.GetModelCount()} model(s) successfully.");

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

        foreach (var entry in config.Models)
        {
            ModelService.InitializeModel(entry.Key, entry.Value);
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
        foreach (var model in Service.GetAllModels())
        {
            array.Add(model.path);
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

    [ConsoleCommand("playermodelchanger_resynccache", "Resync cache.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void ResyncCache(CCSPlayerController? player, CommandInfo commandInfo) {
        Service.ResyncCache();
        commandInfo.ReplyToCommand("Resynced.");
    }

    [ConsoleCommand("css_model", "Change your model.")]
    [CommandHelper(minArgs: 0, usage: "<model name> <all/ct/t>", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void ChangeModelCommand(CCSPlayerController? player, CommandInfo commandInfo) {

        if (commandInfo.ArgCount == 1) {
            var TModel = Service.GetPlayerModelName(player, CsTeam.Terrorist);
            var CTModel = Service.GetPlayerModelName(player, CsTeam.CounterTerrorist);
            if (TModel == "") {
                commandInfo.ReplyToCommand(Localizer["player.notusingmodel.t"]);
            } else {
                commandInfo.ReplyToCommand(Localizer["player.currentmodel.t",TModel]);
            }
             if (CTModel == "") {
                commandInfo.ReplyToCommand(Localizer["player.notusingmodel.ct"]);
            } else {
                commandInfo.ReplyToCommand(Localizer["player.currentmodel.ct",CTModel]);
            }
            commandInfo.ReplyToCommand(Localizer["command.model.hint1"]);
            commandInfo.ReplyToCommand(Localizer["command.model.hint2"]);
            return;
        }

        var modelName = commandInfo.GetArg(1);

        if (modelName != "@random" && !Service.ExistModel(modelName)) {
            commandInfo.ReplyToCommand(Localizer["command.model.notfound", modelName]);
            return;
        }

        var side = "all";
        if (commandInfo.ArgCount == 3) {
            side = commandInfo.GetArg(2).ToLower();
        }

        if (side == "all") {
            Service.SetPlayerTModel(player, modelName);
            Service.SetPlayerCTModel(player, modelName);
        } else if (side == "t") {
            Service.SetPlayerTModel(player, modelName);
        } else if (side == "ct") {
            Service.SetPlayerCTModel(player, modelName);
        } else {
            commandInfo.ReplyToCommand(Localizer["command.unknownside", side]);
            return;
        }
    }

    [ConsoleCommand("css_models", "List all models.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void GetAllModelsCommand(CCSPlayerController? player, CommandInfo commandInfo) {
        ChatMenu modelMenu = new ChatMenu(Localizer["modelmenu.title"]);

        var side = player.Team == CsTeam.Terrorist ? "t" : "ct";
        if (commandInfo.ArgCount == 2) {
            side = commandInfo.GetArg(1);
        }

        List<Model> models;
        if (side == "all") {
            models = Service.GetAllSideAppliableModels();
        } else {
            var team = side.ToLower() == "t" ? CsTeam.Terrorist : CsTeam.CounterTerrorist;
            models = Service.GetAllAppliableModels(team); 
        }

        modelMenu.AddMenuOption(Localizer["modelmenu.unset"], (player, option) => HandleModelMenu(player, option, side));
        modelMenu.AddMenuOption(Localizer["modelmenu.random"], (player, option) => HandleModelMenu(player, option, side));
        foreach (var model in models)
        {
            modelMenu.AddMenuOption($"{model.name} [{model.index}]", (player, option) => HandleModelMenu(player, option, side));
        }

        if (modelMenu.MenuOptions.Count == 0) {
            commandInfo.ReplyToCommand(Localizer["modelmenu.nomodel"]);
        }
        MenuManager.OpenChatMenu(player, modelMenu);
    }

    private void HandleModelMenu(CCSPlayerController player, ChatMenuOption option, string side) {
        var parts = option.Text.Split('[', ']');
        
        var modelIndex = "";
        if (option.Text == Localizer["modelmenu.unset"]) {
            modelIndex = "";
        } else if (option.Text == Localizer["modelmenu.random"]) {
            modelIndex = "@random";
        } else {
            modelIndex = parts[parts.Length - 2]; 
        }

         if (side == "all") {
            Service.SetPlayerTModel(player, modelIndex);
            Service.SetPlayerCTModel(player, modelIndex);
        } else if (side == "t") {
            Service.SetPlayerTModel(player, modelIndex);
        } else if (side == "ct") {
            Service.SetPlayerCTModel(player, modelIndex);
        } else {
            player.PrintToChat(Localizer["command.unknownside", side]);
            return;
        }
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
            CsTeam team = (CsTeam)player.TeamNum;

            if (player.AuthorizedSteamID == null) {
                // bot?
                return HookResult.Continue;
            }
            if (team != CsTeam.Terrorist && team != CsTeam.CounterTerrorist) {
                return HookResult.Continue;
            }
            var model = Service.GetPlayerModel(player);
            if (model != null) {
                SetModelNextServerFrame(player.PlayerPawn.Value, model.path);
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
