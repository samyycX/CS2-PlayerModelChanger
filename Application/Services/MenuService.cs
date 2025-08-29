using System.Text.Json;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Microsoft.Extensions.Localization;
using PlayerModelChanger.Factories;

namespace PlayerModelChanger.Services;



public class MenuService
{
  private ModelService _ModelService { get; init; }
  private DefaultModelService _DefaultModelService { get; init; }
  private ModelMenuManager _MenuManager { get; init; }
  private IStringLocalizer _Localizer { get; init; }
  private ConfigurationService _ConfigurationService { get; init; }
  private MenuFactory _MenuFactory { get; init; }

  public MenuService(
    ModelService modelService,
    DefaultModelService defaultModelService,
    ModelMenuManager menuManager,
    IStringLocalizer localizer,
    ConfigurationService configurationService,
    MenuFactory menuFactory
  )
  {
    _ModelService = modelService;
    _DefaultModelService = defaultModelService;
    _ConfigurationService = configurationService;
    _MenuManager = menuManager;
    _Localizer = localizer;
    _MenuFactory = menuFactory;
  }

  public UncancellableSelectOption GetSelectModelOption(CCSPlayerController player, Side side, string modelIndex, string text, bool isSelected = false, bool hasMeshgroup = false, bool hasSkin = false)
  {
    var option = _MenuFactory.CreateMenuOption<UncancellableSelectOption>();
    option.Text = text;
    option.Select = (player, option, menu) =>
    {
      if (option.IsSelected && (hasMeshgroup || hasSkin))
      {
        option.AdditionalProperties["meshgroupMenu"] = GetSelectMeshgroupMenu(player, hasSkin)!;
        _MenuManager.OpenSubMenu(player, option.AdditionalProperties["meshgroupMenu"]);
        return;
      }
      if (_ModelService.SetPlayerModelWithCheck(player, modelIndex, side))
      {
        menu.Title = _Localizer["modelmenu.title", _Localizer[$"side.{side.ToName()}"], text];
        var menus = _MenuManager.GetPlayer(player.Slot);
        if (menus.Menus.Count > 1)
        {
          var newSideMenu = GetSelectSideMenu(player);
          if (newSideMenu != null)
          {
            menus.Menus.ElementAt(menus.Menus.Count - 1).Options = newSideMenu!.Options;
          }

        }
      }
    };
    option.IsSelected = isSelected;
    option.RerenderAction = (player, option, menu) =>
    {
      if (option.AdditionalProperties.ContainsKey("meshgroupMenu"))
      {
        option.AdditionalProperties["meshgroupMenu"].Rerender(player); // make sure rerender chain pass through meshgroup menu
      }
    };
    return option;
  }

  public WasdModelMenu GetSelectModelMenu(CCSPlayerController player, Side side)
  {

    var menu = _MenuFactory.CreateMenu();
    var currentModel = _ModelService.GetPlayerModel(player, side);
    List<Model> models = _ModelService.GetAllAppliableModels(player, side);
    menu.Title = _Localizer["modelmenu.title", _Localizer[$"side.{side.ToName()}"], currentModel == null ? _Localizer["model.none"] : currentModel.Name];
    menu.AddOption(GetSelectModelOption(player, side, "", _Localizer["modelmenu.unset"]));
    if (!_ConfigurationService.ModelConfig.DisableRandomModel)
    {
      menu.AddOption(GetSelectModelOption(player, side, "@random", _Localizer["modelmenu.random"]));
    }
    menu.AddOption(GetSelectModelOption(player, side, "@default", _Localizer["modelmenu.default"]));

    foreach (var model in models)
    {
      if (model.HideInMenu)
      {
        continue;
      }
      var isSelected = false;
      if (currentModel != null && model.Index == currentModel.Index)
      {
        isSelected = true;
      }
      var hasMeshgroup = model.Meshgroups.Count > 0;
      var hasSkin = model.Skins.Count > 0;

      menu.AddOption(GetSelectModelOption(player, side, model.Index, model.Name, isSelected, hasMeshgroup, hasSkin));
    }
    return menu;
  }

