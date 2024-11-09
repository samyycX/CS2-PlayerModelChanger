namespace PlayerModelChanger;

using System.Text.Json;
using System.Text.Json.Serialization;
public class Model
{
    [JsonPropertyName("index")] public string Index { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("path")] public required string Path { get; set; }
    [JsonPropertyName("permissions")] public string[] Permissions { get; set; } = [];
    [JsonPropertyName("permissionsOr")] public string[] PermissionsOr { get; set; } = [];

    [JsonPropertyName("side")]
    [JsonConverter(typeof(CaseInsensitiveJsonStringEnumConverter))]
    public Side Side { get; set; } = Side.All;
    [JsonPropertyName("disableleg")] public bool Disableleg { get; set; }
    [JsonPropertyName("hideinmenu")] public bool Hideinmenu { get; set; }
    [JsonPropertyName("fixedmeshgroups")] public Dictionary<int, int> FixedMeshgroups { get; set; } = [];
    [JsonPropertyName("meshgroups")] public Dictionary<string, dynamic> Meshgroups { get; set; } = new();
}

public class CaseInsensitiveJsonStringEnumConverter : JsonConverter<Side>
{
    public override Side Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string token but got {reader.TokenType}");
        }

        string? enumString = reader.GetString()?.ToLowerInvariant();

        foreach (Side value in Enum.GetValues(typeof(Side)))
        {
            if (value.ToString().ToLowerInvariant() == enumString)
            {
                return value;
            }
        }

        throw new JsonException($"The value '{enumString}' is not a valid member of {typeof(Side).Name}");
    }

    public override void Write(Utf8JsonWriter writer, Side value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}