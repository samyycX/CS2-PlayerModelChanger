using Microsoft.Extensions.DependencyInjection;
using PlayerModelChanger.Services;

namespace PlayerModelChanger.Hosting;

public static class DatabaseServiceInjection
{
  public static void AddDatabaseService(this IServiceCollection services)
  {
    services.AddSingleton<DatabaseService>();
  }

  public static void UseDatabaseService(this IServiceProvider provider)
  {
    provider.GetRequiredService<DatabaseService>();
  }
}