  public WasdModelMenu? GetSelectSideMenu(CCSPlayerController player)
  {
    var menu = _MenuFactory.CreateMenu();
    menu.Title = _Localizer["modelmenu.selectside"];
    var sides = new List<Side>();

    var tDefault = _DefaultModelService.GetPlayerDefaultModel(player, Side.T);
    var ctDefault = _DefaultModelService.GetPlayerDefaultModel(player, Side.CT);
    if (tDefault == null || !tDefault.force)
    {
      sides.Add(Side.T);
    }
    if (ctDefault == null || !ctDefault.force)
    {
      sides.Add(Side.CT);
    }
    if (sides.Count == 2)
    {
      sides.Insert(0, Side.All);
    }
    if (sides.Count == 0)
    {
      return null;
    }

    foreach (var side in sides)
    {
      var playerModel = _ModelService.GetPlayerModel(player, side);

      var text = playerModel != null ? $"{_Localizer["side." + side.ToName()]}: {playerModel.Name}" : $"{_Localizer["side." + side.ToName()]}";
      var modelMenu = GetSelectModelMenu(player, side);

      var option = _MenuFactory.CreateMenuOption<SubMenuOption>();
      option.Text = text;
      option.NextMenu = modelMenu;

      menu.AddOption(option);
    }
    ;
    return menu;
  }
  public void OpenSelectSideMenu(CCSPlayerController player)
  {
    var modelMenu = GetSelectSideMenu(player);
    if (modelMenu == null)
    {
      player.PrintToChat(_Localizer["modelmenu.forced"]);
      return;
    }
    _MenuManager.OpenMainMenu(player, modelMenu);
  }

  public void OpenSelectModelMenu(CCSPlayerController player, Side side, Model? model)
  {
    var modelMenu = GetSelectModelMenu(player, side);
    var defaultModel = _DefaultModelService.GetPlayerDefaultModel(player, side);
    if (defaultModel != null && defaultModel.force)
    {
      player.PrintToChat(_Localizer["modelmenu.forced"]);
      return;
    }
    _MenuManager.OpenMainMenu(player, modelMenu);
  }

