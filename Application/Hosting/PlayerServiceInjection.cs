using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Services;

namespace PlayerModelChanger.Hosting;

public static class PlayerServiceInjection
{
  public static void AddPlayerService(this IServiceCollection services)
  {
    services.AddSingleton<PlayerService>();
  }

  public static void UsePlayerService(this IServiceProvider provider)
  {
    provider.GetRequiredService<PlayerService>();
  }
}