using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Serilog.Debugging;
using Service;

namespace Config;

public class ModelConfig : BasePluginConfig
{
    [JsonPropertyName("Models")] public Dictionary<string, Model> Models { get; set; } = new Dictionary<string, Model>();
    
    [JsonPropertyName("MenuType")] public string MenuType { get; set; } = "chat"; // chat or centerhtml
    
    [JsonPropertyName("StorageType")] public string StorageType { get; set; } = "sqlite";

    [JsonPropertyName("MySQL_IP")] public string MySQLIP { get; set; } = "";
    [JsonPropertyName("MySQL_Port")] public string MySQLPort { get; set; } = "";
    [JsonPropertyName("MySQL_User")] public string MySQLUser { get; set; } = "";
    [JsonPropertyName("MySQL_Password")] public string MySQLPassword { get; set; } = "";
    [JsonPropertyName("MySQL_Database")] public string MySQLDatabase { get; set; } = "";
    [JsonPropertyName("MySQL_Table")] public string MySQLTable { get; set; } = "playermodelchanger";
}