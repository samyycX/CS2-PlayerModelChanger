# CS2-PlayerModelChanger
![](https://img.shields.io/badge/build-passing-brightgreen) ![](https://img.shields.io/github/downloads/samyycX/CS2-PlayerModelChanger/total
) ![](https://img.shields.io/github/stars/samyycX/CS2-PlayerModelChanger?style=flat&logo=github
) ![](https://img.shields.io/github/license/samyycX/CS2-PlayerModelChanger
) [![](https://img.shields.io/badge/Feedback-blue?style=flat&logo=discord&logoColor=white
)](https://discord.com/channels/1160907911501991946/1210856437786484747) [![](https://img.shields.io/badge/Tutorial-By_KEDI103-grey?style=flat&logo=youtube&labelColor=red)](https://youtu.be/9Vy-im9N8KM) [![](https://img.shields.io/badge/%E4%B8%AD%E6%96%87%E6%95%99%E7%A8%8B-red
)](https://github.com/samyycX/CS2-PlayerModelChanger/blob/master/README_CN.md)

✨ 一个CounterStrikeSharp插件，允许玩家自定义他们的模型，具有诸如模型选择菜单、权限限制、设置默认模型以及其他高级功能。

> [!CAUTION] 
> 此插件可能导致GSLT封禁，请自行承担风险

<div><video controls src="https://github.com/user-attachments/assets/5b4c34e7-69ea-4b13-ba16-7a811d2c2e42" muted="true"></video></div>

- [🚀 功能](#-功能)
- [📦 自定义模型依赖插件](#-自定义模型依赖插件)
- [📥 安装指南](#-安装指南)
- [🛠️ 命令](#-命令)
- [⚙️ 配置](#-配置)
- [🐞 常见问题](#-常见问题)
- [🙏 感谢](#-感谢)
- [📋 TODOs](#-todos)
- [🤝 Contribution](#-contribution)

自定义模型部分
- [如何添加原版或创意工坊模型](#如何添加原版或创意工坊模型)
- [如何把你的模型打包并上传到创意工坊](#如何把你的模型打包并上传到创意工坊)

## 🚀 功能
- 菜单式选择模型 (wasd菜单)
- 可为T/CT阵营设置不同模型
- 随机模型
- 选择后可立刻更新
- 第三人称预览
- 可设置默认模型
- 可按照权限和玩家设置特殊模型
- 可禁用腿部模型

## 📥 安装指南
从 [Release](https://github.com/samyycX/CS2-PlayerModelChanger/releases)下载最新版插件, 然后放到counterstrikesharp的plugins文件夹里

## 📦 自定义模型依赖插件
**如果你不使用自定义模型，不用安装这些插件**
1. [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)



## 📦 可选依赖
1. 使用创意工坊模型请安装：[MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)


## 🛠️ 指令
### 菜单指令 (推荐使用!)
- `!md <all/ct/t> / !models <all/ct/t>` 打开选择模型菜单
- `!mg` 打开皮肤组选择菜单
### 客户端
- `!model` 显示玩家当前用的模型 + 帮助
- `!model <@random / model name> <all/ct/t>` 改变玩家的模型 (@random 代表随机模型)
- `!md <all/ct/t> / !models <all/ct/t>` 打开选择模型菜单
- `!mg` 打开皮肤组选择菜单
### 服务器端
- `pmc_enable [true/false]` 开启 / 关闭此插件
- `pmc_resynccache` 重新同步缓存
### Admin (需要 `@pmc/admin` flag 或 `#pmc/admin` 组)
- `!modeladmin [all/steamid] reset [all/ct/t]` 重置玩家的模型
- `!modeladmin [all/steamid] set [all/ct/t] [model index] ` 设置玩家的模型
- `!modeladmin [steamid] check` 检查玩家是否可以使用当前使用的模型，如果不可以则重置
- `!modeladmin reload` 重载配置文件

## ⚙️ 配置
如果你正确的安装了此插件，`counterstrikesharp/configs/plugins/PlayerModelChanger/PlayerModelChanger.json`文件会被生成。


见 [Wiki](https://github.com/samyycX/CS2-PlayerModelChanger/wiki)

## 🐞 常见问题
- **你应该使用编译后的模型 (后缀为 `.vmdl_c`)**
- **在配置文件内，你应该使用 `.vmdl` 而不是 `.vmdl_c` 作为后缀**


## 📚 如何添加原版或创意工坊模型

### 1. 找到模型路径
用 `Source2Viewer` 或 `GCFScape` 打开创意工坊vpk (或者原版的pak01 vpk)

找到你想要的模型 (`.vmdl_c`文件)，复制出他的路径
路径应该是这样的: `characters/.../xxx.vmdl` (如果在characters文件夹里)

**注意: 路径里的文件格式`.vmdl_c`应该被替换成`.vmdl`**


### 2. 设置 MultiAddonManager
在这个插件里设置你的workshop id, 请看 [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)

### 3. 设置 PlayerModelChanger
请看 [配置](#配置) 部分

## 🙏 感谢
- 替换模型的方法: [DefaultSkins](https://github.com/Challengermode/cm-cs2-defaultskins) by ChallengerMode
- 第三人称预览代码: [ThirdPerson-WIP](https://github.com/UgurhanK/ThirdPerson-WIP) by UgurhanK
- 菜单：[WASDMenuAPI](https://github.com/Interesting-exe/WASDMenuAPI) by Interesing-exe

## 📋 TODOs
1. 翻译

## 🤝 Contribution
构建插件请使用 `dotnet build`.

欢迎一切 Issues / Pull Requests



## 如何把你的模型打包并上传到创意工坊
前置:
- 你自己的模型
- Counter-Strike 2 Workshop Tools (可以在 `Steam客户端 -> cs2 -> 属性 -> DLC`里安装)

**Step 1.** 打开cs2安装目录, 找到 `game/csgo/gameinfo.gi`,
定位到文件最后，找到`AddonConfig -> VpkDirectories`
。然后添加你想要打包到VPK里的文件夹，下面是例子:


*例子*:
```json
AddonConfig	
	{
		"VpkDirectories"
		{
			"exclude"       "maps/content_examples"
			"include"       "maps"
			"include"		"characters" // 这是需要打包进去的文件夹
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

**Step 2.** 启动 `Counter-Strike 2 Workshop Tools`, 然后点击 `Create New Addon`

**Step 3.** 前往目录 `./game/csgo_addons/<your addon name>/` 然后把你的character文件夹复制到这里

**Step 4.** 打开 `Asset Browser`, 然后点击右上角的 `Tools` 按钮， 打开 `Counter-Strike 2 Workshop Manager`

**Step 5.** 点击 `Counter-Strike 2 Workshop Manager` 里的 `New` 按钮, 把所有信息都填上，然后发布。

**Step 6.** 等待Steam审核。
