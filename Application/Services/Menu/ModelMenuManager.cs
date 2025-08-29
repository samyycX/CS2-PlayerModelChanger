using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace PlayerModelChanger;

public class ModelMenuManager
{
  public static readonly Dictionary<int, ModelMenuPlayer> Players = new();

  public void OpenMainMenu(CCSPlayerController player, WasdModelMenu menu)
  {
    Players[player.Slot].OpenMainMenu(menu);
  }

  public void CloseMenu(CCSPlayerController player)
  {
    Players[player.Slot].CloseMenu();
  }

  public void OpenSubMenu(CCSPlayerController player, WasdModelMenu menu)
  {
    Players[player.Slot].OpenSubMenu(menu);
  }

  public void CloseSubMenu(CCSPlayerController player)
  {
    Players[player.Slot].CloseSubMenu();
  }

  public void AddPlayer(int slot, ModelMenuPlayer menuPlayer)
  {
    Players.Add(slot, menuPlayer);
  }

  public ModelMenuPlayer GetPlayer(int slot)
  {
    return Players[slot];
  }

  public void RemovePlayer(int slot)
  {
    Players.Remove(slot);
  }

  public void ReloadPlayer()
  {
    foreach (var player in Utilities.GetPlayers())
    {
      Players[player.Slot] = new ModelMenuPlayer
      {
        Player = player,
        Buttons = player.Buttons
      };
    }
  }

  public void RerenderPlayer(int slot)
  {
    Players[slot].Rerender();
  }

  public void Update()
  {
    foreach (var player in Players.Values.Where(player => player.HasMenu()))
    {
      if ((player.Buttons & PlayerButtons.Forward) == 0 && (player.Player.Buttons & PlayerButtons.Forward) != 0)
      {
        player.ScrollUp();
      }
      else if ((player.Buttons & PlayerButtons.Back) == 0 && (player.Player.Buttons & PlayerButtons.Back) != 0)
      {
        player.ScrollDown();
      }
      else if ((player.Buttons & PlayerButtons.Moveright) == 0 && (player.Player.Buttons & PlayerButtons.Moveright) != 0)
      {
        player.Next();
      }
      else if ((player.Buttons & PlayerButtons.Moveleft) == 0 && (player.Player.Buttons & PlayerButtons.Moveleft) != 0)
      {
        player.Prev();
      }
      else if ((player.Buttons & PlayerButtons.Use) == 0 && (player.Player.Buttons & PlayerButtons.Use) != 0)
      {
        player.ToTop();
      }
      else if ((player.Buttons & PlayerButtons.Reload) == 0 && (player.Player.Buttons & PlayerButtons.Reload) != 0)
      {
        player.ToSelected();
      }

      if (((long)player.Player.Buttons & 8589934592) == 8589934592)
      {
        player.CloseMenu();
      }

      player.Buttons = player.Player.Buttons;
      if (player.CenterHtml != "")
        Server.NextFrame(() =>
        player.Player.PrintToCenterHtml(player.CenterHtml)
    );
    }
  }
}