namespace Service;

public class Model {
    public string index { get; set; }
    public string name { get; set; }

    public required string path { get; set; }
    
    public string[] permissions { get; set; } = new string[0];
    public string[] permissionsOr { get; set; } = new string[0];
    public string side { get; set; } = "all";
    public bool disableleg { get; set; }

    public bool hideinmenu { get; set; }
}