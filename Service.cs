using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;

namespace PlayerModelChanger;

struct MapDefaultModel
{
    public string? T { get; set; }
    public string? CT { get; set; }
}

public class ModelService
{

    private ModelConfig _Config;
    public IStorage _Storage;

    private DefaultModelManager _DefaultModelManager;

    private IStringLocalizer _Localizer;

    private ModelCacheManager _CacheManager;

    private Dictionary<ulong, MapDefaultModel> _MapDefaultModels = new Dictionary<ulong, MapDefaultModel>();


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
        if (model.Name == "")
        {
            model.Name = model.Index;
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
        _Storage.SetAllTModel(tmodel, permissionBypass);
        _Storage.SetAllCTModel(ctmodel, permissionBypass);
    }
    public void SetPlayerModel(ulong steamid, string? modelIndex, Side side, bool permissionBypass)
    {
        modelIndex = modelIndex != null ? modelIndex : "";
        _CacheManager.SetPlayerModel(steamid, modelIndex, side, true);
        Utils.ExecuteSide(side,
            () =>
            {
                _Storage.SetPlayerTModel(steamid, modelIndex, permissionBypass);
                _Storage.SetPlayerCTModel(steamid, modelIndex, permissionBypass);
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
                var model = GetPlayerModel(player, side);
                Utils.InstantUpdatePlayer(player, model, _Config.Inspection.Enable && modelIndex != "@random");
            }
        }
    }

    public void SetPlayerAllModel(ulong steamid, string? tModel, string? ctModel, bool permissionBypass, bool inspection = true)
    {
        tModel = tModel != null ? tModel : "";
        ctModel = ctModel != null ? ctModel : "";
        _CacheManager.SetPlayerModel(steamid, tModel, Side.T, permissionBypass);
        _CacheManager.SetPlayerModel(steamid, ctModel, Side.CT, permissionBypass);
        _Storage.SetPlayerTModel(steamid, tModel, permissionBypass);
        _Storage.SetPlayerCTModel(steamid, ctModel, permissionBypass);
        if (!_Config.DisableInstantChange)
        {
            var player = Utilities.GetPlayerFromSteamId(steamid);
            if (player == null) { return; }
            if (Utils.CanPlayerSetModelInstantly(player, Side.All))
            {
                var model = GetModel(player.Team == CsTeam.Terrorist ? tModel : ctModel);
                Utils.InstantUpdatePlayer(player, model, inspection && _Config.Inspection.Enable && model?.Index != "@random");
            }
        }
    }
    public void SetPlayerModel(ulong steamid, string? modelIndex, CsTeam team, bool permissionBypass)
    {
        var side = team == CsTeam.Terrorist ? Side.T : Side.CT;
        SetPlayerModel(steamid, modelIndex != null ? modelIndex : "", side, permissionBypass);
    }

    public int GetModelCount() => _Config.Models.Count;
    public List<ulong> GetAllPlayers() => _CacheManager.GetAllPlayers();
    public List<Model> GetAllModels() => _Config.Models.Values.ToList();
    public bool ExistModel(string modelIndex) => _Config.Models.ContainsKey(modelIndex);
    public Model? FindModel(string modelName) => _Config.Models.Values.FirstOrDefault(model => model?.Name == modelName, null);
    public Model? GetModel(string modelIndex) => _Config.Models.TryGetValue(modelIndex, out var value) ? value : null;

    public bool CanPlayerApplyModel(CCSPlayerController player, Side side, Model model)
    {
        return Utils.PlayerHasPermission(player, model.Permissions, model.PermissionsOr) && // permission
            (model.Side == Side.All || model.Side == side); // side
    }

    public List<Model> GetAllAppliableModels(CCSPlayerController player, Side side)
    {
        return _Config.Models.Values.Where(model => CanPlayerApplyModel(player, side, model)).ToList();
    }

