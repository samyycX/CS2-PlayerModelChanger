# CS2-PlayerModelChanger
![](https://img.shields.io/badge/build-passing-brightgreen) ![](https://img.shields.io/github/downloads/samyycX/CS2-PlayerModelChanger/total
) ![](https://img.shields.io/github/stars/samyycX/CS2-PlayerModelChanger?style=flat&logo=github
) ![](https://img.shields.io/github/license/samyycX/CS2-PlayerModelChanger
) [![](https://img.shields.io/badge/Feedback-blue?style=flat&logo=discord&logoColor=white
)](https://discord.com/channels/1160907911501991946/1210856437786484747) [![](https://img.shields.io/badge/Tutorial-By_KEDI103-grey?style=flat&logo=youtube&labelColor=red)](https://youtu.be/9Vy-im9N8KM) [![](https://img.shields.io/badge/%E4%B8%AD%E6%96%87%E6%95%99%E7%A8%8B-red
)](https://github.com/samyycX/CS2-PlayerModelChanger/blob/master/README_CN.md)

✨ A CounterStrikeSharp plugin that allows players to customize their models, with features like a model selection menu, permission restrictions, setting default models, and other advanced options.

> [!CAUTION] 
> This plugin can cause a GSLT ban, please use at your own risk.

<div><video controls src="https://github.com/user-attachments/assets/5b4c34e7-69ea-4b13-ba16-7a811d2c2e42" muted="true"></video></div>

- [🚀 Features](#-features)
- [📥 Installation Guide](#-installation-guide)
- [📦 Optional Dependencies](#-dependencies)
- [🛠️ Commands](#-commands)
- [⚙️ Configuration](#-configuration)
- [🐞 Common Issues](#-issues)
- [🙏 Credits](#-credits)
- [📋 TODOs](#-todos)
- [🤝 Contribution](#-contribution)
- [📚 How to add default or workshop model](#-how-to-add-default-or-workshop-model)

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
- `!md <all/ct/t> / !models <all/ct/t>` open select model menu
- `!mg / !mesh` open select meshgroup menu (if exists)
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

### Step 1: Locate the Model Path

1. Open the workshop `.vpk` (or `pak01.vpk`) file using `Source2Viewer` or `GCFScape`.
2. Navigate to find the `.vmdl_c` file. Copy its path.
3. Replace `.vmdl_c` in the path with `.vmdl`.

The path should look like this:

```plaintext
characters/.../xxx.vmdl
```

> [!IMPORTANT]
> **replace `.vmdl_c` in the path with `.vmdl`**

### Step 2: Setup MultiAddonManager (Workshop Model Only)

1. Add your workshop ID to `MultiAddonManager`.
2. Follow the instructions provided on the [MultiAddonManager GitHub page](https://github.com/Source2ZE/MultiAddonManager).
3. After adding the ID, switch the map once to initiate the download of the addon by `MultiAddonManager`.


### Step 3: Config PlayerModelChanger
See the [Configuration](#configuration)

## 🐞 Common Issues
- **You should use the compiled model (suffix `.vmdl_c`)**
- **You should use `.vmdl` instead of `.vmdl_c` in config json**
- If your model don't have animation, try to switch the map once (to make multiaddonmanager download the model)

## 🙏 Credits
- Method to change model: [DefaultSkins](https://github.com/Challengermode/cm-cs2-defaultskins) by ChallengerMode
- Thirdperson inspection code: [ThirdPerson-WIP](https://github.com/UgurhanK/ThirdPerson-WIP) by UgurhanK
- Menu：[WASDMenuAPI](https://github.com/Interesting-exe/WASDMenuAPI) by Interesing-exe
## 📋 TODOs
1. Translation

## 🤝 Contribution
To build this plugin, run `dotnet build`.

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
