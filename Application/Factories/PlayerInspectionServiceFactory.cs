using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Services;
using PlayerModelChanger.Services.PlayerBased;

namespace PlayerModelChanger.Factories;

public class PlayerInspectionServiceFactory {
  private readonly IServiceProvider _Provider;

  public PlayerInspectionServiceFactory(IServiceProvider provider) {
    _Provider = provider;
  }

  public PlayerInspectionService Create(int slot) {
    var configurationService = _Provider.GetRequiredService<ConfigurationService>();
    var playerService = _Provider.GetRequiredService<PlayerService>();
    var modelService = _Provider.GetRequiredService<ModelService>();
    return ActivatorUtilities.CreateInstance<PlayerInspectionService>(_Provider, slot, configurationService, playerService, modelService);
  }
}