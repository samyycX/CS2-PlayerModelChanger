using CounterStrikeSharp.API.Core;

using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using CounterStrikeSharp.API.Modules.Config;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Memory;
using System.Runtime.InteropServices;
namespace PlayerModelChanger;

public partial class PlayerModelChanger : BasePlugin
{
    public override string ModuleName => "Player Model Changer";
    public override string ModuleVersion => "1.8.6";
    public override string ModuleAuthor => "samyyc";

    public override void Load(bool hotReload)
    {
        Bootstrap.Run(this, hotReload);
    }

    
}
