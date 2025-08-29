using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Models;

namespace PlayerModelChanger.Hosting;

public static class GlobalVarsInjection {
  public static void AddGlobalVars(this IServiceCollection services) {
    services.AddSingleton<GlobalVars>();
  }

  public static void UseGlobalVars(this IServiceProvider provider) {
    provider.GetRequiredService<GlobalVars>();
  }
}