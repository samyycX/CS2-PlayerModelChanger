namespace PlayerModelChanger;

public interface IStorage
{
    public string? GetPlayerTModel(ulong SteamID);
    public string? GetPlayerCTModel(ulong SteamID);
    public void SetPlayerTModel(ulong SteamID, string modelIndex, bool permissionBypass);
    public void SetPlayerCTModel(ulong SteamID, string modelIndex, bool permissionBypass);
    public void SetAllTModel(string tmodel, bool permissionBypass);
    public void SetAllCTModel(string ctmodel, bool permissionBypass);

    public List<int> GetMeshgroupPreference(ulong SteamID, string modelIndex);
    public void AddMeshgroupPreference(ulong SteamID, string modelIndex, int meshgroup);
    public void RemoveMeshgroupPreference(ulong SteamID, string modelIndex, int meshgroup);
    public Tuple<List<ModelCache>, List<MeshgroupPreferenceCache>> GetCaches();
}
