namespace PlayerModelChanger;

public class TeamMenuData {
    public string title { get;set;}
    public Dictionary<string, string> selection { get; set; } = new();
}

public class ModelMenuData {
    public Dictionary<string, SingleModelMenuData> data { get; set; } = new();
}

public class SingleModelMenuData {
    public string title { get; set; }
    public Dictionary<string, string> specialModelSelection { get; set; } = new();
    public Dictionary<string, string> modelSelection { get; set; } = new();
}