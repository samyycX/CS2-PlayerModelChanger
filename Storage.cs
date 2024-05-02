using Service;

namespace Storage;

public interface IStorage {
    public string? GetPlayerTModel(ulong SteamID);
    public string? GetPlayerCTModel(ulong SteamID);
    public List<ModelCache> GetAllPlayerModel();
    public Task<int> SetPlayerTModel(ulong SteamID, string modelName, bool permissionBypass);

    public Task<int> SetPlayerCTModel(ulong SteamID, string modelName, bool permissionBypass);

    public Task<int> SetAllTModel(string tmodel, bool permissionBypass);
    public Task<int> SetAllCTModel(string ctmodel, bool permissionBypass);
}