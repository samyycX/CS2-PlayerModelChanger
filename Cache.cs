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

public class ModelCacheManager
{
    private List<ModelCache> _Cache = new List<ModelCache>();
    private IStorage _Storage;
    public ModelCacheManager(IStorage storage)
    {
        this._Storage = storage;
    }
    public void ResyncCache()
    {
        var data = _Storage.GetAllPlayerModel();
        if (data != null)
        {
            _Cache = data;
        }
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
    public void SetPlayerModel(ulong steamid, string modelIndex, string side, bool permissionBypass)
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
}
