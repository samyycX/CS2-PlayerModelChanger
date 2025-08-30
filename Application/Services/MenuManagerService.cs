using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Services.Menu;

namespace PlayerModelChanger.Services;

public class MenuManagerService
{

  private readonly PluginMetadataProviderService _PluginMetadataProviderService;
  private readonly ModelMenuManager _MenuManager;

  private readonly PlayerModelChanger _Plugin;

  public MenuManagerService(
    PluginMetadataProviderService pluginMetadataProviderService, 
    ModelMenuManager menuManager,
    PlayerModelChanger plugin)
  {
    _PluginMetadataProviderService = pluginMetadataProviderService;
    _MenuManager = menuManager;
    _Plugin = plugin;

    _Plugin.RegisterEventHandler<EventPlayerActivate>((@event, info) =>
        {
          if (@event.Userid != null)
          {
            _MenuManager.RemovePlayer(@event.Userid.Slot);
            _MenuManager.AddPlayer(@event.Userid.Slot, new ModelMenuPlayer { Player = @event.Userid, Buttons = 0 });
          }
          return HookResult.Continue;
        });

    _Plugin.RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
    {
      if (@event.Userid != null)
      {
        _MenuManager.RemovePlayer(@event.Userid.Slot);
      }
      return HookResult.Continue;
    });

    _Plugin.RegisterListener<Listeners.OnTick>(() => {
      _MenuManager.Update();
    });
  }
}