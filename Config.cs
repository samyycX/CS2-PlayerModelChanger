using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
namespace PlayerModelChanger;

public class BotIndexConvertor : JsonConverter<List<string>>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(List<string>) || typeToConvert == typeof(string);
    }

    public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new List<string> { reader.GetString()! };
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<string>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;
                if (reader.TokenType == JsonTokenType.String)
                    list.Add(reader.GetString()!);
            }
            return list;
        }

        throw new JsonException("Expected string or array of strings");
    }

    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
    {
        if (value == null) return;

        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStringValue(item);
        }
        writer.WriteEndArray();
    }
}

public class BotsConfig
{
    [JsonConverter(typeof(BotIndexConvertor))]
    public List<string> CT { get; set; } = [];
    [JsonConverter(typeof(BotIndexConvertor))]
    public List<string> T { get; set; } = [];

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
