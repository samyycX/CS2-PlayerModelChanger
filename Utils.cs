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
        IEnumerable<string> source = permissions.Where((string perm) => perm.StartsWith('#'));
        IEnumerable<string> source2 = permissions.Where((string perm) => perm.StartsWith('@'));
        if (source2.Count() != 0 && !AdminManager.PlayerHasPermissions(player, source2.ToArray()))
        {
            return false;
        }

        if (source.Count() != 0 && !AdminManager.PlayerInGroup(player, source.ToArray()))
        {
            return false;
        }

        var flag = permissionsOr.Count() == 0;
        foreach (var perm in permissionsOr) {
            if (perm.StartsWith("#") && AdminManager.PlayerHasPermissions(player, perm)) {
                flag = true;
            }
            if (perm.StartsWith("@") && AdminManager.PlayerInGroup(player, perm)) {
                flag = true;
            }
        }

        return flag;
    }
}