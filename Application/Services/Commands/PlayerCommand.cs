using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;

namespace PlayerModelChanger.Services.Commands;

public class PlayerCommand
{
    private ModelService _ModelService { get; init; }
    private ConfigurationService _ConfigurationService { get; init; }
    private IStringLocalizer _Localizer { get; init; }
    private DefaultModelService _DefaultModelService { get; init; }
    private MenuService _MenuService { get; init; }
    private PermissionService _PermissionService { get; init; }
    private PlayerModelChanger _Plugin { get; init; }

    public PlayerCommand(
        ModelService modelService,
        ConfigurationService configurationService,
        IStringLocalizer localizer,
        DefaultModelService defaultModelService,
        MenuService menuService,
        PermissionService permissionService,
        PlayerModelChanger plugin
    )
    {
        _ModelService = modelService;
        _ConfigurationService = configurationService;
        _Localizer = localizer;
        _DefaultModelService = defaultModelService;
        _MenuService = menuService;
        _PermissionService = permissionService;
        _Plugin = plugin;

        _Plugin.AddCommand("css_model", "Show your model.", ChangeModelCommand);
        _Plugin.AddCommand("css_models", "Select models.", GetAllModelsCommand);
        _Plugin.AddCommand("css_mesh", "Select models.", MeshgroupCommand);
        _Plugin.AddCommand("css_skin", "Select skin.", SkinCommand);
    }

    [ConsoleCommand("css_model", "Show your model.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void ChangeModelCommand(CCSPlayerController player, CommandInfo commandInfo)
    {

        if (commandInfo.ArgCount == 1)
        {
            var TModel = _ModelService.GetPlayerModelName(player, CsTeam.Terrorist);
            var CTModel = _ModelService.GetPlayerModelName(player, CsTeam.CounterTerrorist);
            commandInfo.ReplyToCommand(_Localizer["player.currentmodel", _Localizer["side.t"], TModel]);
            commandInfo.ReplyToCommand(_Localizer["player.currentmodel", _Localizer["side.ct"], CTModel]);
            commandInfo.ReplyToCommand(_Localizer["command.model.hint1"]);
            commandInfo.ReplyToCommand(_Localizer["command.model.hint2"]);
            return;
        }

        if (_ConfigurationService.ModelConfig.DisablePlayerSelection)
        {
            return;
        }
        if (!_PermissionService.PlayerHasBasicPermission(player))
        {
            commandInfo.ReplyToCommand(_Localizer["model.nochangepermission"]);
            return;
        }

        var modelIndex = commandInfo.GetArg(1);

        if (modelIndex != "@random" && !_ModelService.ExistModel(modelIndex))
        {
            var model = _ModelService.FindModel(modelIndex);
            if (model == null)
            {
                commandInfo.ReplyToCommand(_Localizer["command.model.notfound", modelIndex]);
                return;
            }
            else
            {
                modelIndex = model.Index;
            }
        }

        Side? side = Side.All;
        if (commandInfo.ArgCount == 3)
        {
            side = commandInfo.GetArg(2).ToSide();
            if (side == null)
            {
                commandInfo.ReplyToCommand(_Localizer["command.unknownside", "null"]);
                return;
            }
        }
        var defaultModel = _DefaultModelService.GetPlayerDefaultModel(player!, (Side)side!);
        if (defaultModel != null && defaultModel.force)
        {
            commandInfo.ReplyToCommand(_Localizer["model.nochangepermission"]);
            return;
        }

        _ModelService.SetPlayerModelWithCheck(player, modelIndex, (Side)side!);
    }

    [ConsoleCommand("css_md", "Select models.")]
    [ConsoleCommand("css_models", "Select models.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void GetAllModelsCommand(CCSPlayerController player, CommandInfo commandInfo)
    {

        if (_ConfigurationService.ModelConfig.DisablePlayerSelection)
        {
            return;
        }
        if (!_PermissionService.PlayerHasBasicPermission(player))
        {
            commandInfo.ReplyToCommand(_Localizer["model.nochangepermission"]);
            return;
        }
        if (commandInfo.ArgCount == 1)
        {
            _MenuService.OpenSelectSideMenu(player);
            return;
        }
        var side = commandInfo.GetArg(1).ToSide();
        if (side == null)
        {
            commandInfo.ReplyToCommand(_Localizer["command.unknownside", "null"]);
            return;
        }

        _MenuService.OpenSelectModelMenu(player, (Side)side!, _ModelService.GetPlayerModel(player, (Side)side!));

    }

    [ConsoleCommand("css_mesh", "Select models.")]
    [ConsoleCommand("css_mg", "Select models.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void MeshgroupCommand(CCSPlayerController player, CommandInfo commandInfo)
    {
        if (_ConfigurationService.ModelConfig.DisablePlayerSelection)
        {
            return;
        }
        if (!_PermissionService.PlayerHasBasicPermission(player))
        {
            commandInfo.ReplyToCommand(_Localizer["model.nochangepermission"]);
            return;
        }
        _MenuService.OpenSelectMeshgroupMenu(player);
    }

    [ConsoleCommand("css_skin", "Select skin.")]
    [ConsoleCommand("css_mat", "Select skin.")]
    [ConsoleCommand("css_materialgroup", "Select skin.")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void SkinCommand(CCSPlayerController player, CommandInfo commandInfo)
    {
        if (_ConfigurationService.ModelConfig.DisablePlayerSelection)
        {
            return;
        }
        if (!_PermissionService.PlayerHasBasicPermission(player))
        {
            commandInfo.ReplyToCommand(_Localizer["model.nochangepermission"]);
            return;
        }
        _MenuService.OpenSelectSkinMenu(player);
    }
}
