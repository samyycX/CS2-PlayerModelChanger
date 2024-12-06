using System.Collections.Concurrent;
using System.Reflection;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace PlayerModelChanger;

public class Utils
{

    public static void ExecuteSide(Side? side, Action? whenAll, Action whenT, Action whenCT, Action? invalid = null)
    {
        Action action = side switch
        {
            Side.All => whenAll ?? (() => { whenT(); whenCT(); }),
            Side.T => whenT,
            Side.CT => whenCT,
            _ => invalid ?? (() => { })
        };
        action();
    }

    public static bool PlayerHasPermission(CCSPlayerController player, string[] permissions, string[] permissionsOr)
    {

        foreach (string perm in permissions)
        {
            if (perm.StartsWith("@"))
            {
                if (!AdminManager.PlayerHasPermissions(player, [perm]))
                {
                    return false;
                }
            }
            else if (perm.StartsWith("#"))
            {
                if (!AdminManager.PlayerInGroup(player, [perm]))
                {
                    return false;
                }
            }
            else
            {
                ulong steamId;
                if (!ulong.TryParse(perm, out steamId))
                {
                    throw new FormatException($"Unknown SteamID64 format: {perm}");
                }
                else
                {
                    if (player.SteamID != steamId)
                    {
                        return false;
                    }
                }

            }

        }

        foreach (string perm in permissionsOr)
        {
            if (perm.StartsWith("@"))
            {
                if (AdminManager.PlayerHasPermissions(player, perm))
                {
                    return true;
                }
            }
            else if (perm.StartsWith("#"))
            {
                if (AdminManager.PlayerInGroup(player, perm))
                {
                    return true;
                }
            }
            else
            {
                ulong steamId;
                if (!ulong.TryParse(perm, out steamId))
                {
                    throw new FormatException($"Unknown SteamID64 format: {perm}");
                }
                else
                {
                    if (player.SteamID == steamId)
                    {
                        return true;
                    }
                }
            }
        }
        return permissionsOr.Length == 0;
    }

    public static bool CanPlayerSetModelInstantly(CCSPlayerController? player, Side side)
    {
        if (player == null || !player.IsValid || player.PlayerPawn.Value == null || !player.PlayerPawn.IsValid)
        {
            return false;
        }
        if (player.PlayerPawn.Value.LifeState != ((byte)LifeState_t.LIFE_ALIVE))
        {
            return false;
        }
        return IsUpdatingSameTeam(player, side);

    }

    private static bool IsUpdatingSameTeam(CCSPlayerController player, Side side)
    {
        if (player.Team == CsTeam.None || player.Team == CsTeam.Spectator)
        {
            return false;
        }
        if (side == Side.All)
        {
            return true;
        }
        return (side == Side.T && player.Team == CsTeam.Terrorist) || (side == Side.CT && player.Team == CsTeam.CounterTerrorist);
    }

    public static void InstantUpdatePlayer(CCSPlayerController player, Model? model, bool enableThirdPersonPreview)
    {
        if (player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid)
        {
            return;
        }
        PlayerModelChanger.getInstance().SetModelNextServerFrame(player, model, model == null ? false : model.Disableleg).ContinueWith((_) =>
        {
            Server.NextFrame(() =>
            {
                if (enableThirdPersonPreview)
                {
                    var model = PlayerModelChanger.getInstance().Service.GetPlayerNowTeamModel(player);
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
                        Inspection.InspectModelForPlayer(player, path, model);
                    }
                }
                player.PrintToChat(PlayerModelChanger.getInstance().Localizer["command.model.instantsuccess"]);
            });
        });
    }

    public static void InitializeLangPrefix()
    {
        var Localizer = PlayerModelChanger.getInstance().Localizer;
        var localizerField = Localizer.GetType().GetField("_localizer", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var internalLocalizer = localizerField.GetValue(Localizer)!;
        var jsonResourceManagerField = internalLocalizer.GetType().GetField("_resourceManager", BindingFlags.Instance | BindingFlags.NonPublic)!;
        JsonResourceManager jsonResourceManager = (JsonResourceManager)jsonResourceManagerField.GetValue(internalLocalizer)!;
        var resourcesCacheField = jsonResourceManager.GetType().GetField("_resourcesCache", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var tryLoadResourceSet = jsonResourceManager.GetType().GetMethod("TryLoadResourceSet", BindingFlags.NonPublic | BindingFlags.Instance, [typeof(string)])!;
        tryLoadResourceSet.Invoke(jsonResourceManager, ["en"]);
        tryLoadResourceSet.Invoke(jsonResourceManager, ["pt-BR"]);
        tryLoadResourceSet.Invoke(jsonResourceManager, ["ru"]);
        tryLoadResourceSet.Invoke(jsonResourceManager, ["zh-Hans"]);

        jsonResourceManager.GetString("command.model.success"); // make it initialize
        ConcurrentDictionary<string, ConcurrentDictionary<string, string>> resourcesCache = (ConcurrentDictionary<string, ConcurrentDictionary<string, string>>)resourcesCacheField.GetValue(jsonResourceManager)!;
        foreach (var caches in resourcesCache)
        {
            foreach (var key in caches.Value.Keys)
            {
                caches.Value[key] = caches.Value[key].Replace("%pmc_prefix%", "[{green}PlayerModelChanger{default}] ");
            }
        }
    }

    public static ulong CalculateMeshgroupmask(int[] enabledMeshgroups, Dictionary<int, int> fixedMeshgroups)
    {
        ulong meshgroupmask = 0;
        foreach (var meshgroup in enabledMeshgroups)
        {
            meshgroupmask |= (ulong)1 << meshgroup;
        }
        foreach (var fixedMeshgroup in fixedMeshgroups)
        {
            if (fixedMeshgroup.Value == 0)
            {
                meshgroupmask &= ~((ulong)1 << fixedMeshgroup.Key);
            }
            else
            {
                meshgroupmask |= (ulong)1 << fixedMeshgroup.Key;
            }
        }
        return meshgroupmask;
    }

}
