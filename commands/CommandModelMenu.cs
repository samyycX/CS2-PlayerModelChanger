using System.Text.Json;
using CounterStrikeSharp.API.Core;

namespace PlayerModelChanger;



public partial class PlayerModelChanger
{

    public UncancellableSelectOption GetSelectModelOption(CCSPlayerController player, Side side, string modelIndex, string text, bool isSelected = false, bool hasMeshgroup = false)
    {
        return new UncancellableSelectOption()
        {
            Text = text,
            Select = (player, option, menu) =>
            {
                if (option.IsSelected && hasMeshgroup)
                {
                    option.AdditionalProperties["meshgroupMenu"] = GetSelectMeshgroupMenu(player)!;
                    MenuManager.OpenSubMenu(player, option.AdditionalProperties["meshgroupMenu"]);
                    return;
                }
                if (Service.SetPlayerModelWithCheck(player, modelIndex, side))
                {
                    menu.Title = Localizer["modelmenu.title", Localizer[$"side.{side.ToName()}"], text];
                    var menus = MenuManager.GetPlayer(player.Slot);
                    if (menus.Menus.Count > 1)
                    {
                        var newSideMenu = GetSelectSideMenu(player);
                        if (newSideMenu != null)
                        {
                            menus.Menus.ElementAt(menus.Menus.Count - 1).Options = newSideMenu!.Options;
                        }

                    }
                }
            },
            IsSelected = isSelected,
            RerenderAction = (player, option, menu) =>
            {
                if (option.AdditionalProperties.ContainsKey("meshgroupMenu"))

                {
                    option.AdditionalProperties["meshgroupMenu"].Rerender(player); // make sure rerender chain pass through meshgroup menu
                }
            },

        };
    }

    public WasdModelMenu GetSelectModelMenu(CCSPlayerController player, Side side)
    {

        var menu = new WasdModelMenu();
        var currentModel = Service.GetPlayerModel(player, side);
        List<Model> models = Service.GetAllAppliableModels(player, side);
        menu.Title = Localizer["modelmenu.title", Localizer[$"side.{side.ToName()}"], currentModel == null ? Localizer["model.none"] : currentModel.Name];
        menu.AddOption(GetSelectModelOption(player, side, "", Localizer["modelmenu.unset"]));
        if (!Config.DisableRandomModel)
        {
            menu.AddOption(GetSelectModelOption(player, side, "@random", Localizer["modelmenu.random"]));
        }
        menu.AddOption(GetSelectModelOption(player, side, "@default", Localizer["modelmenu.default"]));

        foreach (var model in models)
        {
            if (model.Hideinmenu)
            {
                continue;
            }
            var isSelected = false;
            if (currentModel != null && model.Index == currentModel.Index)
            {
                isSelected = true;
            }
            var hasMeshgroup = model.Meshgroups.Count > 0;

            menu.AddOption(GetSelectModelOption(player, side, model.Index, model.Name, isSelected, hasMeshgroup));
        }
        return menu;
    }

