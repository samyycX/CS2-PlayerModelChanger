using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using PlayerModelChanger.Services;

namespace PlayerModelChanger;

struct MapDefaultModel
{
    public string? T { get; set; }
    public string? CT { get; set; }
}

public class ModelService
{

    private ConfigurationService _ConfigurationService;
    private ModelConfig _Config;
    private IStorage _Storage;
    private IStringLocalizer _Localizer;
    private DefaultModelService _DefaultModelManager;
    private ModelCacheService _CacheManager;
    private PlayerService _PlayerService;
    private PermissionService _PermissionService;

    private Dictionary<ulong, MapDefaultModel> _MapDefaultModels = new Dictionary<ulong, MapDefaultModel>();


    private Dictionary<ulong, long> _ModelChangeCooldown = new Dictionary<ulong, long>();

    public ModelService(
        ConfigurationService configurationService,
        DatabaseService databaseService,
        IStringLocalizer localizer,
        DefaultModelService defaultModelService,
        ModelCacheService modelCacheService,
        PlayerService playerService,
        PermissionService permissionService
        )
    {
        _ConfigurationService = configurationService;
        _Config = configurationService.ModelConfig;
        _Storage = databaseService.GetStorage();
        _Localizer = localizer;
        _DefaultModelManager = defaultModelService;
        _CacheManager = modelCacheService;
        _PlayerService = playerService;
        _PermissionService = permissionService;

        _CacheManager.ResyncCache();
    }

    public void InitializeModel(string key, Model model)
    {
        model.Index = key;
        if (model.Name == "")
        {
            model.Name = model.Index;
        }
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
                InstantUpdatePlayer(player, model, _Config.Inspection.Enable);
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
                InstantUpdatePlayer(player, model, inspection && _Config.Inspection.Enable && model?.Index != "@random");
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
        return _PermissionService.PlayerHasPermission(player, model.Permissions, model.PermissionsOr) && // permission
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
            SetPlayerAllModel(steamid, "@default", "@default", false, false);
            return new Tuple<bool, bool>(false, false);
        }
        var tValid = CheckModel(player, Side.T, modelCache, defaultTModel);
        var ctValid = CheckModel(player, Side.CT, modelCache, defaultCTModel);
        if (!tValid && !ctValid)
        {
            SetPlayerAllModel(steamid, "@default", "@default", false);
            return new Tuple<bool, bool>(true, true);
        }
        else if (!tValid)
        {
            SetPlayerModel(steamid, "@default", Side.T, false);
            return new Tuple<bool, bool>(true, false);
        }
        else if (!ctValid)
        {
            SetPlayerModel(steamid, "@default", Side.CT, false);
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
        if (defaultModel != null && defaultModel.force && modelIndex != "@default")
        {
            return false;
        }
        if (modelIndex == "@random" && _Config.DisableRandomModel)
        {
            return false;
        }
        if (modelIndex == "" || modelIndex == "@random" || modelIndex == "@default")
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

    private bool IsPlayerInCooldown(CCSPlayerController player)
    {
        if (_ModelChangeCooldown.ContainsKey(player.SteamID))
        {
            var lastTime = _ModelChangeCooldown[player.SteamID];
            if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastTime < (_Config.ModelChangeCooldownSecond * 1000))
            {
                player.PrintToChat(_Localizer["command.model.cooldown"]);
                return true;
            }
        }
        return false;
    }

