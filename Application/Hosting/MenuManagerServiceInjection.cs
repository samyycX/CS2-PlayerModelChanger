using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Services;
using PlayerModelChanger.Services.Menu;

namespace PlayerModelChanger.Hosting;

public static class MenuServiceInjection
{
  public static void AddMenuService(this IServiceCollection services)
  {
    services.AddSingleton<MenuManagerService>();
    services.AddSingleton<MenuService>();
    services.AddSingleton<ModelMenuManager>();

  }

  public static void UseMenuService(this IServiceProvider provider)
  {
    provider.GetRequiredService<MenuManagerService>();
    provider.GetRequiredService<MenuService>();
    provider.GetRequiredService<ModelMenuManager>();
  }
}