    public Tuple<bool, bool> CheckAndReplaceModel(CCSPlayerController player)
    {
        var steamid = player.AuthorizedSteamID!.SteamId64!;
        var modelCache = _CacheManager.GetPlayerModelCache(player);
        var defaultTModel = _DefaultModelManager.GetPlayerDefaultModel(player, Side.T);
        var defaultCTModel = _DefaultModelManager.GetPlayerDefaultModel(player, Side.CT);
        if (modelCache == null) // player first time join
        {
            SetPlayerAllModel(steamid, defaultTModel?.index, defaultCTModel?.index, false, false);
            return new Tuple<bool, bool>(false, false);
        }
        var tValid = CheckModel(player, Side.T, modelCache, defaultTModel);
        var ctValid = CheckModel(player, Side.CT, modelCache, defaultCTModel);
        if (!tValid && !ctValid)
        {
            SetPlayerAllModel(steamid, defaultTModel?.index, defaultCTModel?.index, false);
            return new Tuple<bool, bool>(true, true);
        }
        else if (!tValid)
        {
            SetPlayerModel(steamid, defaultTModel?.index, Side.T, false);
            return new Tuple<bool, bool>(true, false);
        }
        else if (!ctValid)
        {
            SetPlayerModel(steamid, defaultCTModel?.index, Side.CT, false);
            return new Tuple<bool, bool>(false, true);
        }
        return new Tuple<bool, bool>(false, false);
    }

