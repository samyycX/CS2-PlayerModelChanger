using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using PlayerModelChanger.Services.Menu;

namespace PlayerModelChanger.Factories;

public class MenuFactory {
  private readonly IServiceProvider _ServiceProvider;

  public MenuFactory(IServiceProvider serviceProvider) {
    _ServiceProvider = serviceProvider;
  }

  public WasdModelMenu CreateMenu() {
    var localizer = _ServiceProvider.GetRequiredService<IStringLocalizer>();
    return ActivatorUtilities.CreateInstance<WasdModelMenu>(_ServiceProvider, localizer);
  }

  public T CreateMenuOption<T>() where T : MenuOption {
    var menuManager = _ServiceProvider.GetRequiredService<ModelMenuManager>();
    return ActivatorUtilities.CreateInstance<T>(_ServiceProvider, menuManager);
  }
}