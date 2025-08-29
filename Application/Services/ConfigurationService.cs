using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace PlayerModelChanger.Services;

public class ConfigurationService
{

  private PluginMetadataProviderService _PluginMetadataProviderService { get; init; }
  public ModelConfig ModelConfig { get; private set; }

  public ConfigTemplate DefaultModelConfig { get; private set; }

  public ConfigurationService(PluginMetadataProviderService pluginMetadataProviderService)
  {
    _PluginMetadataProviderService = pluginMetadataProviderService;
    Reload();
  }

  public void Reload()
  {
    var configPath = Path.Join(
      _PluginMetadataProviderService.GetPluginMetadata().ModuleDirectory,
      "../../configs/plugins/PlayerModelChanger/PlayerModelChanger.json");

    var defaultConfigPath = Path.Join(
      _PluginMetadataProviderService.GetPluginMetadata().ModuleDirectory,
      "../../configs/plugins/PlayerModelChanger/DefaultModels.json");

    SaveDefaultIfNotExists<ModelConfig>(configPath);
    SaveDefaultIfNotExists<ConfigTemplate>(defaultConfigPath);

    ModelConfig = JsonSerializer.Deserialize<ModelConfig>(File.ReadAllText(configPath)) ?? new();
    DefaultModelConfig = JsonSerializer.Deserialize<ConfigTemplate>(File.ReadAllText(defaultConfigPath)) ?? new();
  }

  private void SaveDefaultIfNotExists<T>(string configPath) where T : new()
  {
    T config = new();
    new FileInfo(configPath).Directory?.Create();
    if (!File.Exists(configPath))
    {
      File.WriteAllText(configPath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
    }
  }
}