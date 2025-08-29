using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Services;

namespace PlayerModelChanger.Hosting;

public static class ModelCacheServiceInjection
{
  public static void AddModelCacheService(this IServiceCollection services)
  {
    services.AddSingleton<ModelCacheService>();
  }

  public static void UseModelCacheService(this IServiceProvider provider)
  {
    provider.GetRequiredService<ModelCacheService>();
  }
}