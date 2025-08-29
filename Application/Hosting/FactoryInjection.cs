using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Factories;
using PlayerModelChanger.Services.PlayerBased;

namespace PlayerModelChanger.Hosting;

public static class FactoryInjection {
  public static void AddFactory(this IServiceCollection services) {
    services.AddSingleton<MenuFactory>();
    services.AddSingleton<PlayerInspectionServiceFactory>();
  }

  public static void UseFactory(this IServiceProvider provider) {
    provider.GetRequiredService<MenuFactory>();
    provider.GetRequiredService<PlayerInspectionServiceFactory>();
  }
}