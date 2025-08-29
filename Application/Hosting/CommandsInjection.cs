using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Services.Commands;

namespace PlayerModelChanger.Hosting;

public static class CommandsInjection {
  public static void AddCommands(this IServiceCollection services) {
    services.AddSingleton<AdminCommand>();
    services.AddSingleton<PlayerCommand>();
    services.AddSingleton<ServerCommand>();
  }

  public static void UseCommands(this IServiceProvider provider) {
    provider.GetRequiredService<AdminCommand>();
    provider.GetRequiredService<PlayerCommand>();
    provider.GetRequiredService<ServerCommand>();
  }
}