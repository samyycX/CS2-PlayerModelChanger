using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace PlayerModelChanger.Services;

public class PermissionService {
  private readonly PlayerModelChanger _Plugin;
  private readonly ConfigurationService _ConfigurationService;

  public PermissionService(PlayerModelChanger plugin, ConfigurationService configurationService) {
    _Plugin = plugin;
    _ConfigurationService = configurationService;
  }

  public bool PlayerHasBasicPermission(CCSPlayerController player)
  {
    var basicPermission = _ConfigurationService.ModelConfig.BasicPermission;
    if (basicPermission == "")
    {
      return true;
    }
    if (PlayerHasPermission(player, [basicPermission], []))
    {
      return true;
    }
    return false;
  }

  public bool PlayerHasPermission(CCSPlayerController player, string[] permissions, string[] permissionsOr)
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
}