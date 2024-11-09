using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
namespace PlayerModelChanger;

public class BotsConfig
{
    public string CT { get; set; } = "";
    public string T { get; set; } = "";

}

public class InspectionConfig
{
    public bool Enable { get; set; } = true;
    public string Mode { get; set; } = "rotation";
}

public class ModelConfig : BasePluginConfig
{
    [JsonPropertyName("Models")] public Dictionary<string, Model> Models { get; set; } = new Dictionary<string, Model>();
    [JsonPropertyName("StorageType")] public string StorageType { get; set; } = "sqlite";

    [JsonPropertyName("MySQL_IP")] public string MySQLIP { get; set; } = "";
    [JsonPropertyName("MySQL_Port")] public string MySQLPort { get; set; } = "";
    [JsonPropertyName("MySQL_User")] public string MySQLUser { get; set; } = "";
    [JsonPropertyName("MySQL_Password")] public string MySQLPassword { get; set; } = "";
    [JsonPropertyName("MySQL_Database")] public string MySQLDatabase { get; set; } = "";
    [JsonPropertyName("MySQL_Table")] public string MySQLTable { get; set; } = "playermodelchanger";

    [JsonPropertyName("ModelForBots")] public BotsConfig ModelForBots { get; set; } = new BotsConfig();
    [JsonPropertyName("ModelChangeCooldownSecond")] public float ModelChangeCooldownSecond { get; set; } = 0f;
    [JsonPropertyName("Inspection")] public InspectionConfig Inspection { get; set; } = new InspectionConfig();

    [JsonPropertyName("DisableInstantChange")] public bool DisableInstantChange { get; set; } = false;
    [JsonPropertyName("DisablePrecache")] public bool DisablePrecache { get; set; } = false;
    [JsonPropertyName("DisableRandomModel")] public bool DisableRandomModel { get; set; } = false;
    [JsonPropertyName("DisableAutoCheck")] public bool DisableAutoCheck { get; set; } = false;
    [JsonPropertyName("DisablePlayerSelection")] public bool DisablePlayerSelection { get; set; } = false;
    [JsonPropertyName("AutoResyncCache")] public bool AutoResyncCache { get; set; } = false;
    [JsonPropertyName("ConfigVersion")] public override int Version { get; set; } = 1;
}
