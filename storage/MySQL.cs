using Dapper;
using MySqlConnector;
using Service;
using Storage;

namespace Storage;
public class MySQLStorage : IStorage {

    private MySqlConnection conn;

    private string table;
    public MySQLStorage(string ip, string port, string user, string password, string database, string table) {
        string connectStr = $"server={ip};port={port};user={user};password={password};database={database};Pooling=true;MinimumPoolSize=0;MaximumPoolsize=640;ConnectionIdleTimeout=30;AllowUserVariables=true";
        this.table = table;
        conn = new MySqlConnection(connectStr);
        conn.Execute($"""
            CREATE TABLE IF NOT EXISTS `{table}` (
                `steamid` BIGINT UNSIGNED NOT NULL PRIMARY KEY,
                `t_model` TEXT,
                `ct_model` TEXT
            );
        """);
        // UPDATE #1
        conn.Execute($"""
            SET @dbname = DATABASE();
            SET @tablename = "{table}";
            SET @columnname = "t_permission_bypass";
            SET @columnname2 = "ct_permission_bypass";
            SET @preparedStatement = (SELECT IF(
            (
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE
                (table_name = @tablename)
                AND (table_schema = @dbname)
                AND (column_name = @columnname)
            ) > 0,
            "SELECT 1",
            CONCAT("ALTER TABLE ", @tablename, " ADD ", @columnname, " BOOLEAN;")
            ));
            SET @preparedStatement2 = (SELECT IF(
            (
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE
                (table_name = @tablename)
                AND (table_schema = @dbname)
                AND (column_name = @columnname2)
            ) > 0,
            "SELECT 1",
            CONCAT("ALTER TABLE ", @tablename, " ADD ", @columnname2, " BOOLEAN;")
            ));
            PREPARE alterIfNotExists FROM @preparedStatement;
            PREPARE alterIfNotExists2 FROM @preparedStatement2;
            EXECUTE alterIfNotExists;
            EXECUTE alterIfNotExists2;
            DEALLOCATE PREPARE alterIfNotExists;
            DEALLOCATE PREPARE alterIfNotExists2;
            
        """);
    }
    public List<ModelCache>? GetAllPlayerModel() {
        try {   
            return conn.Query<ModelCache>($"select * from {table};").ToList();
        } catch (InvalidOperationException e) {
            
        }
        return null;
    }

    public dynamic? GetPlayerModel(ulong SteamID, string modelfield)
    {
        var result = conn.QueryFirstOrDefault($"SELECT `{modelfield}` FROM `{table}` WHERE `steamid` = {SteamID};");
        return result;
    }

    public string? GetPlayerTModel(ulong SteamID)
    {
        var result = GetPlayerModel(SteamID, "t_model");
        if (result == null) {
            return null;
        }
        return result!.t_model;
    }
    public string? GetPlayerCTModel(ulong SteamID)
    {
        var result = GetPlayerModel(SteamID, "ct_model");
        if (result == null) {
            return null;
        }
        return result!.ct_model;
    }

    public async Task<int> SetPlayerModel(ulong SteamID, string modelName, string modelfield, bool permissionBypass, string side)
    {
        
        var sql = $"""
            INSERT INTO {table} (`steamid`, `{modelfield}`, `{side}_permission_bypass`) VALUES ({SteamID}, @model, @permissionBypass) ON DUPLICATE key UPDATE `{modelfield}` = @model, `{side}_permission_bypass`=@permissionBypass;
            """;
        return await conn.ExecuteAsync(sql,
            new {
                model = modelName, permissionBypass
            }
        );
       
    }
    public async Task<int> SetPlayerTModel(ulong SteamID, string modelName, bool permissionBypass) {
        return await SetPlayerModel(SteamID, modelName, "t_model", permissionBypass, "t");
    }
    public async Task<int> SetPlayerCTModel(ulong SteamID, string modelName, bool permissionBypass) {
        return await SetPlayerModel(SteamID, modelName, "ct_model", permissionBypass, "ct");
    }
    public async Task<int> SetAllTModel(string tmodel, bool permissionBypass) {
        return await conn.ExecuteAsync(@$"
                UPDATE `{table}` SET `t_model` = @tmodel, `t_permission_bypass`=@permissionBypass",
                new {
                    tmodel,
                    permissionBypass
                }
        );
    }
    public async Task<int> SetAllCTModel(string ctmodel, bool permissionBypass) {
        return await conn.ExecuteAsync(@$"
            UPDATE `{table}` SET `ct_model` = @ctmodel, `ct_permission_bypass`=@permissionBypass",
            new {
                ctmodel,
                permissionBypass
            }
        );
    }
}