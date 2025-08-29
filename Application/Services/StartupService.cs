using System.Collections.Concurrent;
using System.Drawing;
using System.Reflection;
using System.Reflection.Metadata;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PlayerModelChanger.Models;

namespace PlayerModelChanger.Services;

public class StartupService {

  private PlayerModelChanger _Plugin { get; init; }
  private ModelService _ModelService { get; init; }
  private ILogger _Logger { get; init; }

  private IStringLocalizer _Localizer { get; init; }
  private ConfigurationService _ConfigurationService { get; init; }
  private GlobalVars _GlobalVars { get; init; }
  private ModelCacheService _ModelCacheService { get; init; }
  private PlayerService _PlayerService { get; init; }


  public StartupService(
    PlayerModelChanger plugin,
    ModelService modelService,
    PlayerService playerService,
    ILogger logger,
    IStringLocalizer localizer,
    ConfigurationService configurationService,
    ModelCacheService modelCacheService,
    GlobalVars globalVars) {
    _Plugin = plugin;
    _ModelService = modelService;
    _PlayerService = playerService;
    _Logger = logger;
    _Localizer = localizer;
    _ConfigurationService = configurationService;
    _ModelCacheService = modelCacheService;
    _GlobalVars = globalVars;

    InitializeLangPrefix();

    if (!_ConfigurationService.ModelConfig.DisablePrecache) {
      _Plugin.RegisterListener<Listeners.OnServerPrecacheResources>(manifest => {
        foreach (var model in _ModelService.GetAllModels())
        {
          _Logger.LogInformation($"Precaching {model.Path}");
          manifest.AddResource(model.Path);
        }
      });
    }

    _Plugin.RegisterEventHandler<EventPlayerActivate>((@event, info) =>
    {
      _PlayerService.AddPlayerServices(@event.Userid!.Slot);
      return HookResult.Continue;
    });

    _Plugin.RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
    {
      _PlayerService.RemovePlayerServices(@event.Userid!.Slot);
      return HookResult.Continue;
    });


