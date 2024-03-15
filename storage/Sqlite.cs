using Microsoft.Data.Sqlite;
using Dapper;
using Service;
namespace Storage;

public class SqliteStorage : IStorage {

    private SqliteConnection conn { get; set; }
    public SqliteStorage(string ModuleDirectory) {

        conn = new SqliteConnection($"Data Source={Path.Join(ModuleDirectory, "data.db")}");
        conn.Open();

        Task.Run( async () => {
            await conn.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS `players` (
                    `steamid` UNSIGNED BIG INT NOT NULL,
                    `t_model` TEXT,
                    `ct_model` TEXT,
                    PRIMARY KEY (`steamid`));
                )
            ");
        });
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

    public void SetPlayerModel(ulong SteamID, string model, string modelfield) {

        Task.Run( async () => {
            await conn.ExecuteAsync(@$"
                INSERT INTO `players` (`steamid`, `{modelfield}`) VALUES (@SteamId, @Model)
                ON CONFLICT(`steamid`) DO UPDATE SET `{modelfield}` = @Model;",
                new {
                    SteamId = SteamID,
                    Model = model
                }
            );
        });
        
    }
    public void SetPlayerTModel(ulong SteamID, string model) {
        SetPlayerModel(SteamID, model, "t_model");
    }
    public void SetPlayerCTModel(ulong SteamID, string model) {
        SetPlayerModel(SteamID, model, "ct_model");
    }
    public void SetPlayerAllModel(ulong SteamID, string tmodel, string ctmodel) {
         Task.Run( async () => {
            await conn.ExecuteAsync(@$"
                INSERT INTO `players` (`steamid`, `t_model`, `ct_model`) VALUES (@SteamId, @TModel, @CTModel)
                ON CONFLICT(`steamid`) DO UPDATE SET `t_model` = @TModel, `ct_model` = @CTModel;",
                new {
                    SteamId = SteamID,
                    TModel = tmodel,
                    CTModel = ctmodel,
                }
            );
        });
    }
    public void SetAllTModel(string tmodel) {
        Task.Run( async () => {
            await conn.ExecuteAsync(@$"
                UPDATE `players` SET `t_model` = @TModel",
                new {
                    TModel = tmodel
                }
            );
        });
    }
    public void SetAllCTModel(string ctmodel) {
        Task.Run( async () => {
            await conn.ExecuteAsync(@$"
                UPDATE `players` SET `ct_model` = @CTModel",
                new {
                    CTModel = ctmodel
                }
            );
        });
    }
    public void SetAllModel(string tmodel, string ctmodel) {
        Task.Run( async () => {
            await conn.ExecuteAsync(@$"
                UPDATE `players` SET `t_model` = @TModel, `ct_model` = @CTModel;",
                new {
                    TModel = tmodel,
                    CTModel = ctmodel,
                }
            );
        });
    }
}