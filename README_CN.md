# CS2-PlayerModelChanger
一个轻量的counterstrikesharp插件，用于改变玩家模型

如果你喜欢这个插件请给个Star :)
### 此插件可能导致GSLT封禁，请自行承担风险
- **[用前须知](#用前须知)**
- [已知问题](#k已知问题)
- [自定义模型依赖插件](#自定义模型依赖插件)
- [安装指南](#安装指南)
- [命令](#命令)
- [配置](#配置)
- [感谢](#感谢)
- [TODOs](#todos)
- [常见问题](#常见问题)
- [Contribution](#contribution)

自定义模型部分
- [如何添加原版或创意工坊模型](#如何添加原版或创意工坊模型)
- [如何把你的模型打包并上传到创意工坊](#如何把你的模型打包并上传到创意工坊)


## 用前须知
1. 此插件仍然在开发过程中，数据库的结构可能会发生变化 **(如果改变，需要你自己修改数据库或者删除数据库)**
2. **此插件可能导致GSLT封禁，请自行承担风险**
3. 查看 [已知问题](#已知问题)

## 已知问题
1. 有些服务器在切换地图时会崩溃，解决方法是把 `DisablePrecache` 设置为 `true`，然后配合ResourcePrecacher插件 v1.0.4版本使用

## 自定义模型依赖插件
**如果你不使用自定义模型，不用安装这些插件**
1. [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)

## 安装指南
从 [Release](https://github.com/samyycX/CS2-PlayerModelChanger/releases)下载最新版插件, 然后放到counterstrikesharp的plugins文件夹里

## 指令
### 服务器端
- `pmc_enable [true/false]` 开启 / 关闭此插件
- `pmc_resynccache` 重新同步缓存
### 客户端
- `!model` 显示玩家当前用的模型 + 帮助
- `!model <@random / model name> <all/ct/t>` 改变玩家的模型 (@random 代表随机模型)
- `!md <all/ct/t> / !models <all/ct/t>` 选择模型
### Admin (需要 `@pmc/admin` flag 或 `#pmc/admin` 组)
- `!modeladmin [all/steamid] reset [all/ct/t]` 重置玩家的模型
- `!modeladmin [all/steamid] set [all/ct/t] [model index] ` 设置玩家的模型
- `!modeladmin [steamid] check` 检查玩家是否可以使用当前使用的模型，如果不可以则重置
- `!modeladmin reload` 重载配置文件
## 配置
如果你正确的安装了此插件，`counterstrikesharp/configs/plugins/PlayerModelChanger/PlayerModelChanger.json`文件会被生成。
```

见 [Wiki](https://github.com/samyycX/CS2-PlayerModelChanger/wiki)


## 如何添加原版或创意工坊模型

### 找到模型路径
用 `Source2Viewer` 或 `GCFScape` 打开创意工坊vpk (或者原版的pak01 vpk)

找到你想要的模型 (`.vmdl_c`文件)，复制出他的路径
路径应该是这样的: `characters/.../xxx.vmdl` (如果在characters文件夹里)

**注意: 路径里的文件格式`.vmdl_c`应该被替换成`.vmdl`**


### 设置 MultiAddonManager
在这个插件里设置你的workshop id, 请看 [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)

### 设置 PlayerModelChanger
请看 [配置](#配置) 部分

## 感谢
- 替换模型的方法: [DefaultSkins](https://github.com/Challengermode/cm-cs2-defaultskins), 作者 ChallengerMode

## TODOs
1. 翻译

## 常见问题
- **你应该使用编译后的模型 (后缀为 `.vmdl_c`)**
- **在配置文件内，你应该使用 `.vmdl` 而不是 `.vmdl_c` 作为后缀**

## Contribution
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
