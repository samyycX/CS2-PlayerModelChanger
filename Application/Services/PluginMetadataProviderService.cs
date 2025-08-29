using PlayerModelChanger.Models;

namespace PlayerModelChanger.Services;

public class PluginMetadataProviderService
{

  private PluginMetadata _PluginMetadata { get; init; }

  private PlayerModelChanger _Plugin { get; init; }

  public PluginMetadataProviderService(PlayerModelChanger plugin)
  {
    _Plugin = plugin;
    _PluginMetadata = new PluginMetadata(
      plugin.ModuleName,
      plugin.ModuleVersion,
      plugin.ModuleAuthor,
      plugin.ModuleDirectory
    );
  }

  public PluginMetadata GetPluginMetadata()
  {
    return _PluginMetadata;
  }
}