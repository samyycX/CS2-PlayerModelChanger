using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;

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

    public static bool PlayerHasPermission(CCSPlayerController player, string[] permissions) {
        IEnumerable<string> source = permissions.Where((string perm) => perm.StartsWith('#'));
        IEnumerable<string> source2 = permissions.Where((string perm) => perm.StartsWith('@'));
        if (!AdminManager.PlayerHasPermissions(player, source2.ToArray()))
        {
            return false;
        }

        if (!AdminManager.PlayerInGroup(player, source.ToArray()))
        {
            return false;
        }
        return true;
    }
}