  public WasdModelMenu? GetSelectMeshgroupMenu(CCSPlayerController player, bool skin)
  {

    var menu = _MenuFactory.CreateMenu();
    var currentModel = _ModelService.GetPlayerNowTeamModel(player);
    if (currentModel == null || currentModel.Meshgroups.Count == 0)
    {
      return null;
    }
    menu.Title = currentModel.Name;
    var meshgroupPreference = _ModelService.GetMeshgroupPreference(player, currentModel);

    if (skin)
    {
      var skinMenu = GetSelectSkinMenu(player);
      if (skinMenu != null)
      {
        var option = _MenuFactory.CreateMenuOption<SubMenuOption>();
        option.Text = _Localizer["modelmenu.skin"];
        option.NextMenu = skinMenu;
        menu.AddOption(option);
      }
    }

    foreach (var meshgroup in currentModel.Meshgroups)
    {
      Dictionary<string, dynamic> data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(meshgroup.Value);
      bool radio = meshgroup.Key.Contains("@radio");
      bool opradio = meshgroup.Key.Contains("@opradio");
      bool combination = meshgroup.Key.Contains("@combination");
      var key = meshgroup.Key.Replace("@radio", "").Replace("@combination", "").Replace("@opradio", "");
      var subMenu = _MenuFactory.CreateMenu();
      subMenu.Title = key;

      foreach (var item in data)
      {
        if (!combination)
        {
          var value = JsonSerializer.Deserialize<int>(item.Value);
          var isSelected = false;
          if (meshgroupPreference.Contains(value))
          {
            isSelected = true;
          }
          if (radio)
          {
            var steamid = player.AuthorizedSteamID!.SteamId64;
            var usoption = _MenuFactory.CreateMenuOption<UncancellableSelectOption>();
            usoption.Text = item.Key;
            usoption.Select = (player, option, menu) =>
              {
                if (!option.IsSelected)
                {
                  _ModelService.AddMeshgroupPreference(player, currentModel, value);
                  foreach (var op in menu.Options)
                  {
                    if (op is SelectOption && ((SelectOption)op).IsSelected)
                    {
                      _ModelService.RemoveMeshgroupPreference(player, currentModel, ((SelectOption)op).AdditionalProperties["meshgroup"], false);
                    }
                  }
                  ;
                }
              };
            usoption.IsSelected = isSelected;
            usoption.RerenderAction = (player, option, menu) =>
            {
              option.IsSelected = _ModelService.HasMeshgroupPreference(player, currentModel, value);
            };
            usoption.SetAdditionalProperty("meshgroup", value);
            subMenu.AddOption(usoption);
          }
          else if (opradio)
          {
            var steamid = player.AuthorizedSteamID!.SteamId64;
            var opoption = _MenuFactory.CreateMenuOption<SelectOption>();
            opoption.Text = item.Key;
            opoption.Select = (player, option, menu) =>
            {
              if (!option.IsSelected)
              {
                _ModelService.AddMeshgroupPreference(player, currentModel, value);
                foreach (var op in menu.Options)
                {
                  if (op is SelectOption && ((SelectOption)op).IsSelected)
                  {
                    _ModelService.RemoveMeshgroupPreference(player, currentModel, ((SelectOption)op).AdditionalProperties["meshgroup"], false);
                  }
                }
              }
            };
            opoption.IsSelected = isSelected;
            opoption.RerenderAction = (player, option, menu) =>
            {
              option.IsSelected = _ModelService.HasMeshgroupPreference(player, currentModel, value);
            };
            opoption.SetAdditionalProperty("meshgroup", value);
            subMenu.AddOption(opoption);
          }
          else
          {
            var msoption = _MenuFactory.CreateMenuOption<MultiSelectOption>();
            msoption.Text = item.Key;
            msoption.Select = (player, option, menu) =>
            {
              if (!option.IsSelected)
              {
                _ModelService.AddMeshgroupPreference(player, currentModel, value);
              }
              else
              {
                _ModelService.RemoveMeshgroupPreference(player, currentModel, value);
              }
            };
            msoption.IsSelected = isSelected;
            msoption.RerenderAction = (player, option, menu) =>
            {
              option.IsSelected = _ModelService.HasMeshgroupPreference(player, currentModel, value);
            };
            subMenu.AddOption(msoption);
          }
        }
        else
        {
          var value = JsonSerializer.Deserialize<List<int>>(item.Value);
          if (!opradio && !radio)
          {
            var msoption = _MenuFactory.CreateMenuOption<MultiSelectOption>();
            msoption.Text = item.Key;
            msoption.Select = (player, option, menu) =>
            {
              if (!option.IsSelected)
              {
                foreach (var meshgroup in value)
                {
                  _ModelService.AddMeshgroupPreference(player, currentModel, meshgroup, false);
                }
              }
              else
              {
                foreach (var meshgroup in value)
                {
                  _ModelService.RemoveMeshgroupPreference(player, currentModel, meshgroup, false);
                }
              }
              _ModelService.MeshgroupUpdate(player);
              _MenuManager.RerenderPlayer(player.Slot);
            };
            subMenu.AddOption(msoption);
          }
          else
          {
            var usoption = _MenuFactory.CreateMenuOption<UncancellableSelectOption>();
            usoption.Text = item.Key;
            usoption.Select = (player, option, menu) =>
            {
              _ModelService.SetMeshgroupPreference(player, currentModel, value);
              _MenuManager.RerenderPlayer(player.Slot);
            };
            subMenu.AddOption(usoption);
          }
        }
      }
      var option = _MenuFactory.CreateMenuOption<SubMenuOption>();
      option.Text = key;
      option.NextMenu = subMenu;
      menu.AddOption(option);
    }
    return menu;
  }

  public WasdModelMenu? GetSelectSkinMenu(CCSPlayerController player)
  {
    var menu = _MenuFactory.CreateMenu();
    var currentModel = _ModelService.GetPlayerNowTeamModel(player);
    if (currentModel == null || currentModel.Skins.Count == 0)
    {
      return null;
    }
    menu.Title = currentModel.Name;
    var skinPreference = _ModelService.GetSkinPreference(player, currentModel);
    foreach (var skin in currentModel.Skins)
    {
      var usoption = _MenuFactory.CreateMenuOption<UncancellableSelectOption>();
      usoption.Text = skin.Key;
      usoption.IsSelected = skin.Value == skinPreference;
      usoption.Select = (player, option, menu) =>
      {
        _ModelService.SetSkinPreference(player, currentModel, skin.Value);
      };
      menu.AddOption(usoption);
    }

    return menu;
  }

  public void OpenSelectMeshgroupMenu(CCSPlayerController player)
  {
    // TODO: maybe also add skin menu? but we have !skin command
    var menu = GetSelectMeshgroupMenu(player, false);
    if (menu == null)
    {
      player.PrintToChat(_Localizer["modelmenu.nomeshgroup"]);
      return;
    }
    _MenuManager.OpenMainMenu(player, menu);
  }

  public void OpenSelectSkinMenu(CCSPlayerController player)
  {
    var menu = GetSelectSkinMenu(player);
    if (menu == null)
    {
      player.PrintToChat(_Localizer["modelmenu.noskin"]);
      return;
    }
    _MenuManager.OpenMainMenu(player, menu);
  }

}
