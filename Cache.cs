using CounterStrikeSharp.API.Core;

namespace PlayerModelChanger;

public class ModelCache
{

    public ulong steamid { get; set; }
    public string? t_model { get; set; }
    public string? ct_model { get; set; }
    public bool t_permission_bypass { get; set; }
    public bool ct_permission_bypass { get; set; }
}

public class MeshgroupPreferenceCache
{
    public ulong steamid { get; set; }
    public required string model { get; set; }
    public List<int> meshgroups { get; set; } = new();

}

public class ModelCacheManager
{
    private List<ModelCache> _Cache = new List<ModelCache>();
    private List<MeshgroupPreferenceCache> _MeshgroupPreferenceCache = new List<MeshgroupPreferenceCache>();
    private IStorage _Storage;
    public ModelCacheManager(IStorage storage)
    {
        this._Storage = storage;
    }
    public void ResyncCache()
    {
        var data = _Storage.GetCaches();
        _Cache = data.Item1;
        _MeshgroupPreferenceCache = data.Item2;
    }

    public void SetAllTModels(string tmodel, bool permissionBypass)
    {
        _Cache.ForEach(model => { model.t_model = tmodel; model.t_permission_bypass = permissionBypass; });
    }
    public void SetAllCTModels(string ctmodel, bool permissionBypass)
    {
        _Cache.ForEach(model => { model.ct_model = ctmodel; model.ct_permission_bypass = permissionBypass; });
    }
    public void SetAllModels(string tmodel, string ctmodel, bool permissionBypass)
    {
        _Cache.ForEach(model => { model.t_model = tmodel; model.ct_model = ctmodel; model.t_permission_bypass = permissionBypass; model.ct_permission_bypass = permissionBypass; });
    }
    public List<ulong> GetAllPlayers()
    {
        return _Cache.Select(model => model.steamid).ToList();
    }
    public void SetPlayerModel(ulong steamid, string modelIndex, Side side, bool permissionBypass)
    {
        var obj = _Cache.Find(model => model.steamid == steamid);

        if (obj == null)
        {
            var modelcache = new ModelCache
            {
                steamid = steamid
            };
            _Cache.Add(modelcache);
            obj = modelcache;
        }
        Utils.ExecuteSide(side,
            null,
            () => { obj.t_model = modelIndex; obj.t_permission_bypass = permissionBypass; },
            () => { obj.ct_model = modelIndex; obj.ct_permission_bypass = permissionBypass; }
        );

    }
    public ModelCache? GetPlayerModelCache(CCSPlayerController player)
    {
        return _Cache.Find(model => model.steamid == player!.AuthorizedSteamID!.SteamId64);
    }

    public void AddMeshgroupPreference(ulong steamid, string modelIndex, int meshgroup)
    {
        var obj = _MeshgroupPreferenceCache.Find(model => model.steamid == steamid && model.model == modelIndex);
        if (obj == null)
        {
            obj = new MeshgroupPreferenceCache
            {
                steamid = steamid,
                model = modelIndex,
                meshgroups = new List<int>()
            };
            _MeshgroupPreferenceCache.Add(obj);
        }
        obj.meshgroups.Add(meshgroup);
    }

    public List<int> GetMeshgroupPreference(ulong steamid, string modelIndex)
    {
        var obj = _MeshgroupPreferenceCache.Find(model => model.steamid == steamid && model.model == modelIndex);
        if (obj == null)
        {
            return new List<int>();
        }
        return obj.meshgroups;
    }

    public void RemoveMeshgroupPreference(ulong steamid, string modelIndex, int meshgroup)
    {
        var obj = _MeshgroupPreferenceCache.Find(model => model.steamid == steamid && model.model == modelIndex);
        if (obj == null)
        {
            return;
        }
        obj.meshgroups.Remove(meshgroup);
    }


}
