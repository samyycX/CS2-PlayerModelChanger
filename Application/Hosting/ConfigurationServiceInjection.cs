using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Services;

namespace PlayerModelChanger.Hosting;

public static class ConfigurationServiceInjection
{
  public static void AddConfigurationService(this IServiceCollection services)
  {
    services.AddSingleton<ConfigurationService>();
  }

  public static void UseConfigurationService(this IServiceProvider provider)
  {
    provider.GetRequiredService<ConfigurationService>();
  }
}