    _Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawnEvent);

    _Plugin.RegisterListener<Listeners.OnTick>(() => {
      _PlayerService.Update();
    });

    _Plugin.RegisterListener<Listeners.OnMapEnd>(() =>
      {
        _ModelService.ClearMapDefaultModel();
      }
    );

    for (int i = 0; i < _ConfigurationService.ModelConfig.Models.Count; i++)
    {
      var entry = _ConfigurationService.ModelConfig.Models.ElementAt(i);
      _ModelService.InitializeModel(entry.Key, entry.Value);
      if (_ConfigurationService.ModelConfig.Models.Where(m => m.Value.Name == entry.Value.Name).Count() > 1)
      {
        throw new Exception($"[PlayerModelChanger] Found duplicated model name: {entry.Value.Name}");
      }
    }

    _Logger.LogInformation($"Loaded {_ModelService.GetModelCount()} model(s) successfully.");

  }

  public void InitializeLangPrefix()
    {
    var localizerField = _Localizer.GetType().GetField("_localizer", BindingFlags.Instance | BindingFlags.NonPublic)!;
    var internalLocalizer = localizerField.GetValue(_Localizer)!;
    var jsonResourceManagerField = internalLocalizer.GetType().GetField("_resourceManager", BindingFlags.Instance | BindingFlags.NonPublic)!;
    JsonResourceManager jsonResourceManager = (JsonResourceManager)jsonResourceManagerField.GetValue(internalLocalizer)!;
    var resourcesCacheField = jsonResourceManager.GetType().GetField("_resourcesCache", BindingFlags.Instance | BindingFlags.NonPublic)!;
    var tryLoadResourceSet = jsonResourceManager.GetType().GetMethod("TryLoadResourceSet", BindingFlags.NonPublic | BindingFlags.Instance, [typeof(string)])!;
    tryLoadResourceSet.Invoke(jsonResourceManager, ["en"]);
    tryLoadResourceSet.Invoke(jsonResourceManager, ["pt-BR"]);
    tryLoadResourceSet.Invoke(jsonResourceManager, ["ru"]);
    tryLoadResourceSet.Invoke(jsonResourceManager, ["zh-Hans"]);

    jsonResourceManager.GetString("command.model.success"); // make it initialize
    ConcurrentDictionary<string, ConcurrentDictionary<string, string>> resourcesCache = (ConcurrentDictionary<string, ConcurrentDictionary<string, string>>)resourcesCacheField.GetValue(jsonResourceManager)!;
    foreach (var caches in resourcesCache)
    {
      foreach (var key in caches.Value.Keys)
      {
        caches.Value[key] = caches.Value[key].Replace("%pmc_prefix%", "[{green}PlayerModelChanger{default}] ");
      }
    }
  }

  // from https://github.com/Challengermode/cm-cs2-defaultskins/
  [GameEventHandler]
  public HookResult OnPlayerSpawnEvent(EventPlayerSpawn @event, GameEventInfo info)
  {
    if (!_GlobalVars.Enable)
    {
      return HookResult.Continue;
    }

    if (@event == null)
    {
      return HookResult.Continue;
    }

    CCSPlayerController? player = @event.Userid;

    if (player == null
        || !player.IsValid)
    {
      return HookResult.Continue;
    }
    try
    {
      CsTeam team = (CsTeam)player.TeamNum;

      if (team != CsTeam.Terrorist && team != CsTeam.CounterTerrorist)
      {
        return HookResult.Continue;
      }

      if (player.IsBot)
      {
        List<string> modelindexs = team == CsTeam.Terrorist ? _ConfigurationService.ModelConfig.ModelForBots.T : _ConfigurationService.ModelConfig.ModelForBots.CT;
        if (modelindexs.Count() == 0)
        {
          return HookResult.Continue;
        }
        var modelindex = modelindexs[Random.Shared.Next(modelindexs.Count)];
        var botmodel = _ModelService.GetModel(modelindex);
        if (modelindex == "@random")
        {
          botmodel = _ModelService.GetRandomModel(player, team == CsTeam.Terrorist ? Side.T : Side.CT);
        }
        if (botmodel != null)
        {
          _ModelService.SetModelNextServerFrame(player, botmodel, botmodel.Disableleg);
        }
        else
        {
          Server.NextFrame(() =>
          {
            var originalRender = player.Pawn.Value!.Render;
            player.Pawn.Value.Render = Color.FromArgb(255, originalRender.R, originalRender.G, originalRender.B);
          });
        }
        return HookResult.Continue;
      }

      if (player.AuthorizedSteamID == null)
      {
        return HookResult.Continue;
      }

      if (
      player.PlayerPawn == null
      || !player.PlayerPawn.IsValid
      || player.PlayerPawn.Value == null
      || !player.PlayerPawn.Value.IsValid
      )
      {
        return HookResult.Continue;
      }

      if (_ConfigurationService.ModelConfig.AutoResyncCache)
      {
        _ModelCacheService.ResyncCache();
      }

      if (!_ConfigurationService.ModelConfig.DisableAutoCheck)
      {
        var result = _ModelService.CheckAndReplaceModel(player);

        if (result.Item1 && result.Item2)
        {
          player.PrintToChat(_Localizer["model.invalidreseted", _Localizer["side.all"]]);
        }
        else if (result.Item1)
        {
          player.PrintToChat(_Localizer["model.invalidreseted", _Localizer["side.t"]]);
        }
        else if (result.Item2)
        {
          player.PrintToChat(_Localizer["model.invalidreseted", _Localizer["side.ct"]]);
        }
      }

      Server.NextFrame(() =>
      {
        if (!_ModelService.MapDefaultModelInitialized(player))
        {
          _ModelService.SetMapDefaultModel(player, player.PlayerPawn.Value.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName);
        }
        Server.NextFrame(() =>
              {
            var model = _ModelService.GetPlayerNowTeamModel(player);
            if (model != null)
            {
              _ModelService.SetModelNextServerFrame(player, model, model.Disableleg);
            }
            else
            {
              var originalRender = player.PlayerPawn.Value.Render;
              player.PlayerPawn.Value.Render = Color.FromArgb(_ConfigurationService.ModelConfig.DisableDefaultModelLeg ? 254 : 255, originalRender.R, originalRender.G, originalRender.B);
              Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseModelEntity", "m_clrRender");
            }
          });
      });
    }
    catch (Exception ex)
    {
      _Logger.LogInformation("Could not set player model: {0}", ex);
    }

    return HookResult.Continue;
  }

}