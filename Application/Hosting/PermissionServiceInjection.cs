using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Services;

namespace PlayerModelChanger.Hosting;

public static class PermissionServiceInjection
{
  public static void AddPermissionService(this IServiceCollection services)
  {
    services.AddSingleton<PermissionService>();
  }

  public static void UsePermissionService(this IServiceProvider provider)
  {
    provider.GetRequiredService<PermissionService>();
  }
}