    public bool SetPlayerModelWithCheck(CCSPlayerController player, string modelIndex, Side side)
    {
        if (IsPlayerInCooldown(player))
        {
            return false;
        }

        var isSpecial = modelIndex == "" || modelIndex == "@random";

        if (modelIndex == "@default")
        {
            Utils.ExecuteSide(side,
                () => SetPlayerAllModel(player!.AuthorizedSteamID!.SteamId64, "@default", "@default", false),
                () => SetPlayerModel(player!.AuthorizedSteamID!.SteamId64, "@default", side, false),
                () => SetPlayerModel(player!.AuthorizedSteamID!.SteamId64, "@default", side, false)
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

            if (!_PermissionService.PlayerHasPermission(player, model.Permissions, model.PermissionsOr))
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

    public Model? GetRandomModel(CCSPlayerController player, Side side)
    {
        var models = GetAllAppliableModels(player, side);
        if (models.Count() == 0)
        {
            return null;
        }
        var index = Random.Shared.Next(models.Count());
        return models[index];
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

        if (modelIndex == "@default")
        {
            var defaultModel = _DefaultModelManager.GetPlayerDefaultModel(player, side);
            if (defaultModel == null || defaultModel.index.Count == 0)
            {
                return null;
            }
            var index = defaultModel.index[Random.Shared.Next(defaultModel.index.Count)];
            if (index == "")
            {
                return null;
            }
            modelIndex = index;
        }
        if (modelIndex == "@random")
        {
            return GetRandomModel(player, side);
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
            InstantUpdatePlayer(player, model, _Config.Inspection.Enable);
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

    public int GetSkinPreference(CCSPlayerController player, Model model)
    {
        return _CacheManager.GetSkinPreference(player.AuthorizedSteamID!.SteamId64, model.Index);
    }

    public void SetSkinPreference(CCSPlayerController player, Model model, int skin, bool update = true)
    {
        _CacheManager.UpdateSkinPerference(player.AuthorizedSteamID!.SteamId64, model.Index, skin);
        _Storage.UpdateSkinPerference(player.AuthorizedSteamID!.SteamId64, model.Index, skin);
        if (update)
        {
            var pawn = player.PlayerPawn.Value!;
            pawn.AcceptInput("Skin", pawn, pawn, skin.ToString());
        }
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

    public void InstantUpdatePlayer(CCSPlayerController player, Model? model, bool enableThirdPersonPreview)
    {
        if (player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid)
        {
            return;
        }
        SetModelNextServerFrame(player, model, model == null ? false : model.Disableleg).ContinueWith((_) =>
        {
            Server.NextFrame(() =>
            {
                if (enableThirdPersonPreview)
                {
                    var model = GetPlayerNowTeamModel(player);
                    var path = "";
                    if (model == null || model.Path == "")
                    {
                        path = player.PlayerPawn.Value.CBodyComponent?.SceneNode?.GetSkeletonInstance().ModelState.ModelName;
                    }
                    else
                    {
                        path = model.Path;
                    }
                    if (path != null)
                    {
                        _PlayerService.GetInspectionService(player.Slot).InspectModelForPlayer(path, model);
                    }
                }
                player.PrintToChat(_Localizer["command.model.instantsuccess"]);
            });
        });
    }

    public Task SetModelNextServerFrame(CCSPlayerController player, Model? model, bool disableleg)
    {
        return Server.NextFrameAsync(() =>
        {
            var pawn = player.Pawn.Value!;
            var originalRender = pawn.Render;
            if (model == null)
            {
                var defaultModel = GetMapDefaultModel(player);
                if (defaultModel != null)
                {
                    pawn.SetModel(defaultModel);

                }
                pawn.Render = Color.FromArgb(_ConfigurationService.ModelConfig.DisableDefaultModelLeg ? 254 : 255, originalRender.R, originalRender.G, originalRender.B);
                Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

                return;
            }
            pawn.SetModel(model.Path);
            pawn.Render = Color.FromArgb(disableleg ? 254 : 255, originalRender.R, originalRender.G, originalRender.B);
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

            if (model.FixedSkin != -1)
            {
                pawn.AcceptInput("Skin", pawn, pawn, model.FixedSkin.ToString());
            }
            else
            {
                pawn.AcceptInput("Skin", pawn, pawn, GetSkinPreference(player, model).ToString());
            }

            ulong meshgroupmask = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.MeshGroupMask;
            if (InitMeshgroupPreference(player, model, meshgroupmask))
            {
                return;
            }
            meshgroupmask = Utils.CalculateMeshgroupmask(GetMeshgroupPreference(player, model).ToArray(), model.FixedMeshgroups);
            if (meshgroupmask != 0)
            {
                pawn.CBodyComponent.SceneNode.GetSkeletonInstance().ModelState.MeshGroupMask = meshgroupmask;
                Utilities.SetStateChanged(pawn, "CBaseEntity", "m_CBodyComponent");
            }

        });
    }
}
