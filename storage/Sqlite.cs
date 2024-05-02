using Microsoft.Data.Sqlite;
using Dapper;
using Service;
namespace Storage;

public class SqliteStorage : IStorage {

    private SqliteConnection conn { get; set; }
    public SqliteStorage(string ModuleDirectory) {

        conn = new SqliteConnection($"Data Source={Path.Join(ModuleDirectory, "data.db")}");
        conn.Open();

        conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS `players` (
                `steamid` UNSIGNED BIG INT NOT NULL,
                `t_model` TEXT,
                `ct_model` TEXT,
                `t_permission_bypass` BOOLEAN,
                `ct_permission_bypass` BOOLEAN,
                PRIMARY KEY (`steamid`));
            )
        ");      
        IEnumerable<dynamic> tPermissionBypassResult = conn.Query("select * from sqlite_master where name='players' and sql like '%t_permission_bypass%'");
        if (tPermissionBypassResult.Count() == 0) {
            conn.Execute("ALTER TABLE players ADD COLUMN `t_permission_bypass` BOOLEAN;");
        }
        IEnumerable<dynamic> ctPermissionBypassResult = conn.Query("select * from sqlite_master where name='players' and sql like '%ct_permission_bypass%'");
        if (ctPermissionBypassResult.Count() == 0) {
            conn.Execute("ALTER TABLE players ADD COLUMN `ct_permission_bypass` BOOLEAN;");
        }
    }
    public List<ModelCache> GetAllPlayerModel() {
        return conn.Query<ModelCache>($"select * from players;").ToList();
    }

    public dynamic? GetPlayerModel(ulong SteamID, string modelfield) {

        var result = conn.QueryFirstOrDefault(@$"SELECT `{modelfield}` FROM `players` WHERE `steamid` = @SteamId;", new { SteamId = SteamID });
        return result;
    }

    public string? GetPlayerTModel(ulong SteamID) {
        var result = GetPlayerModel(SteamID, "t_model");
        if (result == null) {
            return null;
        }
        return result!.t_model;
    }
    public string? GetPlayerCTModel(ulong SteamID) {
        var result = GetPlayerModel(SteamID, "ct_model");
        if (result == null) {
            return null;
        }
        return result!.ct_model;
    }

    public async Task<int> SetPlayerModel(ulong SteamId, string model, string modelfield, bool permissionBypass, string side) {

        return await conn.ExecuteAsync(@$"
            INSERT INTO `players` (`steamid`, `{modelfield}`, `{side}_permission_bypass`) VALUES (@SteamId, @model, @permissionBypass)
            ON CONFLICT(`steamid`) DO UPDATE SET `{modelfield}` = @model, `{side}_permission_bypass`=@permissionBypass;",
            new {
                SteamId,
                model,
                permissionBypass
            }
        );
        
    }
    public async Task<int> SetPlayerTModel(ulong SteamID, string model, bool permissionBypass) {
        return await SetPlayerModel(SteamID, model, "t_model", permissionBypass, "t");
    }
    public async Task<int> SetPlayerCTModel(ulong SteamID, string model, bool permissionBypass) {
        return await SetPlayerModel(SteamID, model, "ct_model", permissionBypass, "ct");
    }
    public async Task<int> SetAllTModel(string tmodel, bool permissionBypass) {
        return await conn.ExecuteAsync(@$"
            UPDATE `players` SET `t_model` = @tmodel, `t_permission_bypass`=@permissionBypass;",
            new {
                tmodel, permissionBypass
            }
        );
    }
    public async Task<int> SetAllCTModel(string ctmodel, bool permissionBypass) {
        return await conn.ExecuteAsync(@$"
            UPDATE `players` SET `ct_model` = @ctmodel, `ct_permission_bypass`=@permissionBypass;",
            new {
                ctmodel,
                permissionBypass
            }
        );
    }
}