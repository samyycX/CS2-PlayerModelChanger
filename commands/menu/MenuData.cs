namespace PlayerModelChanger;

public class TeamMenuData
{
    public string Title { get; set; } = "";
    public Dictionary<string, string> Selections { get; set; } = new();
}

public class ModelMenuData
{
    public Dictionary<string, SingleModelMenuData> Data { get; set; } = new();
}

public class SingleModelMenuData
{
    public string Title { get; set; } = "";
    public Dictionary<string, string> SpecialModelSelections { get; set; } = new();
    public Dictionary<string, string> ModelSelections { get; set; } = new();
}
