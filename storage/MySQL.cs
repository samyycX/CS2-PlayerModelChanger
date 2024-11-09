using Dapper;
using MySqlConnector;

namespace PlayerModelChanger;
public class MySQLStorage : IStorage
{

    private string Table;

    private string DbConnString { get; set; }
    public MySQLStorage(string ip, string port, string user, string password, string database, string table)
    {
        DbConnString = $"server={ip};port={port};user={user};password={password};database={database};Pooling=true;MinimumPoolSize=0;MaximumPoolsize=640;ConnectionIdleTimeout=30;AllowUserVariables=true";
        Table = table;
        var conn = new MySqlConnection(DbConnString);
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

        // meshgroup preference
        conn.Execute($"""
            CREATE TABLE IF NOT EXISTS `{table}_meshgrouppreferences` (
                `id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
                `steamid` BIGINT UNSIGNED NOT NULL,
                `model` VARCHAR(255) NOT NULL,
                `meshgroup` INT UNSIGNED NOT NULL,
                INDEX `steamid` (`steamid`),
                INDEX `model` (`model`)
            );
        """);
    }

    public async Task<MySqlConnection> ConnectAsync()
    {
        MySqlConnection connection = new(DbConnString);
        await connection.OpenAsync();
        return connection;
    }

    public void ExecuteAsync(string query, object? parameters)
    {
        Task.Run(async () =>
        {
            using MySqlConnection connection = await ConnectAsync();
            await connection.ExecuteAsync(query, parameters);
        });
    }

    public dynamic? GetPlayerModel(ulong SteamID, string modelfield)
    {
        using MySqlConnection connection = ConnectAsync().Result;
        var result = connection.QueryFirstOrDefault($"SELECT `{modelfield}` FROM `{Table}` WHERE `steamid` = {SteamID};");
        return result;
    }

    public string? GetPlayerTModel(ulong SteamID)
    {
        var result = GetPlayerModel(SteamID, "t_model");
        if (result == null)
        {
            return null;
        }
        return result!.t_model;
    }
    public string? GetPlayerCTModel(ulong SteamID)
    {
        var result = GetPlayerModel(SteamID, "ct_model");
        if (result == null)
        {
            return null;
        }
        return result!.ct_model;
    }

    public void SetPlayerModel(ulong SteamID, string modelIndex, string modelfield, bool permissionBypass, string side)
    {

        var sql = $"""
            INSERT INTO {Table} (`steamid`, `{modelfield}`, `{side}_permission_bypass`) VALUES ({SteamID}, @model, @permissionBypass) ON DUPLICATE key UPDATE `{modelfield}` = @model, `{side}_permission_bypass`=@permissionBypass;
            """;
        ExecuteAsync(sql,
            new
            {
                model = modelIndex,
                permissionBypass
            }
        );

    }
    public void SetPlayerTModel(ulong SteamID, string modelIndex, bool permissionBypass)
    {
        SetPlayerModel(SteamID, modelIndex, "t_model", permissionBypass, "t");
    }
    public void SetPlayerCTModel(ulong SteamID, string modelIndex, bool permissionBypass)
    {
        SetPlayerModel(SteamID, modelIndex, "ct_model", permissionBypass, "ct");
    }
    public void SetAllTModel(string tmodel, bool permissionBypass)
    {
        ExecuteAsync(@$"
                UPDATE `{Table}` SET `t_model` = @tmodel, `t_permission_bypass`=@permissionBypass",
                new
                {
                    tmodel,
                    permissionBypass
                }
        );
    }
    public void SetAllCTModel(string ctmodel, bool permissionBypass)
    {
        ExecuteAsync(@$"
            UPDATE `{Table}` SET `ct_model` = @ctmodel, `ct_permission_bypass`=@permissionBypass",
            new
            {
                ctmodel,
                permissionBypass
            }
        );
    }

    public List<int> GetMeshgroupPreference(ulong SteamID, string modelIndex)
    {

        using MySqlConnection connection = ConnectAsync().Result;
        return connection.Query<int>($"SELECT `meshgroup` FROM `{Table}_meshgrouppreferences` WHERE `steamid` = {SteamID} AND `model` = '{modelIndex}';").ToList();
    }


    public void AddMeshgroupPreference(ulong SteamID, string modelIndex, int meshgroup)
    {
        Task.Run(async () =>
        {
            using MySqlConnection connection = await ConnectAsync();
            var existing = connection.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM `{Table}_meshgrouppreferences` WHERE `steamid` = {SteamID} AND `model` = '{modelIndex}' AND `meshgroup` = {meshgroup};");
            if (existing == 0)
            {
                await connection.ExecuteAsync($"INSERT INTO `{Table}_meshgrouppreferences` (`steamid`, `model`, `meshgroup`) VALUES ({SteamID}, '{modelIndex}', {meshgroup});");
            }
        });
    }

    public void RemoveMeshgroupPreference(ulong SteamID, string modelIndex, int meshgroup)
    {
        ExecuteAsync($"DELETE FROM `{Table}_meshgrouppreferences` WHERE `steamid` = {SteamID} AND `model` = '{modelIndex}' AND `meshgroup` = {meshgroup};", null);
    }
    class MeshgroupPreferenceRow
    {
        public ulong steamid { get; set; }
        public required string model { get; set; }
        public int meshgroup { get; set; }

    }
    public Tuple<List<ModelCache>, List<MeshgroupPreferenceCache>> GetCaches()
    {
        using MySqlConnection connection = ConnectAsync().Result;
        List<ModelCache> modelCache = connection.Query<ModelCache>($"select * from {Table};").ToList();
        var result = connection.Query<dynamic>($"SELECT * FROM `{Table}_meshgrouppreferences`");
        List<MeshgroupPreferenceCache> meshgroupPreferenceCaches = new();
        foreach (var query in result)
        {
            MeshgroupPreferenceCache? cache = meshgroupPreferenceCaches.Find(meshgroupPreference => meshgroupPreference.steamid == query.steamid && meshgroupPreference.model == query.model);
            if (cache == null)
            {
                cache = new MeshgroupPreferenceCache { steamid = query.steamid, model = query.model };
                meshgroupPreferenceCaches.Add(cache);
            }
            cache.meshgroups.Add((int)query.meshgroup);
        }
        return new(modelCache, meshgroupPreferenceCaches);
    }
}
