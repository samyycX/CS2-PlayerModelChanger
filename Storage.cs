namespace Storage;

public interface IStorage {
    public string GetPlayerTModel(ulong SteamID);
    public string GetPlayerCTModel(ulong SteamID);

    public void SetPlayerTModel(ulong SteamID, string modelName);

    public void SetPlayerCTModel(ulong SteamID, string modelName);
    
}