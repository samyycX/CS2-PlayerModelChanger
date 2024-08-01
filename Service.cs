using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;

namespace PlayerModelChanger;

public class ModelService
{

    private ModelConfig _Config;
    public IStorage _Storage;

    private DefaultModelManager _DefaultModelManager;

    private IStringLocalizer _Localizer;

    private ModelCacheManager _CacheManager;


    private Dictionary<ulong, long> _ModelChangeCooldown = new Dictionary<ulong, long>();

    public ModelService(ModelConfig Config, IStorage storage, IStringLocalizer localizer, DefaultModelManager defaultModelManager)
    {
        this._Config = Config;
        this._Storage = storage;
        this._Localizer = localizer;

        this._DefaultModelManager = defaultModelManager;
        _CacheManager = new ModelCacheManager(storage);
        _CacheManager.ResyncCache();
    }

    public static void InitializeModel(string key, Model model)
    {
        model.Index = key;
        if (model.Name == null)
        {
            model.Name = model.Index;
        }
        if (model.Side == null)
        {
            model.Side = "ALL";
        }
        if (model.Permissions == null)
        {
            model.Permissions = new string[0];
        }
        if (model.PermissionsOr == null)
        {
            model.PermissionsOr = new string[0];
        }
    }

    public void ReloadConfig(string ModuleDirectory, ModelConfig config)
    {
        this._Config = config;
        _DefaultModelManager.ReloadConfig(ModuleDirectory, this);
    }
    public void ResyncCache()
    {
        _CacheManager.ResyncCache();
    }
    public void SetAllTModels(string tmodel, bool permissionBypass)
    {
        _CacheManager.SetAllTModels(tmodel, permissionBypass);
        _Storage.SetAllTModel(tmodel, permissionBypass);
    }
    public void SetAllCTModels(string ctmodel, bool permissionBypass)
    {
        _CacheManager.SetAllCTModels(ctmodel, permissionBypass);
        _Storage.SetAllCTModel(ctmodel, permissionBypass);
    }

    public void SetAllModels(string tmodel, string ctmodel, bool permissionBypass)
    {
        _CacheManager.SetAllModels(tmodel, ctmodel, permissionBypass);
        _Storage.SetAllTModel(tmodel, permissionBypass).ContinueWith((_) =>
        {
            _Storage.SetAllCTModel(ctmodel, permissionBypass);
        });
    }
    public void SetPlayerModel(ulong steamid, string? modelIndex, string side, bool permissionBypass)
    {
        modelIndex = modelIndex != null ? modelIndex : "";
        _CacheManager.SetPlayerModel(steamid, modelIndex, side, true);
        Utils.ExecuteSide(side,
            () =>
            {
                _Storage.SetPlayerTModel(steamid, modelIndex, permissionBypass).ContinueWith((_) =>
                {
                    _Storage.SetPlayerCTModel(steamid, modelIndex, permissionBypass);
                });
            },
            () =>
            {
                _Storage.SetPlayerTModel(steamid, modelIndex, permissionBypass);
            },
            () =>
            {
                _Storage.SetPlayerCTModel(steamid, modelIndex, permissionBypass);
            }
        );
        if (!_Config.DisableInstantChange)
        {
            var player = Utilities.GetPlayerFromSteamId(steamid);
            if (player == null) { return; }
            if (Utils.CanPlayerSetModelInstantly(player, side))
            {
                Utils.RespawnPlayer(player, _Config.Inspection.Enable || modelIndex == "@random");
            }
        }
    }

