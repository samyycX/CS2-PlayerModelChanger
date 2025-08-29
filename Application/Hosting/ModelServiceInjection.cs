using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Services;

namespace PlayerModelChanger.Hosting;

public static class ModelServiceInjection
{
  public static void AddModelService(this IServiceCollection services)
  {
    services.AddSingleton<ModelService>();
  }

  public static void UseModelService(this IServiceProvider provider)
  {
    provider.GetRequiredService<ModelService>();
  }
}