namespace Service;

public class Model {
    public string index { get; set; }
    public string name { get; set; }

    public required string path { get; set; }
    
    public string[] permissions { get; set; }
    public string[] permissionsOr { get; set; }
    public string side { get; set; }
    public bool disableleg { get; set; }

    public bool hideinmenu { get; set; }
}