    public void SetPlayerAllModel(ulong steamid, string? tModel, string? ctModel, bool permissionBypass)
    {
        tModel = tModel != null ? tModel : "";
        ctModel = ctModel != null ? ctModel : "";
        _CacheManager.SetPlayerModel(steamid, tModel, "t", permissionBypass);
        _CacheManager.SetPlayerModel(steamid, ctModel, "ct", permissionBypass);
        _Storage.SetPlayerTModel(steamid, tModel, permissionBypass).ContinueWith((_) =>
        {
            _Storage.SetPlayerCTModel(steamid, ctModel, permissionBypass);
        });
        if (!_Config.DisableInstantChange)
        {
            var player = Utilities.GetPlayerFromSteamId(steamid);
            if (player == null) { return; }
            if (Utils.CanPlayerSetModelInstantly(player, "all"))
            {
                var index = GetModel(player.Team == CsTeam.Terrorist ? tModel : ctModel)?.Index;
                Utils.RespawnPlayer(player, _Config.Inspection.Enable || index == "@random");
            }
        }
    }
    public void SetPlayerModel(ulong steamid, string? modelIndex, CsTeam team, bool permissionBypass)
    {
        var side = team == CsTeam.Terrorist ? "t" : "ct";
        SetPlayerModel(steamid, modelIndex != null ? modelIndex : "", side, permissionBypass);
    }

    public int GetModelCount()
    {
        return _Config.Models.Count();
    }
    public List<ulong> GetAllPlayers()
    {
        return _CacheManager.GetAllPlayers();
    }
    public List<Model> GetAllModels()
    {
        return _Config.Models.Values.ToList();
    }
    public bool ExistModel(string modelIndex)
    {
        return _Config.Models.ContainsKey(modelIndex);
    }
    public Model? FindModel(string modelName)
    {
        return _Config.Models.Values.FirstOrDefault(model => model?.Name == modelName, null);
    }
    public Model? GetModel(string modelIndex)
    {
        Model? value;
        if (!_Config.Models.TryGetValue(modelIndex, out value))
        {
            return null;
        }
        else
        {
            return value;
        }

    }

    public bool CanPlayerApplyModel(CCSPlayerController player, string side, Model model)
    {
        return Utils.PlayerHasPermission(player, model.Permissions, model.PermissionsOr) && // permission
            (model.Side.ToUpper() == "ALL" || model.Side.ToUpper() == side.ToUpper()); // side
    }

    public List<Model> GetAllAppliableModels(CCSPlayerController player, string side)
    {
        return _Config.Models.Values.Where(model => CanPlayerApplyModel(player, side, model)).ToList();
    }

    public Tuple<bool, bool> CheckAndReplaceModel(CCSPlayerController player)
    {
        var steamid = player.AuthorizedSteamID!.SteamId64!;
        var modelCache = _CacheManager.GetPlayerModelCache(player);

        var defaultTModel = _DefaultModelManager.GetPlayerDefaultModel(player, "t");
        var defaultCTModel = _DefaultModelManager.GetPlayerDefaultModel(player, "ct");
        var tValid = CheckModel(player, "t", modelCache, defaultTModel);
        var ctValid = CheckModel(player, "ct", modelCache, defaultCTModel);
        if (!tValid && !ctValid)
        {
            SetPlayerAllModel(steamid, defaultTModel?.index, defaultCTModel?.index, false);
            return new Tuple<bool, bool>(true, true);
        }
        else if (!tValid)
        {
            SetPlayerModel(steamid, defaultTModel?.index, "t", false);
            return new Tuple<bool, bool>(true, false);
        }
        else if (!ctValid)
        {
            SetPlayerModel(steamid, defaultCTModel?.index, "ct", false);
            return new Tuple<bool, bool>(false, true);
        }
        return new Tuple<bool, bool>(false, false);
    }

    // side only t and ct
    public bool CheckModel(CCSPlayerController player, string side, ModelCache? modelCache, DefaultModel? defaultModel)
    {
        if (modelCache == null)
        {
            return false;
        }
        var modelIndex = side == "t" ? modelCache!.t_model : modelCache!.ct_model;
        if ((side == "t" && modelCache!.t_permission_bypass) || (side == "ct" && modelCache!.ct_permission_bypass))
        {
            return true;
        }
        if (defaultModel != null && defaultModel.force)
        {
            if (modelIndex != defaultModel.index)
            {
                return false;
            }
        }
        if (modelIndex == "@random" && _Config.DisableRandomModel)
        {
            return false;
        }
        if (modelIndex == "" || modelIndex == "@random")
        {
            return true;
        }

        var model = GetModel(modelIndex!);
        if (model == null)
        {
            return false;
        }
        return CanPlayerApplyModel(player, side, model);
    }