    // side only t and ct
    public bool CheckModel(CCSPlayerController player, Side side, ModelCache? modelCache, DefaultModel? defaultModel)
    {
        if (modelCache == null)
        {
            return false;
        }
        var modelIndex = side == Side.T ? modelCache!.t_model : modelCache!.ct_model;
        if ((side == Side.T && modelCache!.t_permission_bypass) || (side == Side.CT && modelCache!.ct_permission_bypass))
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

    public bool SetPlayerModelWithCheck(CCSPlayerController player, string modelIndex, Side side)
    {
        if (_ModelChangeCooldown.ContainsKey(player.SteamID))
        {
            var lastTime = _ModelChangeCooldown[player.SteamID];
            if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastTime < (_Config.ModelChangeCooldownSecond * 1000))
            {
                player.PrintToChat(_Localizer["command.model.cooldown"]);
                return false;
            }
        }

        var isSpecial = modelIndex == "" || modelIndex == "@random";

        if (modelIndex == "@default")
        {
            var tDefault = _DefaultModelManager.GetPlayerDefaultModel(player, Side.T);
            var ctDefault = _DefaultModelManager.GetPlayerDefaultModel(player, Side.CT);
            Utils.ExecuteSide(side,
                () => SetPlayerAllModel(player!.AuthorizedSteamID!.SteamId64, tDefault == null ? "" : tDefault.index, ctDefault == null ? "" : ctDefault.index, false),
                () => SetPlayerModel(player!.AuthorizedSteamID!.SteamId64, tDefault == null ? "" : tDefault.index, side, false),
                () => SetPlayerModel(player!.AuthorizedSteamID!.SteamId64, ctDefault == null ? "" : ctDefault.index, side, false)
            );
            if (_Config.DisableInstantChange || !Utils.CanPlayerSetModelInstantly(player, side))
            {
                player.PrintToChat(_Localizer["command.model.success", _Localizer["side." + side.ToName()]]);
            }
            return true;
        }

        var model = GetModel(modelIndex);
        if (!isSpecial)
        {
            if (model == null)
            {
                player.PrintToChat(_Localizer["command.model.notfound", modelIndex]);
                return false;
            }

            if (!Utils.PlayerHasPermission(player, model.Permissions, model.PermissionsOr))
            {
                player.PrintToChat(_Localizer["model.nopermission", modelIndex]);
                return false;
            }

            if (!GetAllAppliableModels(player, side).Contains(model))
            {
                player.PrintToChat(_Localizer["model.wrongteam", modelIndex]);
                return false;
            }

        }

        var steamid = player!.AuthorizedSteamID!.SteamId64;
        SetPlayerModel(steamid, modelIndex, side, false);
        _ModelChangeCooldown[steamid] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (_Config.DisableInstantChange || !Utils.CanPlayerSetModelInstantly(player, side))
        {
            player.PrintToChat(_Localizer["command.model.success", _Localizer["side." + side.ToName()]]);
        }
        return true;

    }
    public Model? GetPlayerModel(CCSPlayerController player, Side side)
    {
        if (side == Side.All)
        {
            var tModel = GetPlayerModel(player, Side.T);
            var ctModel = GetPlayerModel(player, Side.CT);
            return tModel?.Index == ctModel?.Index ? tModel : null;
        }
        var modelCache = _CacheManager.GetPlayerModelCache(player);
        var modelIndex = side == Side.T ? modelCache?.t_model : modelCache?.ct_model;
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
        var side = team == CsTeam.Terrorist ? Side.T : Side.CT;
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

    public List<int> GetMeshgroupPreference(CCSPlayerController player, Model model)
    {
        return _CacheManager.GetMeshgroupPreference(player.AuthorizedSteamID!.SteamId64, model.Index);
    }

    public void AddMeshgroupPreference(CCSPlayerController player, Model model, int meshgroup, bool update = true)

    {

        _CacheManager.AddMeshgroupPreference(player.AuthorizedSteamID!.SteamId64, model.Index, meshgroup);
        _Storage.AddMeshgroupPreference(player.AuthorizedSteamID!.SteamId64, model.Index, meshgroup);
        if (update) MeshgroupUpdate(player);
    }

    public void SetMeshgroupPreference(CCSPlayerController player, Model model, List<int> meshgroups)
    {
        var old = new List<int>(GetMeshgroupPreference(player, model));
        old.Sort();
        old.RemoveAt(old.Count - 1); // preserve the 'autodefault' meshgroup
        foreach (var meshgroup in old)
        {
            RemoveMeshgroupPreference(player, model, meshgroup, false);
        }

        foreach (var meshgroup in meshgroups)
        {
            AddMeshgroupPreference(player, model, meshgroup, false);
        }
        MeshgroupUpdate(player);
    }

    public void RemoveMeshgroupPreference(CCSPlayerController player, Model model, int meshgroup, bool update = true)
    {
        _CacheManager.RemoveMeshgroupPreference(player.AuthorizedSteamID!.SteamId64, model.Index, meshgroup);
        _Storage.RemoveMeshgroupPreference(player.AuthorizedSteamID!.SteamId64, model.Index, meshgroup);
        if (update) MeshgroupUpdate(player);
    }

    public bool HasMeshgroupPreference(CCSPlayerController player, Model model, int meshgroup)
    {
        return GetMeshgroupPreference(player, model).Contains(meshgroup);
    }

    public bool InitMeshgroupPreference(CCSPlayerController player, Model model, ulong meshgroupmask)
    {
        if (model.Meshgroups.Count == 0)
        {
            return true;
        }
        if (GetMeshgroupPreference(player, model).Count > 0)
        {
            return false;
        }
        var meshgroups = Convert.ToString((long)meshgroupmask, 2).ToList().Select((b, i) => b == '1' ? i : -1).Where(i => i != -1).ToList();
        var steamid = player.AuthorizedSteamID!.SteamId64;
        foreach (var meshgroup in meshgroups)
        {
            _CacheManager.AddMeshgroupPreference(steamid, model.Index, meshgroup);
            _Storage.AddMeshgroupPreference(steamid, model.Index, meshgroup);
        }
        return true;
    }

    public void MeshgroupUpdate(CCSPlayerController player)
    {
        if (Utils.CanPlayerSetModelInstantly(player, Side.All))
        {
            var model = GetPlayerNowTeamModel(player);
            Utils.InstantUpdatePlayer(player, model, _Config.Inspection.Enable);
        }
    }

    public bool MapDefaultModelInitialized(CCSPlayerController player)
    {
        if (!_MapDefaultModels.ContainsKey(player.AuthorizedSteamID!.SteamId64))
        {
            return false;
        }
        MapDefaultModel model = _MapDefaultModels[player.AuthorizedSteamID!.SteamId64];
        if (player.Team == CsTeam.None || player.Team == CsTeam.Spectator)
        {
            return true;
        }
        return player.Team == CsTeam.Terrorist ? model.T != null : model.CT != null;
    }
    public void SetMapDefaultModel(CCSPlayerController player, string model)
    {
        var m = _MapDefaultModels.GetValueOrDefault(player.AuthorizedSteamID!.SteamId64, new MapDefaultModel());
        if (player.Team == CsTeam.CounterTerrorist)
        {
            m.CT = model;
        }
        else if (player.Team == CsTeam.Terrorist)
        {
            m.T = model;
        }
        _MapDefaultModels[player.AuthorizedSteamID!.SteamId64] = m;
    }
    public string? GetMapDefaultModel(CCSPlayerController player)
    {
        if (!_MapDefaultModels.ContainsKey(player.AuthorizedSteamID!.SteamId64))
        {
            return null;
        }
        if (player.Team == CsTeam.CounterTerrorist)
        {
            return _MapDefaultModels[player.AuthorizedSteamID!.SteamId64].CT;
        }
        else if (player.Team == CsTeam.Terrorist)
        {
            return _MapDefaultModels[player.AuthorizedSteamID!.SteamId64].T;
        }
        return null;
    }
    public void ClearMapDefaultModel()
    {
        _MapDefaultModels.Clear();
    }

}
