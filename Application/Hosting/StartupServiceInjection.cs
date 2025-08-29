using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Services;

namespace PlayerModelChanger.Hosting;

public static class StartupServiceInjection
{
  public static void AddStartupService(this IServiceCollection services)
  {
    services.AddSingleton<StartupService>();
  }

  public static void UseStartupService(this IServiceProvider provider)
  {
    provider.GetRequiredService<StartupService>();
  }
}