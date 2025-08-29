using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Hosting;

namespace PlayerModelChanger;

internal static class Bootstrap
{
  public static void Run(PlayerModelChanger plugin, bool hotReload)
  {
    ServiceCollection services = new();

    services.AddPluginMetadataProviderService(plugin);
    services.AddConfigurationService();
    services.AddMenuService();
    services.AddDatabaseService();
    services.AddModelCacheService();
    services.AddModelService();
    services.AddPlayerService();
    services.AddStartupService();
    services.AddDefaultModelService();
    services.AddPermissionService();
    services.AddGlobalVars();
    services.AddCommands();
    services.AddFactory();

    var provider = services.BuildServiceProvider();

    provider.UsePluginMetadataProviderService();
    provider.UseConfigurationService();
    provider.UseMenuService();
    provider.UseDatabaseService();
    provider.UseModelCacheService();
    provider.UseModelService();
    provider.UsePlayerService();
    provider.UseStartupService();
    provider.UseDefaultModelService();
    provider.UsePermissionService();
    provider.UseGlobalVars();
    provider.UseCommands();
    provider.UseFactory();

  }
}