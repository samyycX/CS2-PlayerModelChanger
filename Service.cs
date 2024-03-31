using Config;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using PlayerModelChanger;
using Storage;

namespace Service;

public class ModelCache {

    public ulong steamid { get; set; }
    public string t_model { get; set; }
    public string ct_model { get; set; }
}
public class ModelService {

    private ModelConfig config;
    public IStorage storage;

    private DefaultModelManager defaultModelManager;

    private IStringLocalizer localizer;

    private List<ModelCache> cache;

    public ModelService(ModelConfig Config, IStorage storage, IStringLocalizer localizer, DefaultModelManager defaultModelManager) {
        this.config = Config;
        this.storage = storage;
        this.localizer = localizer;

        this.defaultModelManager = defaultModelManager;
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
        if (model.permissions == null) {
            model.permissions = new string[0];
        }
        if (model.permissionsOr == null) {
            model.permissionsOr = new string[0];
        }
    }

    public void ResyncCache() {
        cache = storage.GetAllPlayerModel();
    }
    public void SetAllTModels(string tmodel) {
        cache.ForEach(model => model.t_model = tmodel);
        storage.SetAllTModel(tmodel);
    }
    public void SetAllCTModels(string ctmodel) {
        cache.ForEach(model => model.ct_model = ctmodel);
        storage.SetAllCTModel(ctmodel);
    }
    public void SetAllModels(string tmodel, string ctmodel) {
        cache.ForEach(model => {model.t_model = tmodel; model.ct_model = ctmodel;});
        storage.SetAllModel(tmodel, ctmodel);
    }
    public int GetModelCount() {
        return config.Models.Count();
    }
    public List<ulong> GetAllPlayers() {
        return cache.Select(model => model.steamid).ToList();
    }
    public List<Model> GetAllModels() {
        return config.Models.Values.ToList();
    }
    public bool ExistModel(string modelIndex) {
        return config.Models.ContainsKey(modelIndex);
    }
    public Model? FindModel(string modelName) {
        return config.Models.Values.FirstOrDefault(model => model?.name == modelName, null);
    }
    public Model? GetModel(string modelIndex) {
        Model? value;
        if (!config.Models.TryGetValue(modelIndex, out value)) {
            return null;
        } else {
            return value;
        }
        
    }

    public bool CanPlayerApplyModel(CCSPlayerController player, string side, Model model) {
        return Utils.PlayerHasPermission(player, model.permissions, model.permissionsOr) && // permission
            (model.side.ToUpper() == "ALL" || model.side.ToUpper() == side.ToUpper()); // side
    }

    public List<Model> GetAllAppliableModels(CCSPlayerController player, string side) {
        return config.Models.Values.Where(model => CanPlayerApplyModel(player, side, model)).ToList();
    }

    private void PutInCache(ulong steamid, string modelIndex, string side) {
        var obj = cache.Find(model => model.steamid == steamid);

        if (obj == null) {
            var modelcache = new ModelCache();
            modelcache.steamid = steamid;
            cache.Add(modelcache);
            obj = modelcache;
        }
        Utils.ExecuteSide(side,
            null,
            () => obj.t_model = modelIndex,
            () => obj.ct_model = modelIndex
        );
       
    }

    public Tuple<bool, bool> CheckAndReplaceModel(CCSPlayerController player) {
        var steamid = player.AuthorizedSteamID!.SteamId64!;
        var tModel = GetPlayerModelIndex(player, "t");
        var ctModel = GetPlayerModelIndex(player, "ct");

        var defaultTModel = defaultModelManager.GetPlayerDefaultModel(player, "t");
        var defaultCTModel = defaultModelManager.GetPlayerDefaultModel(player, "ct");
        var tValid = CheckModel(player, "t", tModel, defaultTModel);
        var ctValid = CheckModel(player, "ct", ctModel, defaultCTModel);
        
        if (!tValid && !ctValid) {
            AdminSetPlayerAllModel(steamid, defaultTModel?.index, defaultCTModel?.index);
            return new Tuple<bool, bool>(true, true);
        } else if (!tValid) {
            AdminSetPlayerModel(steamid, defaultTModel?.index, "t");
            return new Tuple<bool, bool>(true, false);
        } else if (!ctValid) {
            AdminSetPlayerModel(steamid, defaultCTModel?.index, "ct");
            return new Tuple<bool, bool>(false, true);
        }
        return new Tuple<bool, bool>(false, false);
    }

    // pass in defaultModel to reduce search times
    public bool CheckModel(CCSPlayerController player, string side, string? modelIndex, DefaultModel? defaultModel) {
        if (modelIndex == null) {
            return false;
        }
        if (defaultModel != null && defaultModel.force) {
            if (modelIndex != defaultModel.index) {
                return false;
            }
        }
        if (modelIndex == "" || modelIndex == "@random") {
            return true;
        }
        CsTeam team = side.ToLower() == "t" ? CsTeam.Terrorist : CsTeam.CounterTerrorist;

        var model = GetModel(modelIndex);
        if (model == null) {
            return false;
        }
        return CanPlayerApplyModel(player, side, model);
    }

