using Config;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using Storage;

namespace Service;

public class ModelCache {

    public ulong steamid { get; set; }
    public string t_model { get; set; }
    public string ct_model { get; set; }
}
public class ModelService {

    private ModelConfig config;
    private IStorage storage;

    private IStringLocalizer localizer;

    private List<ModelCache> cache;

    public ModelService(ModelConfig Config, IStorage storage, IStringLocalizer localizer) {
        this.config = Config;
        this.storage = storage;
        this.localizer = localizer;

        cache = storage.GetAllPlayerModel();
    }

    public static void InitializeModel(string key, Model model) {
        model.index = key;
        if (model.name == null) {
            model.name = model.index;
        }
        if (model.side == null) {
            model.side = "ALL";
        }
    }

    public void ResyncCache() {
        cache = storage.GetAllPlayerModel();
    }

    public int GetModelCount() {
        return config.Models.Count();
    }
    public List<Model> GetAllModels() {
        return config.Models.Values.ToList();
    }
    public List<Model> GetAllSideAppliableModels() {
        return config.Models.Values.Where(model => model.side == "ALL").ToList();
    }
    public bool ExistModel(string modelIndex) {
        return config.Models.ContainsKey(modelIndex);
    }
    public Model? GetModel(string modelIndex) {
        return config.Models.GetValueOrDefault(modelIndex, null);
    }
    public List<Model> GetAllAppliableModels(CsTeam team) {
        return config.Models.Values.Where(model => model.side == "ALL" || model.side == (team == CsTeam.Terrorist ? "T" : "CT")).ToList();
    }

    private void PutInCache(ulong steamid, string modelIndex, CsTeam team) {
        var obj = cache.Find(model => model.steamid == steamid);

        if (obj == null) {
            var modelcache = new ModelCache();
            modelcache.steamid = steamid;
            cache.Add(modelcache);
            obj = modelcache;
        }
        if (team == CsTeam.Terrorist) {
            obj.t_model = modelIndex;
        } else {
            obj.ct_model = modelIndex;
        }
    }

    public void SetPlayerModel(CCSPlayerController? player, string modelIndex, CsTeam team) {
        var isSpecial = modelIndex == "" || modelIndex == "@random";
        
        var model = GetModel(modelIndex);
        if (model == null && !isSpecial) {
            player.PrintToChat(localizer["command.model.notfound", modelIndex]);
            return;
        }
        if (isSpecial || GetAllAppliableModels(team).Contains(model)) {
            var steamid = player!.AuthorizedSteamID!.SteamId64;
            PutInCache(steamid, modelIndex, team);
            if (team == CsTeam.Terrorist) { 
                storage.SetPlayerTModel(steamid, modelIndex);
            } else {
                storage.SetPlayerCTModel(steamid, modelIndex);
            }
            
        } else {
            player.PrintToChat(localizer["model.wrongteam", modelIndex]);
            return;
        }

        
        player.PrintToChat(localizer["command.model.success"]);
        
    }
    public void SetPlayerTModel(CCSPlayerController? player, string modelIndex) {
        SetPlayerModel(player, modelIndex, CsTeam.Terrorist);
    }
    public void SetPlayerCTModel(CCSPlayerController? player, string modelIndex) {
        SetPlayerModel(player, modelIndex, CsTeam.CounterTerrorist);
    }
    public Model? GetPlayerModel(CCSPlayerController? player) {
        var team = (CsTeam)player.TeamNum;
        var modelIndex = "";
        if (team == CsTeam.Terrorist) {
            modelIndex = cache.Find(model => model.steamid == player!.AuthorizedSteamID!.SteamId64)?.t_model;
            // modelIndex = storage.GetPlayerTModel(player!.AuthorizedSteamID!.SteamId64);
        } else {
            modelIndex = cache.Find(model => model.steamid == player!.AuthorizedSteamID!.SteamId64)?.ct_model;
            // modelIndex = storage.GetPlayerCTModel(player!.AuthorizedSteamID!.SteamId64);
        }
        if (modelIndex == null || modelIndex == "") {
            return null;
        }
        if (modelIndex == "@random") {
            var models = GetAllAppliableModels(team);
            var index = Random.Shared.Next(models.Count());
            return models[index];
        }
        return GetModel(modelIndex);
    }
    public string GetPlayerModelName(CCSPlayerController? player, CsTeam team) {
        
        var modelIndex = "";
        if (team == CsTeam.Terrorist) {
            modelIndex = cache.Find(model => model.steamid == player!.AuthorizedSteamID!.SteamId64)?.t_model;
            // modelIndex = storage.GetPlayerTModel(player!.AuthorizedSteamID!.SteamId64);
        } else {
            modelIndex = cache.Find(model => model.steamid == player!.AuthorizedSteamID!.SteamId64)?.ct_model;
            // modelIndex = storage.GetPlayerCTModel(player!.AuthorizedSteamID!.SteamId64);
        }
        if (modelIndex == null || modelIndex == "") {
            return "";
        }
        if (modelIndex == "@random") {
            return localizer["modelmenu.random"];
        } else {
            var model = GetModel(modelIndex);
            if (model == null) {
                return "";
            } 
            return model.name;
        }
    }
}