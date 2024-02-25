using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Config;

public class ModelConfig : BasePluginConfig
{
    [JsonPropertyName("ModelPaths")] public Dictionary<string, string> ModelPaths { get; set; } = new Dictionary<string, string>();
    
    [JsonPropertyName("StorageType")] public string StorageType { get; set; } = "sqlite";

    [JsonPropertyName("MySQL_IP")] public string MySQLIP { get; set; } = "";
    [JsonPropertyName("MySQL_Port")] public string MySQLPort { get; set; } = "";
    [JsonPropertyName("MySQL_User")] public string MySQLUser { get; set; } = "";
    [JsonPropertyName("MySQL_Password")] public string MySQLPassword { get; set; } = "";
    [JsonPropertyName("MySQL_Database")] public string MySQLDatabase { get; set; } = "";
    [JsonPropertyName("MySQL_Table")] public string MySQLTable { get; set; } = "playermodelchanger";

}