using CounterStrikeSharp.API.Core;

using Config;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Storage;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using Service;
using System.Drawing;
using CounterStrikeSharp.API.Modules.Config;
namespace PlayerModelChanger;

public partial class PlayerModelChanger : BasePlugin, IPluginConfig<ModelConfig>
{
    public override string ModuleName => "Player Model Changer";
    public override string ModuleVersion => "1.4.1";

    public override string ModuleAuthor => "samyyc";
    public required ModelConfig Config { get; set; }
    public required ModelService Service { get; set; }

    public required DefaultModelManager DefaultModelManager { get;set; }

    public bool Enable = true;

    public override void Load(bool hotReload)
    {
        IStorage? Storage = null;
        switch (Config.StorageType) {
            case "sqlite":
                Storage = new SqliteStorage(ModuleDirectory);
                break;
            case "mysql":
                Storage = new MySQLStorage(Config.MySQLIP,Config.MySQLPort,Config.MySQLUser,Config.MySQLPassword,Config.MySQLDatabase,Config.MySQLTable);
                break;
        };
        if (Storage == null) {
            throw new Exception("Failed to initialize storage. Please check your config");
        }
        DefaultModelManager = new DefaultModelManager(ModuleDirectory);
        this.Service = new ModelService(Config, Storage, Localizer, DefaultModelManager);
        if (!Config.DisablePrecache) {
            RegisterListener<Listeners.OnServerPrecacheResources>(PrecacheResource);
        }
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawnEvent);
        RegisterListener<Listeners.OnMapEnd>(() => Unload(true));

        Console.WriteLine($"Player Model Changer loaded {Service.GetModelCount()} model(s) successfully.");
    }

    private void PrecacheResource(ResourceManifest manifest) {
        foreach (var model in Service.GetAllModels())
        {
            Console.WriteLine($"[PlayerModelChanger] Precaching {model.path}");
            manifest.AddResource(model.path);
        }
    }

    public override void Unload(bool hotReload)
    {
      RemoveListener("OnServerPrecacheResources", PrecacheResource);
      DeregisterEventHandler("EventPlayerSpawn", OnPlayerSpawnEvent, false);
    }

    public void ReloadConfig() {
         var config = typeof(ConfigManager)
                    .GetMethod("Load")!
                    .MakeGenericMethod(typeof(ModelConfig))
                    .Invoke(null, new object[] { Path.GetFileName(ModuleDirectory) }) as IBasePluginConfig;
        OnConfigParsed((config as ModelConfig)!);
        Unload(true);
        Load(false);
        Service.ReloadConfig(ModuleDirectory, Config);
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

        if (config.ModelForBots == null) {
          config.ModelForBots = new BotsConfig();
        }

        if (config.MenuType.ToLower() != "chat" && config.MenuType.ToLower() != "centerhtml") {
            throw new Exception($"Unknown menu type: {config.MenuType}");
        }
        config.MenuType = config.MenuType.ToLower();
        foreach (var entry in config.Models)
        {
            ModelService.InitializeModel(entry.Key, entry.Value);
        }

        Config = config;
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
            || !player.IsValid)
        {
            return HookResult.Continue;
        }
        try
        {    
            CsTeam team = (CsTeam)player.TeamNum;
              
            if (team != CsTeam.Terrorist && team != CsTeam.CounterTerrorist) {
                return HookResult.Continue;
            }

            if (player.IsBot) {
              string modelindex = team == CsTeam.Terrorist ? Config.ModelForBots.T : Config.ModelForBots.CT;
              if (modelindex == "") {
                return HookResult.Continue;
              }
              var botmodel = Service.GetModel(modelindex);
              if (botmodel != null) {
                 SetModelNextServerFrame(player.Pawn.Value, botmodel.path, botmodel.disableleg);
              } else {
                  Server.NextFrame(() => {
                      var originalRender = player.Pawn.Value.Render;
                      player.Pawn.Value.Render = Color.FromArgb(255, originalRender.R, originalRender.G, originalRender.B);
                  });
              }
            }

            if (player.AuthorizedSteamID == null) {
                return HookResult.Continue;
            }

            if (
            player.PlayerPawn == null
            || !player.PlayerPawn.IsValid
            || player.PlayerPawn.Value == null
            || !player.PlayerPawn.Value.IsValid
            ) {
              return HookResult.Continue;
            }
            
            if (Config.AutoResyncCache) {
              Service.ResyncCache();
            }

            if (!Config.DisableAutoCheck) {
                var result = Service.CheckAndReplaceModel(player);
            
                if (result.Item1 && result.Item2) {
                    player.PrintToChat(Localizer["model.invalidreseted", Localizer["side.all"]]);
                } else if (result.Item1) {
                    player.PrintToChat(Localizer["model.invalidreseted", Localizer["side.t"]]);
                } else if (result.Item2) {
                    player.PrintToChat(Localizer["model.invalidreseted", Localizer["side.ct"]]);
                }
            }
            

            var model = Service.GetPlayerNowTeamModel(player);
            
            if (model != null) {
                SetModelNextServerFrame(player.PlayerPawn.Value, model.path, model.disableleg);
            } else {
                Server.NextFrame(() => {
                    var originalRender = player.PlayerPawn.Value.Render;
                    player.PlayerPawn.Value.Render = Color.FromArgb(255, originalRender.R, originalRender.G, originalRender.B);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not set player model: {0}", ex);
        }
        
        return HookResult.Continue;
    }

    public static void SetModelNextServerFrame(CBasePlayerPawn pawn, string model, bool disableleg)
    {
        Server.NextFrame(() =>
        {
            pawn.SetModel(model);
            var originalRender = pawn.Render;
            pawn.Render = Color.FromArgb(disableleg ? 254 : 255, originalRender.R, originalRender.G, originalRender.B);
        });
    }
}
