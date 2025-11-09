using Common;
using IGrains;
using IGrains.GrainState;
using Orleans;
using Orleans.Providers;

namespace Grains;

[StorageProvider(ProviderName = "MongoDBStore")]
public class WorldGrain:Grain<WorldInfo>,IWorldGrain
{
    public override async Task OnActivateAsync()
    {
        cfg.battle.BattleMap battleMaps = TableLoader.Instance.MasterTables.TbBattleMap.Get(1);
        if (State == null || State.worlds.Count == 0)
        {
            AddWorld(1);
        }
    }
    
    public Task<WorldInfo> GetWorldInfoAsync()
    {
        return Task.FromResult(State); 
    }

    private void AddWorld(int worldId)
    {
        State.worlds.Add(new WorldInfo.World(worldId));
    }
    
    public Task DeactivateAsync()
    {
        Logger.Instance.Information($"WorldGrain '{this.GetPrimaryKeyString()}' Deactivate.");
        DeactivateOnIdle();
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// 当Grain被停用时，确保状态持久化
    /// </summary>
    public override async Task OnDeactivateAsync()
    {
        await WriteStateAsync();
        await base.OnDeactivateAsync();
    }
}