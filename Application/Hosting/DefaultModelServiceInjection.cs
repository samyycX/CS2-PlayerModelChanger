using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Services;

namespace PlayerModelChanger.Hosting;

public static class DefaultModelServiceInjection {
  public static void AddDefaultModelService(this IServiceCollection services) {
    services.AddSingleton<DefaultModelService>();
  }

  public static void UseDefaultModelService(this IServiceProvider provider) {
    provider.GetRequiredService<DefaultModelService>();
  }
}