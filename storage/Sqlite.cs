using Microsoft.Data.Sqlite;
using Dapper;

namespace PlayerModelChanger;

public class SqliteStorage : IStorage
{
    private string DbConnString { get; set; }
    public SqliteStorage(string ModuleDirectory)
    {

        DbConnString = $"Data Source={Path.Join(ModuleDirectory, "data.db")}";
        var connection = new SqliteConnection(DbConnString);
        connection.Open();

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS `players` (
                `steamid` INTEGER NOT NULL PRIMARY KEY,
                `t_model` TEXT,
                `ct_model` TEXT,
                `t_permission_bypass` INTEGER,
                `ct_permission_bypass` INTEGER
            );
        ");
        IEnumerable<dynamic> tPermissionBypassResult = connection.Query("select * from sqlite_master where name='players' and sql like '%t_permission_bypass%'");
        if (tPermissionBypassResult.Count() == 0)
        {
            connection.Execute("ALTER TABLE players ADD COLUMN `t_permission_bypass` BOOLEAN;");
        }
        IEnumerable<dynamic> ctPermissionBypassResult = connection.Query("select * from sqlite_master where name='players' and sql like '%ct_permission_bypass%'");
        if (ctPermissionBypassResult.Count() == 0)
        {
            connection.Execute("ALTER TABLE players ADD COLUMN `ct_permission_bypass` BOOLEAN;");
        }

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS `meshgrouppreferences` (
                `id` INTEGER PRIMARY KEY AUTOINCREMENT,
                `steamid` INTEGER NOT NULL,
                `model` TEXT NOT NULL,
                `meshgroup` INTEGER NOT NULL
            );
        ");
    }

    public async Task<SqliteConnection> ConnectAsync()
    {
        SqliteConnection connection = new(DbConnString);
        await connection.OpenAsync();
        return connection;
    }

    public void ExecuteAsync(string query, object? parameters)
    {
        Task.Run(async () =>
        {
            using SqliteConnection connection = await ConnectAsync();
            await connection.ExecuteAsync(query, parameters);
        });
    }

    public dynamic? GetPlayerModel(ulong SteamID, string modelfield)
    {
        using SqliteConnection connection = ConnectAsync().Result;
        return connection.QueryFirstOrDefault(@$"SELECT `{modelfield}` FROM `players` WHERE `steamid` = @SteamId;", new { SteamId = SteamID });
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

    public void SetPlayerModel(ulong SteamId, string model, string modelIndex, bool permissionBypass, string side)
    {

        ExecuteAsync(@$"
            INSERT INTO `players` (`steamid`, `{modelIndex}`, `{side}_permission_bypass`) VALUES (@SteamId, @model, @permissionBypass)
            ON CONFLICT(`steamid`) DO UPDATE SET `{modelIndex}` = @model, `{side}_permission_bypass`=@permissionBypass;",
            new
            {
                SteamId,
                model,
                permissionBypass
            }
        );

    }
    public void SetPlayerTModel(ulong SteamID, string model, bool permissionBypass)
    {
        SetPlayerModel(SteamID, model, "t_model", permissionBypass, "t");
    }
    public void SetPlayerCTModel(ulong SteamID, string model, bool permissionBypass)
    {
        SetPlayerModel(SteamID, model, "ct_model", permissionBypass, "ct");
    }
    public void SetAllTModel(string tmodel, bool permissionBypass)
    {
        ExecuteAsync(@$"
            UPDATE `players` SET `t_model` = @tmodel, `t_permission_bypass`=@permissionBypass;",
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
            UPDATE `players` SET `ct_model` = @ctmodel, `ct_permission_bypass`=@permissionBypass;",
            new
            {
                ctmodel,
                permissionBypass
            }
        );
    }

    public List<int> GetMeshgroupPreference(ulong SteamID, string modelIndex)
    {
        using SqliteConnection connection = ConnectAsync().Result;
        return connection.Query<int>("SELECT `meshgroup` FROM `meshgrouppreferences` WHERE `steamid`=@SteamID AND `model`=@modelName", new { SteamID, modelIndex }).ToList();
    }

    public void AddMeshgroupPreference(ulong SteamID, string modelIndex, int meshgroup)
    {
        using SqliteConnection connection = ConnectAsync().Result;
        var existing = connection.QueryFirstOrDefault<int>($"SELECT COUNT(*) FROM `meshgrouppreferences` WHERE `steamid` = {SteamID} AND `model` = '{modelIndex}' AND `meshgroup` = {meshgroup};");
        if (existing == 0)
        {
            connection.ExecuteAsync("INSERT INTO `meshgrouppreferences` (`steamid`, `model`, `meshgroup`) VALUES (@SteamID, @modelIndex, @meshgroup);", new { SteamID, modelIndex, meshgroup });
        }
    }

    public void RemoveMeshgroupPreference(ulong SteamID, string modelIndex, int meshgroup)
    {
        ExecuteAsync("DELETE FROM `meshgrouppreferences` WHERE `steamid`=@SteamID AND `model`=@modelIndex AND `meshgroup`=@meshgroup;", new { SteamID, modelIndex, meshgroup });
    }

    public Tuple<List<ModelCache>, List<MeshgroupPreferenceCache>> GetCaches()
    {
        using SqliteConnection connection = ConnectAsync().Result;
        List<ModelCache> modelCache = connection.Query<ModelCache>($"select * from players;").ToList();
        var result = connection.Query<dynamic>($"SELECT * FROM `meshgrouppreferences`");
        List<MeshgroupPreferenceCache> meshgroupPreferenceCaches = new();
        foreach (var query in result)
        {
            MeshgroupPreferenceCache? cache = meshgroupPreferenceCaches.Find(meshgroupPreference => meshgroupPreference.steamid == (ulong)query.steamid && meshgroupPreference.model == query.model);
            if (cache == null)
            {
                cache = new MeshgroupPreferenceCache { steamid = (ulong)query.steamid, model = query.model };
                meshgroupPreferenceCaches.Add(cache);
            }
            cache.meshgroups.Add((int)query.meshgroup);
        }
        return new(modelCache, meshgroupPreferenceCaches);
    }
}
