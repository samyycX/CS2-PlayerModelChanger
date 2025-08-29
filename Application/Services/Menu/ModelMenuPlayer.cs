using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace PlayerModelChanger;

public class ModelMenuPlayer
{
  public required CCSPlayerController Player { get; set; }
  public Stack<WasdModelMenu> Menus { get; set; } = new();

  public PlayerButtons Buttons { get; set; }

  public string CenterHtml { get; set; } = "";

  public void OpenMainMenu(WasdModelMenu menu)
  {
    Menus.Clear();
    Menus.Push(menu);
    Render();
  }

  public void CloseMenu()
  {
    Menus.Clear();
  }

  public void OpenSubMenu(WasdModelMenu menu)
  {
    Menus.Push(menu);
    Render();
  }

  public void CloseSubMenu()
  {
    Menus.Pop();
    Render();
  }

  public void ScrollUp()
  {
    Menus.Peek().ScrollUp();
    Render();
  }

  public void ScrollDown()
  {
    Menus.Peek().ScrollDown();
    Render();
  }

  public void Next()
  {
    Menus.Peek().Next(Player);
    Render();
  }

  public void Prev()
  {
    if (Menus.Count > 1)
    {
      CloseSubMenu();
    }
  }
  public void ToTop()
  {
    Menus.Peek().ToTop();
    Render();
  }
  public void ToSelected()
  {
    Menus.Peek().ToSelected();
    Render();
  }

  public bool HasMenu()
  {
    return Menus.Count > 0;
  }

  public void Render()
  {
    CenterHtml = Menus.Peek().Render();
  }

  public void Rerender()
  {
    Menus.ElementAt(Menus.Count - 1).Rerender(Player); // the root menu should contains all the path to submenus and eventually update them all
  }

}