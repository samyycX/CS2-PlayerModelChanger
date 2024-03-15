using Service;

namespace Storage;

public interface IStorage {
    public string? GetPlayerTModel(ulong SteamID);
    public string? GetPlayerCTModel(ulong SteamID);
    public List<ModelCache> GetAllPlayerModel();
    public void SetPlayerTModel(ulong SteamID, string modelName);

    public void SetPlayerCTModel(ulong SteamID, string modelName);

    public void SetPlayerAllModel(ulong SteamID, string tModel, string ctModel);

    public void SetAllTModel(string tmodel);
    public void SetAllCTModel(string ctmodel);
    public void SetAllModel(string tmodel, string ctmodel);
}