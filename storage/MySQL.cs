using Dapper;
using MySqlConnector;
using Service;
using Storage;

namespace Storage;
public class MySQLStorage : IStorage {

    private MySqlConnection conn;

    private string table;
    public MySQLStorage(string ip, string port, string user, string password, string database, string table) {
        string connectStr = $"server={ip};port={port};user={user};password={password};database={database};";
        this.table = table;
        conn = new MySqlConnection(connectStr);
        conn.Execute($"""
            CREATE TABLE IF NOT EXISTS `{table}` (
                `steamid` BIGINT UNSIGNED NOT NULL PRIMARY KEY,
                `t_model` TEXT,
                `ct_model` TEXT
            );
        """);
    }
    public List<ModelCache> GetAllPlayerModel() {
        return conn.Query<ModelCache>($"select * from {table};").ToList();
    }

    public dynamic? GetPlayerModel(ulong SteamID, string modelfield)
    {
        var result = conn.QueryFirstOrDefault($"SELECT `{modelfield}` FROM `{table}` WHERE `steamid` = {SteamID};");
        return result;
    }

    public string GetPlayerTModel(ulong SteamID)
    {
        var result = GetPlayerModel(SteamID, "t_model");
        if (result == null) {
            return "";
        }
        return result?.t_model ?? "";
    }
    public string GetPlayerCTModel(ulong SteamID)
    {
        var result = GetPlayerModel(SteamID, "ct_model");
        if (result == null) {
            return "";
        }
        return result?.ct_model ?? "";
    }

    public void SetPlayerModel(ulong SteamID, string modelName, string modelfield)
    {
        
        var sql = $"""
            INSERT INTO {table} (`steamid`, `{modelfield}`) VALUES ({SteamID}, @Model) ON DUPLICATE key UPDATE `{modelfield}` = @Model;
            """;
        Task.Run( async () => {
            await conn.ExecuteAsync(sql,
                new {
                    Model = modelName
                }
            );
        });
       
    }
    public void SetPlayerTModel(ulong SteamID, string modelName) {
        SetPlayerModel(SteamID, modelName, "t_model");
    }
    public void SetPlayerCTModel(ulong SteamID, string modelName) {
        SetPlayerModel(SteamID, modelName, "ct_model");
    }
}