    public void AdminSetPlayerModel(ulong steamid, string? modelIndex, string side) {
        modelIndex = modelIndex != null ? modelIndex : "";
        PutInCache(steamid, modelIndex, side);
        Utils.ExecuteSide(side,
            () => {
                storage.SetPlayerAllModel(steamid, modelIndex, modelIndex);
            },
            () => {
                storage.SetPlayerTModel(steamid, modelIndex);
            },
            () => {
                storage.SetPlayerCTModel(steamid, modelIndex);
            }
        );
    }
    // an ugly implementation
    public void AdminSetPlayerAllModel(ulong steamid, string? tModel, string? ctModel) {
        tModel = tModel != null ? tModel : "";
        ctModel = ctModel != null ? ctModel : "";
        PutInCache(steamid, tModel, "t");
        PutInCache(steamid, ctModel, "ct");
        storage.SetPlayerAllModel(steamid, tModel , ctModel);
    }
    public void AdminSetPlayerModel(ulong steamid, string? modelIndex, CsTeam team) {
        var side = team == CsTeam.Terrorist ? "t" : "ct";
        AdminSetPlayerModel(steamid, modelIndex != null ? modelIndex : "", side);
    }

    public void SetPlayerModel(CCSPlayerController player, string modelIndex, string side) {
        var isSpecial = modelIndex == "" || modelIndex == "@random";

        if (modelIndex == "@default") {
            var tDefault = defaultModelManager.GetPlayerDefaultModel(player, "t");
            var ctDefault = defaultModelManager.GetPlayerDefaultModel(player, "ct");
            Utils.ExecuteSide(side,
                () => AdminSetPlayerAllModel(player!.AuthorizedSteamID!.SteamId64, tDefault == null ? "" : tDefault.index, ctDefault == null ? "" : ctDefault.index),
                () => AdminSetPlayerModel(player!.AuthorizedSteamID!.SteamId64, tDefault == null ? "" : tDefault.index, side),
                () => AdminSetPlayerModel(player!.AuthorizedSteamID!.SteamId64, ctDefault == null ? "" : ctDefault.index, side)
            );
            player.PrintToChat(localizer["command.model.success", localizer["side."+side]]);
            return;
        }

        var model = GetModel(modelIndex);
        if (!isSpecial) {
            if (model == null) {
                player.PrintToChat(localizer["command.model.notfound", modelIndex]);
                return;
            }

            if (!Utils.PlayerHasPermission(player, model.permissions, model.permissionsOr)) {
                player.PrintToChat(localizer["model.nopermission", modelIndex]);
                return;
            }

            if (!GetAllAppliableModels(player, side).Contains(model)) {
                player.PrintToChat(localizer["model.wrongteam", modelIndex]);
                return;
            }

        }
       
        var steamid = player!.AuthorizedSteamID!.SteamId64;
        AdminSetPlayerModel(steamid, modelIndex, side);
        player.PrintToChat(localizer["command.model.success", localizer["side."+side]]);
        
    }
    public Model? GetPlayerModel(CCSPlayerController player, string side) {
        var modelIndex = GetPlayerModelIndex(player, side);
        if (modelIndex == null || modelIndex == "") {
            return null;
        }
        if (modelIndex == "@random") {
            var models = GetAllAppliableModels(player, side);
            if (models.Count() == 0) {
                return null;
            }
            var index = Random.Shared.Next(models.Count());
            return models[index];
        }
        return GetModel(modelIndex);
    }
    public string? GetPlayerModelIndex(CCSPlayerController player, string side) {
        var modelIndex = "";
        side = side.ToLower();
        if (side == "t") {
            modelIndex = cache.Find(model => model.steamid == player!.AuthorizedSteamID!.SteamId64)?.t_model;
        } else {
            modelIndex = cache.Find(model => model.steamid == player!.AuthorizedSteamID!.SteamId64)?.ct_model;
        }
        return modelIndex;
    }
    public Model? GetPlayerNowTeamModel(CCSPlayerController player) {
        var team = (CsTeam)player.TeamNum;
        if (team == CsTeam.Spectator || team == CsTeam.None) {
            return null;
        }
        var side = team == CsTeam.Terrorist ? "t" : "ct";
        return GetPlayerModel(player, side);
    }
    public string GetPlayerModelName(CCSPlayerController? player, CsTeam team) {
        
        var modelIndex = "";
        if (team == CsTeam.Terrorist) {
            modelIndex = cache.Find(model => model.steamid == player!.AuthorizedSteamID!.SteamId64)?.t_model;
        } else {
            modelIndex = cache.Find(model => model.steamid == player!.AuthorizedSteamID!.SteamId64)?.ct_model;
        }
        if (modelIndex == null || modelIndex == "") {
            return localizer["model.none"];
        }
        if (modelIndex == "@random") {
            return localizer["modelmenu.random"];
        } else {
            var model = GetModel(modelIndex);
            if (model == null) {
                return localizer["model.none"];
            } 
            return model.name;
        }
    }
}