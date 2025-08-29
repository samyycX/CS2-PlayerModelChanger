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
