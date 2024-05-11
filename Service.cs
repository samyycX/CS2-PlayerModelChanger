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

public class ModelService {

    private ModelConfig config;
    public IStorage storage;

    private DefaultModelManager defaultModelManager;

    private IStringLocalizer localizer;

    private ModelCacheManager cacheManager;

    public ModelService(ModelConfig Config, IStorage storage, IStringLocalizer localizer, DefaultModelManager defaultModelManager) {
        this.config = Config;
        this.storage = storage;
        this.localizer = localizer;

        this.defaultModelManager = defaultModelManager;
        cacheManager = new ModelCacheManager(storage);
        cacheManager.ResyncCache();
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

    public void ReloadConfig(string ModuleDirectory, ModelConfig config) {
        defaultModelManager.ReloadConfig(ModuleDirectory);
        this.config = config;
    }
    public void ResyncCache() {
        cacheManager.ResyncCache();
    }
    public void SetAllTModels(string tmodel, bool permissionBypass) {
        cacheManager.SetAllTModels(tmodel, permissionBypass);
        storage.SetAllTModel(tmodel, permissionBypass);
    }
    public void SetAllCTModels(string ctmodel, bool permissionBypass) {
        cacheManager.SetAllCTModels(ctmodel, permissionBypass);
        storage.SetAllCTModel(ctmodel, permissionBypass);
    }

    public void SetAllModels(string tmodel, string ctmodel, bool permissionBypass) {
        cacheManager.SetAllModels(tmodel, ctmodel, permissionBypass);
        storage.SetAllTModel(tmodel, permissionBypass).ContinueWith((_) => {  
            storage.SetAllCTModel(ctmodel, permissionBypass);
        });
    }
    public void SetPlayerModel(ulong steamid, string? modelIndex, string side, bool permissionBypass) {
        modelIndex = modelIndex != null ? modelIndex : "";
        cacheManager.SetPlayerModel(steamid, modelIndex, side, true);
        Utils.ExecuteSide(side,
            () => {
                storage.SetPlayerTModel(steamid, modelIndex, permissionBypass).ContinueWith((_) => {
                    storage.SetPlayerCTModel(steamid, modelIndex, permissionBypass);
                });
            },
            () => {
                storage.SetPlayerTModel(steamid, modelIndex, permissionBypass);
            },
            () => {
                storage.SetPlayerCTModel(steamid, modelIndex, permissionBypass);
            }
        );
    }
    
    public void SetPlayerAllModel(ulong steamid, string? tModel, string? ctModel, bool permissionBypass) {
        tModel = tModel != null ? tModel : "";
        ctModel = ctModel != null ? ctModel : "";
        cacheManager.SetPlayerModel(steamid, tModel, "t", permissionBypass);
        cacheManager.SetPlayerModel(steamid, ctModel, "ct", permissionBypass);
        storage.SetPlayerTModel(steamid, tModel, permissionBypass).ContinueWith((_) => {
            storage.SetPlayerCTModel(steamid, ctModel, permissionBypass);
        });
    }
    public void SetPlayerModel(ulong steamid, string? modelIndex, CsTeam team, bool permissionBypass) {
        var side = team == CsTeam.Terrorist ? "t" : "ct";
        SetPlayerModel(steamid, modelIndex != null ? modelIndex : "", side, permissionBypass);
    }

    public int GetModelCount() {
        return config.Models.Count();
    }
    public List<ulong> GetAllPlayers() {
        return cacheManager.GetAllPlayers();
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

    public Tuple<bool, bool> CheckAndReplaceModel(CCSPlayerController player) {
        var steamid = player.AuthorizedSteamID!.SteamId64!;
        var modelCache = cacheManager.GetPlayerModelCache(player);

        var defaultTModel = defaultModelManager.GetPlayerDefaultModel(player, "t");
        var defaultCTModel = defaultModelManager.GetPlayerDefaultModel(player, "ct");
        var tValid = CheckModel(player, "t", modelCache, defaultTModel);
        var ctValid = CheckModel(player, "ct", modelCache, defaultCTModel);
        if (!tValid && !ctValid) {
            SetPlayerAllModel(steamid, defaultTModel?.index, defaultCTModel?.index, false);
            return new Tuple<bool, bool>(true, true);
        } else if (!tValid) {
            SetPlayerModel(steamid, defaultTModel?.index, "t", false);
            return new Tuple<bool, bool>(true, false);
        } else if (!ctValid) {
            SetPlayerModel(steamid, defaultCTModel?.index, "ct", false);
            return new Tuple<bool, bool>(false, true);
        }
        return new Tuple<bool, bool>(false, false);
    }

    // side only t and ct
    public bool CheckModel(CCSPlayerController player, string side, ModelCache? modelCache, DefaultModel? defaultModel) {
        if (modelCache == null) {
            return false;
        }
        var modelIndex = side == "t" ? modelCache!.t_model : modelCache!.ct_model;
        if ((side == "t" && modelCache!.t_permission_bypass) || (side == "ct" && modelCache!.ct_permission_bypass)) {
            return true;
        }
        if (defaultModel != null && defaultModel.force) {
            if (modelIndex != defaultModel.index) {
                return false;
            }
        }
        if (modelIndex == "@random" && config.DisableRandomModel) {
            return false;
        }
        if (modelIndex == "" || modelIndex == "@random") {
            return true;
        }
        CsTeam team = side.ToLower() == "t" ? CsTeam.Terrorist : CsTeam.CounterTerrorist;

        var model = GetModel(modelIndex!);
        if (model == null) {
            return false;
        }
        return CanPlayerApplyModel(player, side, model);
    }

    public void SetPlayerModelWithCheck(CCSPlayerController player, string modelIndex, string side) {
        var isSpecial = modelIndex == "" || modelIndex == "@random";

        if (modelIndex == "@default") {
            var tDefault = defaultModelManager.GetPlayerDefaultModel(player, "t");
            var ctDefault = defaultModelManager.GetPlayerDefaultModel(player, "ct");
            Utils.ExecuteSide(side,
                () => SetPlayerAllModel(player!.AuthorizedSteamID!.SteamId64, tDefault == null ? "" : tDefault.index, ctDefault == null ? "" : ctDefault.index, false),
                () => SetPlayerModel(player!.AuthorizedSteamID!.SteamId64, tDefault == null ? "" : tDefault.index, side, false),
                () => SetPlayerModel(player!.AuthorizedSteamID!.SteamId64, ctDefault == null ? "" : ctDefault.index, side, false)
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
        SetPlayerModel(steamid, modelIndex, side, false);
        player.PrintToChat(localizer["command.model.success", localizer["side."+side]]);
        
    }
    public Model? GetPlayerModel(CCSPlayerController player, string side) {
        if (side == "all") {
          var tModel = GetPlayerModel(player, "t");
          var ctModel = GetPlayerModel(player, "ct");
          return tModel?.index == ctModel?.index ? tModel : null;
        }
        var modelCache = cacheManager.GetPlayerModelCache(player);
        var modelIndex = side == "t" ? modelCache?.t_model : modelCache?.ct_model;
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

    public Model? GetPlayerNowTeamModel(CCSPlayerController player) {
        var team = (CsTeam)player.TeamNum;
        if (team == CsTeam.Spectator || team == CsTeam.None) {
            return null;
        }
        var side = team == CsTeam.Terrorist ? "t" : "ct";
        return GetPlayerModel(player, side);
    }
    public string GetPlayerModelName(CCSPlayerController? player, CsTeam team) {
        var modelCache = cacheManager.GetPlayerModelCache(player);
        string? modelIndex = team == CsTeam.Terrorist ? modelCache?.t_model : modelCache?.ct_model;
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
