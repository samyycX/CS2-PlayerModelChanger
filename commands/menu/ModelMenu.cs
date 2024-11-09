using System.Text;
using CounterStrikeSharp.API.Core;

namespace PlayerModelChanger;

public abstract class MenuOption
{
  public string Text { get; set; } = "";

  public abstract void Next(CCSPlayerController player, WasdModelMenu menu);

  public virtual void Rerender(CCSPlayerController player, WasdModelMenu menu) { }
}

public class SubMenuOption : MenuOption
{
  public required WasdModelMenu NextMenu { get; set; }
  public override void Next(CCSPlayerController player, WasdModelMenu menu)
  {
    PlayerModelChanger.getInstance().MenuManager.OpenSubMenu(player, NextMenu);
  }

  public override void Rerender(CCSPlayerController player, WasdModelMenu menu)
  {
    NextMenu.Rerender(player);
  }
}

public class SelectOption : MenuOption
{
  public Action<CCSPlayerController, SelectOption, WasdModelMenu> Select { get; set; } = (_, _, _) => { };

  public Action<CCSPlayerController, SelectOption, WasdModelMenu> RerenderAction { get; set; } = (_, _, _) => { };

  public Dictionary<string, dynamic> AdditionalProperties = new();
  public bool IsSelected = false;

  public override void Next(CCSPlayerController player, WasdModelMenu menu)
  {
    Select(player, this, menu);
    IsSelected = !IsSelected;
    if (IsSelected)
    {
      menu.Options.ForEach(option =>
      {
        if (option != this && option is SelectOption)
        {
          ((SelectOption)option).IsSelected = false;
        }
      });
    }
  }

  public override void Rerender(CCSPlayerController player, WasdModelMenu menu)
  {
    RerenderAction(player, this, menu);
  }

  public SelectOption SetAdditionalProperty(string key, dynamic value)
  {
    AdditionalProperties[key] = value;
    return this;
  }
}

public class UncancellableSelectOption : SelectOption
{
  public override void Next(CCSPlayerController player, WasdModelMenu menu)
  {
    Select(player, this, menu);
    IsSelected = true;
    menu.Options.ForEach(option =>
    {
      if (option != this && option is UncancellableSelectOption)
      {
        ((UncancellableSelectOption)option).IsSelected = false;
      }
    });
  }

  public new UncancellableSelectOption SetAdditionalProperty(string key, dynamic value)
  {
    AdditionalProperties[key] = value;
    return this;
  }
}


public class MultiSelectOption : MenuOption
{
  public Action<CCSPlayerController, MultiSelectOption, WasdModelMenu> Select { get; set; } = (_, _, _) => { };

  public Action<CCSPlayerController, MultiSelectOption, WasdModelMenu> RerenderAction { get; set; } = (_, _, _) => { };

  public bool IsSelected = false;

  public override void Next(CCSPlayerController player, WasdModelMenu menu)
  {
    Select(player, this, menu);
    IsSelected = !IsSelected;
  }

  public override void Rerender(CCSPlayerController player, WasdModelMenu menu)
  {
    RerenderAction(player, this, menu);
  }
}


public class WasdModelMenu
{
  const int MAX_OPTIONS = 4;

  public string Title { get; set; } = "";
  public List<MenuOption> Options { get; set; } = new();

  public int StartOffset = 0;

  public int SelectedOption = 0;

  public void AddOption(MenuOption option)
  {
    Options.Add(option);
  }

  public void ScrollDown()
  {
    if (Options.Count == 0)
    {
      return;
    }
    SelectedOption = (SelectedOption + 1) % Options.Count;

    if (SelectedOption < StartOffset)
    {
      StartOffset = SelectedOption;
    }

    if (SelectedOption >= StartOffset + MAX_OPTIONS)
    {
      StartOffset = SelectedOption - MAX_OPTIONS + 1;
    }

  }

  public void ScrollUp()
  {
    if (Options.Count == 0)
    {
      return;
    }
    SelectedOption = (SelectedOption - 1 + Options.Count) % Options.Count;

    if (SelectedOption < StartOffset)
    {
      StartOffset = SelectedOption;
    }

    if (SelectedOption >= StartOffset + MAX_OPTIONS)
    {
      StartOffset = SelectedOption - MAX_OPTIONS + 1;
    }
  }

  public void ToTop()
  {
    SelectedOption = 0;
    StartOffset = 0;
  }

  public void ToSelected()
  {
    SelectedOption = Options.FindIndex(option => option is SelectOption && ((SelectOption)option).IsSelected);
    if (SelectedOption == -1)
    {
      return;
    }
    if (SelectedOption < StartOffset)
    {
      StartOffset = SelectedOption;
    }

    if (SelectedOption >= StartOffset + MAX_OPTIONS)
    {
      StartOffset = SelectedOption - MAX_OPTIONS + 1;
    }
  }

  public void Next(CCSPlayerController player)
  {
    if (Options.Count == 0)
    {
      return;
    }
    Options[SelectedOption].Next(player, this);
  }

  public void Rerender(CCSPlayerController player)
  {
    Options.ForEach(option => option.Rerender(player, this));
  }

  public string Render()
  {
    StringBuilder builder = new StringBuilder();
    builder.AppendLine($"<font color='#3b62d9'>{Title}</u></font color='white'>");
    builder.AppendLine("<br>");
    for (int i = StartOffset; i < StartOffset + MAX_OPTIONS; i++)
    {
      if (i >= Options.Count)
      {
        builder.AppendLine("<br>");
        continue;
      }
      if (i == SelectedOption)
      {
        builder.AppendLine($"<font color='#ccacfc'>⋆ {Options[i].Text}</font> <br>");
        continue;
      }
      if (Options[i] is SelectOption && ((SelectOption)Options[i]).IsSelected)
      {
        builder.AppendLine($"<font color='#7219f7'>⋆ {Options[i].Text}</font> <br>");
        continue;
      }
      if (Options[i] is MultiSelectOption && ((MultiSelectOption)Options[i]).IsSelected)
      {
        builder.AppendLine($"<font color='#7219f7'>⋆ {Options[i].Text}</font> <br>");
        continue;
      }
      builder.AppendLine($"<font color='white'>{Options[i].Text}</font> <br>");

    }

    builder.AppendLine($"<font class='fontSize-s' color='#9ee1f0'>{PlayerModelChanger.getInstance().Localizer["modelmenu.instruction"]}</font><br>");
    builder.AppendLine($"<font class='fontSize-s' color='#9ee1f0'>{PlayerModelChanger.getInstance().Localizer["modelmenu.instruction2"]}</font>");
    builder.AppendLine("</div>");
    return builder.ToString();

  }

}