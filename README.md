# CS2-PlayerModelChanger
A lightweighted counterstrikesharp plugin to change player model.

If you like this plugin, please give a star :)
### This plugin can cause a GSLT ban, please use at your own risk.
[中文教程请点这里](https://github.com/samyycX/CS2-PlayerModelChanger/blob/master/README_CN.md)
- **[Before you use](#before-you-use)**
- [Known issues](#known-issues)
- [Installation Guide](#installation-guide)
- [Commands](#commands)
- [Configuration](#configuration)
- [Common Issues](#common-issues)
- [Credits](#credits)
- [TODOs](#todos)
- [Contribution](#contribution)
- [How to add default or workshop model](#how-to-add-default-or-workshop-model)

Custom model parts:
- [Dependencies for custom model](#dependencies-for-custom-model)
- [How to pack a model into steam workshop item](#how-to-pack-a-model-into-steam-workshop-item)

## Before you use
1. This plugin is still in development, which means the player's data database structure may be changed **(If changed, the database need to be reseted or be modified by your own)**
2. **this plugin can cause a GSLT ban, use at your own risk**
3. See [Known issues](#known-issues)

## Known issues
1. Some server will crash when map change, in this case, check the permission of your workshop item and try to set `DisablePrecache` in config to `true` and use ResourcePrecacher plugin V1.0.4

## Installation Guide
Download the plugin from latest [Release](https://github.com/samyycX/CS2-PlayerModelChanger/releases), then put it into your counterstrikesharp plugin folder.

## Commands
### Server side
- `pmc_enable [true/false]` Enable / Disable the plugin
- `pmc_resynccache` Resync cache.
### Client side
- `!model` show sender the model he is using + helper
- `!model <@random / model name> <all/ct/t>` change sender's model (@random for random model every spawn)
- `!md <all/ct/t> / !models <all/ct/t>` select model
### Admin (Need `@pmc/admin` flag or `#pmc/admin` group)
- `!modeladmin [all/steamid] reset [all/ct/t]` Reset player's model.
- `!modeladmin [all/steamid] set [all/ct/t] [model index] ` Set player's model.
- `!modeladmin [steamid] check` Check if player's model is not allowed to have, if not then reset it.
- `!modeladmin reload` Reload the config.
## Configuration
When you install the plugin successfully, it will generate `counterstrikesharp/configs/plugins/PlayerModelChanger/PlayerModelChanger.json`.

See [Wiki](https://github.com/samyycX/CS2-PlayerModelChanger/wiki)

## How to add default or workshop model

### Find the model path
Use `Source2Viewer` or `GCFScape` to open the workshop vpk (or pak01 vpk), then find the `.vmdl_c` file, copy the path out

the path should be like this: `characters/.../xxx.vmdl` (if it is in characters folder)

**Important: replace `.vmdl_c` in the path with `.vmdl`**

### Setup MultiAddonManager
add your workshop id to this plugin, follow [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)

### Config PlayerModelChanger
See the [Configuration](#configuration)

## Common Issues
- **You should use the compiled model (suffix `.vmdl_c`)**
- **You should use `.vmdl` instead of `.vmdl_c` in config json**

## Credits
- Method to change model: [DefaultSkins](https://github.com/Challengermode/cm-cs2-defaultskins) by ChallengerMode

## TODOs
1. Translation

## Contribution
To build this plugin, run `dotnet build`.

Feel free to create Pull Requests or issues.

## Dependencies for custom model
**If you are not using custom model, these plugins are not necessarily needed**
1. [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)

## How to pack a model into steam workshop item

Requirements:
- Your own model
- Counter-Strike 2 Workshop Tools (which can be installed in `Steam -> cs2 -> properties -> DLC`)

**Step 1.** Open your cs2 directory, find `game/csgo/gameinfo.gi`,
go to the  end of the file, find `AddonConfig -> VpkDirectories`
. Then add the directory you want to put in the vpk like the following example:


*example*:
```json
AddonConfig	
	{
		"VpkDirectories"
		{
			"exclude"       "maps/content_examples"
			"include"       "maps"
			"include"		"characters" // this is the directory you want to add to the vpk
			"include"       "cfg/maps"
			"include"       "materials"
			"include"       "models"
			"include"       "panorama/images/overheadmaps"
			"include"       "panorama/images/map_icons"
			"include"       "particles"
			"include"       "resource/overviews"
			"include"       "scripts/vscripts"
			"include"       "sounds"
			"include"       "soundevents"
			"include"       "lighting/postprocessing"
			"include"       "postprocess"
			"include"       "addoninfo.txt"
		} 
		"AllowAddonDownload" "1"
		"AllowAddonDownloadForDemos" "1"
		"DisableAddonValidationForDemos" "1"
	}
```

**Step 2.** Launch `Counter-Strike 2 Workshop Tools`, then click `Create New Addon`

**Step 3.** Go to folder `./game/csgo_addons/<your addon name>/` and paste your characters folder to here.

**Step 4.** Open `Asset Browser`, then click the `Tools` button on the top-right corner, open `Counter-Strike 2 Workshop Manager`

**Step 5.** Click `New` button in the `Counter-Strike 2 Workshop Manager`, fill in all the information, and publish it.

**Step 6.** After verification, you should be able to use the workshop item.
