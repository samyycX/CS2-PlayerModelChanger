# CS2-PlayerModelChanger
![](https://img.shields.io/badge/build-passing-brightgreen) ![](https://img.shields.io/github/license/samyycX/CS2-PlayerModelChanger
) ![](https://img.shields.io/badge/Feedback-blue?style=flat&logo=discord&logoColor=white&link=https%3A%2F%2Fdiscord.com%2Fchannels%2F1160907911501991946%2F1210856437786484747
) ![](https://img.shields.io/badge/Tutorial-By_KEDI103-grey?style=flat&logo=youtube&labelColor=red&link=https%3A%2F%2Fgithub.com%2FKEDI103&link=https%3A%2F%2Fyoutu.be%2F9Vy-im9N8KM
) ![](https://img.shields.io/badge/%E4%B8%AD%E6%96%87%E6%95%99%E7%A8%8B-red?logo=china&link=https%3A%2F%2Fgithub.com%2FsamyycX%2FCS2-PlayerModelChanger%2Fblob%2Fmaster%2FREADME_CN.md
)

✨ A CounterStrikeSharp plugin that allows players to customize their models, with features like a model selection menu, permission restrictions, setting default models, and other advanced options.

> [!CAUTION] 
> This plugin can cause a GSLT ban, please use at your own risk.

- [🚀 Features](#rocket-features)
- [📥 Installation Guide](#inbox_tray-installation-guide)
- [📦 Optional Dependencies](#package-optional-dependencies)
- [🛠️ Commands](#hammer_and_wrench-commands)
- [⚙️ Configuration](#gear-configuration)
- [🐞 Common Issues](#bug-common-issues)
- [🙏 Credits](#pray-credits)
- [📋 TODOs](#clipboard-todos)
- [🤝 Contribution](#handshake-contribution)
- [📚 How to add default or workshop model](#books-how-to-add-default-or-workshop-model)

Custom model parts:
- [How to pack a model into steam workshop item](#how-to-pack-a-model-into-steam-workshop-item)


## 🚀 Features
- Browse models smoothly using the WASD keys.
- Set different models for `T` and `CT`.
- Surprise your players with a random model every time!
- Changes reflect immediately.
- View models in third person for a full perspective.
- Set a default model for different sides.
- Offer unique models based on player permissions.
- Toggle the leg model display on or off for enhanced customization.

## 📥 Installation Guide
Download the plugin from latest [Release](https://github.com/samyycX/CS2-PlayerModelChanger/releases), then put it into your counterstrikesharp plugin folder.

## 📦 Optional Dependencies
1. [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager) (for workshop model)

## 🛠️ Commands
### Menu commands (Recommended!)
- `!md <all/ct/t> / !models <all/ct/t>` select model
- `!mg / !mesh` select meshgroup (if exists)

### Client side
- `!model` show sender the model he is using + helper
- `!model <@random / model name> <all/ct/t>` change sender's model (@random for random model every spawn)
### Server side
- `pmc_enable [true/false]` Enable / Disable the plugin
- `pmc_resynccache` Resync cache.

### Admin (Need `@pmc/admin` flag or `#pmc/admin` group)
- `!modeladmin [all/steamid] reset [all/ct/t]` Reset player's model.
- `!modeladmin [all/steamid] set [all/ct/t] [model index] ` Set player's model.
- `!modeladmin [steamid] check` Check if player's model is not allowed to have, if not then reset it.
- `!modeladmin reload` Reload the config.

## ⚙️ Configuration
When you install the plugin successfully, it will generate `counterstrikesharp/configs/plugins/PlayerModelChanger/PlayerModelChanger.json`.

See [Wiki](https://github.com/samyycX/CS2-PlayerModelChanger/wiki) to config it.

## 📚 How to add default or workshop model

### Find the model path
Use `Source2Viewer` or `GCFScape` to open the workshop vpk (or pak01 vpk), then find the `.vmdl_c` file, copy the path out

the path should be like this: `characters/.../xxx.vmdl` (if it is in characters folder)

**Important: replace `.vmdl_c` in the path with `.vmdl`**

### Setup MultiAddonManager ( for workshop model )
add your workshop id to this plugin, follow [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)
after added, switch the map once to make multiaddonmanager download the addon

### Config PlayerModelChanger
See the [Configuration](#configuration)

## 🐞 Common Issues
- **You should use the compiled model (suffix `.vmdl_c`)**
- **You should use `.vmdl` instead of `.vmdl_c` in config json**
- If your model don't have animation, try to switch the map once (to make multiaddonmanager download the model)

## 🙏 Credits
- Method to change model: [DefaultSkins](https://github.com/Challengermode/cm-cs2-defaultskins) by ChallengerMode
- Thirdperson inspection code: [ThirdPerson-WIP](https://github.com/UgurhanK/ThirdPerson-WIP) by UgurhanK

## 📋 TODOs
1. Translation

## 🤝 Contribution
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
