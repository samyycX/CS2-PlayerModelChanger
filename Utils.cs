using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

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
}
