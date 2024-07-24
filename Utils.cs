using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace PlayerModelChanger;

public class Utils {

    public static void ExecuteSide(string side, Action? whenAll, Action whenT, Action whenCT, Action? invalid = null) {
        switch (side.ToLower()) {
            case "all":
                if (whenAll == null) {
                    whenT();
                    whenCT();
                } else {
                    whenAll();
                }
                break;
            case "t":
                whenT();
                break;
            case "ct":
                whenCT();
                break;
            default:
                if (invalid != null) {
                    invalid();
                }
                break;
        };
    }

    public static bool PlayerHasPermission(CCSPlayerController player, string[] permissions, string[] permissionsOr) {
        
        foreach (string perm in permissions) {
            if (perm.StartsWith("@")) {
                if (!AdminManager.PlayerHasPermissions(player, new string[]{perm})) {
                  return false;
                }
            }
          else if (perm.StartsWith("#")) {
              if (!AdminManager.PlayerInGroup(player, new string[]{perm})) {
                  return false;
              }
          }
          else {
              ulong steamId;
              if (!ulong.TryParse(perm, out steamId)) {
                  throw new FormatException($"Unknown SteamID64 format: {perm}");
              } else {
                  if (player.SteamID != steamId) {
                      return false;
                  }
              }
            
          }

        }

        foreach (string perm in permissionsOr) {
            if (perm.StartsWith("@")) {
                if (AdminManager.PlayerHasPermissions(player, perm)) {
                    return true;
                }
            }
            else if (perm.StartsWith("#")) {
                if (AdminManager.PlayerInGroup(player, perm)) {
                    return true;
                }
            } else {
                ulong steamId;
                if (!ulong.TryParse(perm, out steamId)) {
                    throw new FormatException($"Unknown SteamID64 format: {perm}");
                } else {
                    if (player.SteamID == steamId) {
                        return true;
                    }
                }  
          }
        }
        return true;
    }

    public static bool isUpdatingSameTeam(CCSPlayerController player, string side) {
        side = side.ToLower();
        if (side == "all") {
            return true;
        }
        return (side == "t" && player.Team == CsTeam.Terrorist) || (side == "ct" && player.Team == CsTeam.CounterTerrorist);
    }

    public static void RespawnPlayer(CCSPlayerController player, bool disableThirdPersonPreview) {
        Server.NextFrame(() => {
            var playerPawn = player.PlayerPawn.Value!;
            var absOrigin = new Vector(playerPawn.AbsOrigin?.X, playerPawn.AbsOrigin?.Y, playerPawn.AbsOrigin?.Z);
            var absAngle = new QAngle(playerPawn.AbsRotation?.X, playerPawn.AbsRotation?.Y, playerPawn.AbsRotation?.Z);
            var health = playerPawn.Health;
            var armor = playerPawn.ArmorValue;
            CCSPlayer_ItemServices services = new CCSPlayer_ItemServices(playerPawn.ItemServices!.Handle);
            var armorHelmet = services.HasHelmet;
            var defuser = services.HasDefuser;

            player.Respawn();
            playerPawn.Teleport(absOrigin, absAngle);
            playerPawn.Health = health;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            playerPawn.ArmorValue = armor;
            services.HasHelmet = armorHelmet;
            services.HasDefuser = defuser;
            Utilities.SetStateChanged(playerPawn, "CBasePlayerPawn", "m_pItemServices");
            if (!disableThirdPersonPreview) {
                ThirdPerson.ThirdPersonPreviewForPlayer(player);
            }
        });
    }
}
