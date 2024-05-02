using CounterStrikeSharp.API.Core;
using PlayerModelChanger;
using Storage;

namespace Service;

public class ModelCache {

    public ulong steamid { get; set; }
    public string? t_model { get; set; }
    public string? ct_model { get; set; }
    public bool t_permission_bypass { get; set; }
    public bool ct_permission_bypass { get; set; }
}

public class ModelCacheManager {
    private List<ModelCache> cache = new List<ModelCache>();

    private IStorage storage;

    public ModelCacheManager(IStorage storage) {
        this.storage = storage; 
    }
    public void ResyncCache() {
        cache = storage.GetAllPlayerModel();
    }

    public void SetAllTModels(string tmodel, bool permissionBypass) {
        cache.ForEach(model => {model.t_model = tmodel; model.t_permission_bypass = permissionBypass;});
    }
    public void SetAllCTModels(string ctmodel, bool permissionBypass) {
        cache.ForEach(model => {model.ct_model = ctmodel; model.ct_permission_bypass = permissionBypass;});
    }
    public void SetAllModels(string tmodel, string ctmodel, bool permissionBypass) {
        cache.ForEach(model => {model.t_model = tmodel; model.ct_model = ctmodel; model.t_permission_bypass = permissionBypass; model.ct_permission_bypass = permissionBypass;});
    }
    public List<ulong> GetAllPlayers() {
        return cache.Select(model => model.steamid).ToList();
    }
    public void SetPlayerModel(ulong steamid, string modelIndex, string side, bool permissionBypass) {
        var obj = cache.Find(model => model.steamid == steamid);

        if (obj == null) {
            var modelcache = new ModelCache
            {
                steamid = steamid
            };
            cache.Add(modelcache);
            obj = modelcache;
        }
        Utils.ExecuteSide(side,
            null,
            () => {obj.t_model = modelIndex; obj.t_permission_bypass = permissionBypass;},
            () => {obj.ct_model = modelIndex; obj.ct_permission_bypass = permissionBypass;}
        );
       
    }
    public ModelCache? GetPlayerModelCache(CCSPlayerController player) {
        return cache.Find(model => model.steamid == player!.AuthorizedSteamID!.SteamId64);
    }
}