    public WasdModelMenu? GetSelectSideMenu(CCSPlayerController player)
    {
        var menu = new WasdModelMenu();
        menu.Title = Localizer["modelmenu.selectside"];
        var sides = new List<Side>();

        var tDefault = DefaultModelManager.GetPlayerDefaultModel(player, Side.T);
        var ctDefault = DefaultModelManager.GetPlayerDefaultModel(player, Side.CT);
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
            var playerModel = Service.GetPlayerModel(player, side);

            var text = playerModel != null ? $"{Localizer["side." + side.ToName()]}: {playerModel.Name}" : $"{Localizer["side." + side.ToName()]}";
            var modelMenu = GetSelectModelMenu(player, side);

            var option = new SubMenuOption { Text = text, NextMenu = modelMenu };

            menu.AddOption(option);
        };
        return menu;
    }
    public void OpenSelectSideMenu(CCSPlayerController player)
    {
        var modelMenu = GetSelectSideMenu(player);
        if (modelMenu == null)
        {
            player.PrintToChat(Localizer["modelmenu.forced"]);
            return;
        }
        MenuManager.OpenMainMenu(player, modelMenu);
    }

    public void OpenSelectModelMenu(CCSPlayerController player, Side side, Model? model)
    {
        var modelMenu = GetSelectModelMenu(player, side);
        var defaultModel = DefaultModelManager.GetPlayerDefaultModel(player, side);
        if (defaultModel != null && defaultModel.force)
        {
            player.PrintToChat(Localizer["modelmenu.forced"]);
            return;
        }
        MenuManager.OpenMainMenu(player, modelMenu);
    }

    public WasdModelMenu? GetSelectMeshgroupMenu(CCSPlayerController player)
    {

        var menu = new WasdModelMenu();
        var currentModel = Service.GetPlayerNowTeamModel(player);
        if (currentModel == null || currentModel.Meshgroups.Count == 0)
        {
            return null;
        }
        menu.Title = currentModel.Name;
        var meshgroupPreference = Service.GetMeshgroupPreference(player, currentModel);

        foreach (var meshgroup in currentModel.Meshgroups)
        {
            Dictionary<string, dynamic> data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(meshgroup.Value);
            bool radio = meshgroup.Key.Contains("@radio");
            bool opradio = meshgroup.Key.Contains("@opradio");
            bool combination = meshgroup.Key.Contains("@combination");
            var key = meshgroup.Key.Replace("@radio", "").Replace("@combination", "").Replace("@opradio", "");
            var subMenu = new WasdModelMenu { Title = key };

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
                        subMenu.AddOption(new UncancellableSelectOption
                        {
                            Text = item.Key,
                            Select = (player, option, menu) =>
                            {
                                if (!option.IsSelected)
                                {
                                    Service.AddMeshgroupPreference(player, currentModel, value);
                                    foreach (var op in menu.Options)
                                    {
                                        if (op is SelectOption && ((SelectOption)op).IsSelected)
                                        {
                                            Service.RemoveMeshgroupPreference(player, currentModel, ((SelectOption)op).AdditionalProperties["meshgroup"], false);
                                        }
                                    };
                                }
                            },
                            IsSelected = isSelected,
                            RerenderAction = (player, option, menu) =>
                            {
                                option.IsSelected = Service.HasMeshgroupPreference(player, currentModel, value);
                            }
                        }.SetAdditionalProperty("meshgroup", value));
                    }
                    else if (opradio)
                    {
                        var steamid = player.AuthorizedSteamID!.SteamId64;
                        subMenu.AddOption(new SelectOption
                        {
                            Text = item.Key,
                            Select = (player, option, menu) =>
                            {
                                if (!option.IsSelected)
                                {
                                    Service.AddMeshgroupPreference(player, currentModel, value);
                                    foreach (var op in menu.Options)
                                    {
                                        if (op is SelectOption && ((SelectOption)op).IsSelected)
                                        {
                                            Service.RemoveMeshgroupPreference(player, currentModel, ((SelectOption)op).AdditionalProperties["meshgroup"], false);
                                        }
                                    };
                                }
                                else
                                {
                                    Service.RemoveMeshgroupPreference(player, currentModel, value);
                                }
                            },
                            IsSelected = isSelected,
                            RerenderAction = (player, option, menu) =>
                            {
                                option.IsSelected = Service.HasMeshgroupPreference(player, currentModel, value);
                            }
                        }.SetAdditionalProperty("meshgroup", value));
                    }
                    else
                    {
                        subMenu.AddOption(new MultiSelectOption
                        {
                            Text = item.Key,
                            Select = (player, option, menu) =>
                            {
                                if (!option.IsSelected)
                                {
                                    Service.AddMeshgroupPreference(player, currentModel, value);
                                }
                                else
                                {
                                    Service.RemoveMeshgroupPreference(player, currentModel, value);
                                }
                            },
                            IsSelected = isSelected,
                            RerenderAction = (player, option, menu) =>
                            {
                                option.IsSelected = Service.HasMeshgroupPreference(player, currentModel, value);
                            }
                        });


                    }
                }
                else
                {
                    var value = JsonSerializer.Deserialize<List<int>>(item.Value);
                    if (!opradio && !radio)
                    {
                        subMenu.AddOption(new MultiSelectOption
                        {
                            Text = item.Key,
                            Select = (player, option, menu) =>
                            {
                                if (!option.IsSelected)
                                {
                                    foreach (var meshgroup in value)
                                    {
                                        Service.AddMeshgroupPreference(player, currentModel, meshgroup, false);
                                    }
                                }
                                else
                                {
                                    foreach (var meshgroup in value)
                                    {
                                        Service.RemoveMeshgroupPreference(player, currentModel, meshgroup, false);
                                    }
                                }
                                Service.MeshgroupUpdate(player);
                                MenuManager.RerenderPlayer(player.Slot);
                            }
                        });
                    }
                    else
                    {
                        subMenu.AddOption(new UncancellableSelectOption
                        {
                            Text = item.Key,
                            Select = (player, option, menu) =>
                            {
                                Service.SetMeshgroupPreference(player, currentModel, value);
                                MenuManager.RerenderPlayer(player.Slot);
                            },
                        });
                    }
                }
            }
            menu.AddOption(new SubMenuOption
            {
                Text = key,
                NextMenu = subMenu
            });
        }
        return menu;
    }

    public void OpenSelectMeshgroupMenu(CCSPlayerController player)
    {
        var menu = GetSelectMeshgroupMenu(player);
        if (menu == null)
        {
            player.PrintToChat(Localizer["modelmenu.nomeshgroup"]);
            return;
        }
        MenuManager.OpenMainMenu(player, menu);
    }

}
