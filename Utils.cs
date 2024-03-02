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
}