    public void SetPlayerModelWithCheck(CCSPlayerController player, string modelIndex, string side)
    {
        if (_ModelChangeCooldown.ContainsKey(player.SteamID))
        {
            var lastTime = _ModelChangeCooldown[player.SteamID];
            if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastTime < (_Config.ModelChangeCooldownSecond * 1000))
            {
                player.PrintToChat(_Localizer["command.model.cooldown"]);
                return;
            }
        }

        var isSpecial = modelIndex == "" || modelIndex == "@random";

        if (modelIndex == "@default")
        {
            var tDefault = _DefaultModelManager.GetPlayerDefaultModel(player, "t");
            var ctDefault = _DefaultModelManager.GetPlayerDefaultModel(player, "ct");
            Utils.ExecuteSide(side,
                () => SetPlayerAllModel(player!.AuthorizedSteamID!.SteamId64, tDefault == null ? "" : tDefault.index, ctDefault == null ? "" : ctDefault.index, false),
                () => SetPlayerModel(player!.AuthorizedSteamID!.SteamId64, tDefault == null ? "" : tDefault.index, side, false),
                () => SetPlayerModel(player!.AuthorizedSteamID!.SteamId64, ctDefault == null ? "" : ctDefault.index, side, false)
            );
            if (_Config.DisableInstantChange || !Utils.CanPlayerSetModelInstantly(player, side))
            {
                player.PrintToChat(_Localizer["command.model.success", _Localizer["side." + side]]);
            }
            return;
        }

        var model = GetModel(modelIndex);
        if (!isSpecial)
        {
            if (model == null)
            {
                player.PrintToChat(_Localizer["command.model.notfound", modelIndex]);
                return;
            }

            if (!Utils.PlayerHasPermission(player, model.Permissions, model.PermissionsOr))
            {
                player.PrintToChat(_Localizer["model.nopermission", modelIndex]);
                return;
            }

            if (!GetAllAppliableModels(player, side).Contains(model))
            {
                player.PrintToChat(_Localizer["model.wrongteam", modelIndex]);
                return;
            }

        }

        var steamid = player!.AuthorizedSteamID!.SteamId64;
        SetPlayerModel(steamid, modelIndex, side, false);
        _ModelChangeCooldown[steamid] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (_Config.DisableInstantChange || !Utils.CanPlayerSetModelInstantly(player, side))
        {
            player.PrintToChat(_Localizer["command.model.success", _Localizer["side." + side]]);
        }

    }
    public Model? GetPlayerModel(CCSPlayerController player, string side)
    {
        if (side == "all")
        {
            var tModel = GetPlayerModel(player, "t");
            var ctModel = GetPlayerModel(player, "ct");
            return tModel?.Index == ctModel?.Index ? tModel : null;
        }
        var modelCache = _CacheManager.GetPlayerModelCache(player);
        var modelIndex = side == "t" ? modelCache?.t_model : modelCache?.ct_model;
        if (modelIndex == null || modelIndex == "")
        {
            return null;
        }
        if (modelIndex == "@random")
        {
            var models = GetAllAppliableModels(player, side);
            if (models.Count() == 0)
            {
                return null;
            }
            var index = Random.Shared.Next(models.Count());
            return models[index];
        }
        return GetModel(modelIndex);
    }

    public Model? GetPlayerNowTeamModel(CCSPlayerController player)
    {
        var team = (CsTeam)player.TeamNum;
        if (team == CsTeam.Spectator || team == CsTeam.None)
        {
            return null;
        }
        var side = team == CsTeam.Terrorist ? "t" : "ct";
        return GetPlayerModel(player, side);
    }
    public string GetPlayerModelName(CCSPlayerController player, CsTeam team)
    {
        var modelCache = _CacheManager.GetPlayerModelCache(player);
        string? modelIndex = team == CsTeam.Terrorist ? modelCache?.t_model : modelCache?.ct_model;
        if (modelIndex == null || modelIndex == "")
        {
            return _Localizer["model.none"];
        }
        if (modelIndex == "@random")
        {
            return _Localizer["modelmenu.random"];
        }
        else
        {
            var model = GetModel(modelIndex);
            if (model == null)
            {
                return _Localizer["model.none"];
            }
            return model.Name;
        }
    }
}
