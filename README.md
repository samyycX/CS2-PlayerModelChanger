# CS2-PlayerModelChanger
A lightweighted counterstrikesharp plugin to change player model.

If you like this plugin, please give a star :)

**[A video tutorial about model can be found here](https://github.com/samyycX/CS2-PlayerModelChanger/issues/31)** (thanks to [@KEDI103](https://github.com/KEDI103))

### This plugin can cause a GSLT ban, please use at your own risk.
[中文教程请点这里](https://github.com/samyycX/CS2-PlayerModelChanger/blob/master/README_CN.md)
- **[Before you use](#before-you-use)**
- [Feature](#feature)
- [Installation Guide](#installation-guide)
- [Optional Dependencies](#optional-dependencies)
- [Commands](#commands)
- [Configuration](#configuration)
- [Common Issues](#common-issues)
- [Credits](#credits)
- [TODOs](#todos)
- [Contribution](#contribution)
- [How to add default or workshop model](#how-to-add-default-or-workshop-model)

Custom model parts:
- [How to pack a model into steam workshop item](#how-to-pack-a-model-into-steam-workshop-item)

## Before you use
1. **this plugin can cause a GSLT ban, use at your own risk**

## Feature
- a model select menu (support wasd menu)
- can set different model for T and CT
- random model
- update model after change instantly
- thirdperson inspection
- can set default model
- can provide special models for specified permission or player
- can disable the display of leg model

## Installation Guide
Download the plugin from latest [Release](https://github.com/samyycX/CS2-PlayerModelChanger/releases), then put it into your counterstrikesharp plugin folder.

## Optional Dependencies
1. [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager) (for workshop model)
2. [WASDMenuAPI](https://github.com/Interesting-exe/WASDMenuAPI) (for wasd interactive menu)

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

See [Wiki](https://github.com/samyycX/CS2-PlayerModelChanger/wiki) to config it.

## How to add default or workshop model

### Find the model path
Use `Source2Viewer` or `GCFScape` to open the workshop vpk (or pak01 vpk), then find the `.vmdl_c` file, copy the path out

the path should be like this: `characters/.../xxx.vmdl` (if it is in characters folder)

**Important: replace `.vmdl_c` in the path with `.vmdl`**

### Setup MultiAddonManager ( for workshop model )
add your workshop id to this plugin, follow [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)
after added, switch the map once to make multiaddonmanager download the addon

### Config PlayerModelChanger
See the [Configuration](#configuration)

## Common Issues
- **You should use the compiled model (suffix `.vmdl_c`)**
- **You should use `.vmdl` instead of `.vmdl_c` in config json**
- If your model don't have animation, try to switch the map once (to make multiaddonmanager download the model)

## Credits
- Method to change model: [DefaultSkins](https://github.com/Challengermode/cm-cs2-defaultskins) by ChallengerMode
- Thirdperson inspection code: [ThirdPerson-WIP](https://github.com/UgurhanK/ThirdPerson-WIP) by UgurhanK

## TODOs
1. Translation

## Contribution
To build this plugin, run `build.bat`.

Feel free to create Pull Requests or issues.

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
