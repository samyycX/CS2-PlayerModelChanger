namespace PlayerModelChanger;
using System.Text.Json.Serialization;
public class Model
{
    [JsonPropertyName("index")] public string Index { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("path")] public required string Path { get; set; }
    [JsonPropertyName("permissions")] public string[] Permissions { get; set; } = new string[0];
    [JsonPropertyName("permissionsOr")] public string[] PermissionsOr { get; set; } = new string[0];
    [JsonPropertyName("side")] public string Side { get; set; } = "all";
    [JsonPropertyName("disableleg")] public bool Disableleg { get; set; }
    [JsonPropertyName("hideinmenu")] public bool Hideinmenu { get; set; }
}
