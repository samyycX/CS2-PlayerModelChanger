using Microsoft.Data.Sqlite;
using Dapper;
namespace Data;

public class PlayerData {

    private SqliteConnection _connection { get; set; }
    public void initialize(string ModuleDirectory) {
        if (_connection != null) {
            return;
        }

        _connection = new SqliteConnection($"Data Source={Path.Join(ModuleDirectory, "data.db")}");
        _connection.Open();

        Task.Run( async () => {
            await _connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS `players` (
                    `steamid` UNSIGNED BIG INT NOT NULL,
                    `model` TEXT NOT NULL,
                    PRIMARY KEY (`steamid`));
                )
            ");
        });
    }

    public string GetPlayerModel(ulong SteamID) {

        var result = _connection.QueryFirstOrDefault(@"SELECT `model` FROM `players` WHERE `steamid` = @SteamId;", new { SteamId = SteamID });

        if (result == null) {
            return "";
        }
        return result?.model ?? "";
    }

    public void SetPlayerModel(ulong SteamID, string model) {

        Task.Run( async () => {
            await _connection.ExecuteAsync(@"
                INSERT INTO `players` (`steamid`, `model`) VALUES (@SteamId, @Model)
                ON CONFLICT(`steamid`) DO UPDATE SET `model` = @Model;",
                new {
                    SteamId = SteamID,
                    Model = model
                }
            );
        });
        
    }
}