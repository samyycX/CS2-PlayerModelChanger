using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlayerModelChanger.Services;

namespace PlayerModelChanger.Hosting;

public static class PluginMetadataProviderServiceInjection
{
  public static void AddPluginMetadataProviderService(this IServiceCollection services, PlayerModelChanger plugin)
  {
    PluginMetadataProviderService pluginMetadataProviderService = new(plugin);
    services.AddSingleton(pluginMetadataProviderService);
    services.AddSingleton(plugin.Logger);
    services.AddSingleton(plugin.Localizer);
    services.AddSingleton(plugin);

  }

  public static void UsePluginMetadataProviderService(this IServiceProvider provider)
  {
    provider.GetRequiredService<PluginMetadataProviderService>();
  }
}