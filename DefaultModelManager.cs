using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using Newtonsoft.Json;

namespace Service;

abstract class EntryKey {
    public string content {get;set;}

    public EntryKey(string content) {
        this.content = content;
    }

    public bool Equals(EntryKey? key)
    {
        if (!(key is EntryKey)) {
            return false;
        }
        if (key == null) {
            return false;
        }
        if (this is AllKey) {
            return true;
        }
        return content.Equals(key.content);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((EntryKey) obj);
    }
    public override int GetHashCode()
    {
        return content.GetHashCode();
    }

    public abstract bool Fits(CCSPlayerController player);
}

class SteamIDKey : EntryKey {
    public SteamIDKey(string content) : base(content) {}
    public override bool Fits(CCSPlayerController player)
    {
        return this.content == player.AuthorizedSteamID?.SteamId64.ToString();    
    }
}
class PermissionFlagKey : EntryKey {
    public PermissionFlagKey(string content) : base(content) {}
    public override bool Fits(CCSPlayerController player)
    {
        return AdminManager.PlayerHasPermissions(player, new string[]{content});   
    }
}
class PermissionGroupKey : EntryKey {
    public PermissionGroupKey(string content) : base(content) {}
    public override bool Fits(CCSPlayerController player)
    {
        return AdminManager.PlayerInGroup(player, new string[]{content});   
    }
}
class AllKey : EntryKey {
    public AllKey(): base("") {}
    public override bool Fits(CCSPlayerController player)
    {
        return true;
    }
}

class DefaultModelEntry {
    public EntryKey key;
    public DefaultModel item; // model index
    public string side;

    public DefaultModelEntry(EntryKey key, DefaultModel item, string side)
    {
        this.key = key;
        this.item = item;
        this.side = side;
    }
}

public class DefaultModel {
    public required string index;
    public bool force;
}

class ConfigDefaultModelsTemplate {
    [JsonProperty("all")] public Dictionary<string, DefaultModel>? allModels;
    [JsonProperty("t")] public Dictionary<string, DefaultModel>? tModels;
    [JsonProperty("ct")] public Dictionary<string, DefaultModel>? ctModels;
    
}
class ConfigTemplate {
    [JsonProperty("DefaultModels")] public required ConfigDefaultModelsTemplate models;
}

public class DefaultModelManager {

    private List<DefaultModelEntry> DefaultModels = new List<DefaultModelEntry>();

    public DefaultModelManager(string ModuleDirectory) {
        
        ReloadConfig(ModuleDirectory);
       
    }

    public void ReloadConfig(string ModuleDirectory) {
        var filePath = Path.Join(ModuleDirectory, "../../configs/plugins/PlayerModelChanger/DefaultModels.json");
        if (File.Exists(filePath)) {
            StreamReader reader = File.OpenText(filePath);
            string content = reader.ReadToEnd();
            ConfigTemplate config = JsonConvert.DeserializeObject<ConfigTemplate>(content)!;

            DefaultModels = ParseModelConfig(config.models);
        } else {
            Console.WriteLine("'DefaultModels.json' not found. Disabling default models feature.");
        }
    }

    private static EntryKey ParseKey(string key) {
        EntryKey entryKey;
        if (key == "*") {
            entryKey = new AllKey();
        } else if (key.StartsWith("@")) {
            entryKey = new PermissionFlagKey(key);
        } else if (key.StartsWith("#")) {
            entryKey = new PermissionGroupKey(key);
        } else {
            entryKey = new SteamIDKey(key);
        }
        return entryKey;
    }
    private static List<DefaultModelEntry> ParseModelConfig(ConfigDefaultModelsTemplate config) {
        List<DefaultModelEntry> defaultModels = new List<DefaultModelEntry>();

        if (config.allModels != null) {
            foreach (var model in config.allModels)
            {
                var key = ParseKey(model.Key);
                defaultModels.Add(new DefaultModelEntry(key, model.Value, "ct"));
                defaultModels.Add(new DefaultModelEntry(key, model.Value, "t"));
            }
        }
        if (config.tModels != null) {
            foreach (var model in config.tModels)
            {
                var key = ParseKey(model.Key);
                defaultModels.RemoveAll(entry => entry.key.Equals(key) && entry.side == "t");
                defaultModels.Add(new DefaultModelEntry(key, model.Value, "t"));
            }
        }
        if (config.ctModels != null) {
            foreach (var model in config.ctModels)
            {
                var key = ParseKey(model.Key);
                defaultModels.RemoveAll(entry => entry.key.Equals(key) && entry.side == "ct");
                defaultModels.Add(new DefaultModelEntry(key, model.Value, "ct"));
            }
        }
        return defaultModels;
    }

    // stage 0 : search steam id
    // stage 1 : search permission flag
    // stage 2 : search permission group
    // stage 3 : search all

    private DefaultModelEntry? GetPlayerDefaultModelWithPriority(List<DefaultModelEntry> entries) {
        var result = entries.Find(entry => entry.key is SteamIDKey);
        if (result != null) return result;
        result = entries.Find(entry => entry.key is PermissionFlagKey);
        if (result != null) return result;
        result = entries.Find(entry => entry.key is PermissionGroupKey);
        if (result != null) return result;
        result = entries.Find(entry => entry.key is AllKey);
        return result;
    }
    public DefaultModel? GetPlayerDefaultModel(CCSPlayerController player, string side) {;
        var filter1 = DefaultModels.Where(entry => entry.side == side && entry.key.Fits(player)).ToList();
        if (filter1 == null) {
            return null;
        }
        var result = GetPlayerDefaultModelWithPriority(filter1);
        if (result == null) {
            return null;
        }
        if (result.item == null || result.item.index == "") {
            return null;
        }
        return result.item;
    }
}