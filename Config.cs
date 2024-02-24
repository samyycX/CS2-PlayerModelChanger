using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Config;

public class ModelConfig : BasePluginConfig
{
    [JsonPropertyName("ModelPaths")] public Dictionary<string, string> ModelPaths { get; set; } = new Dictionary<string, string>();
 
}