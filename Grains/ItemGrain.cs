using IGrains;
using IGrains.GrainState;
using Orleans;
using Orleans.Providers;
using Item = IGrains.Models.Item;

namespace Grains;

[StorageProvider(ProviderName = "MongoDBStore")]
public class ItemGrain: Grain<ItemInfo>, IItemGrain
{
    public Task<ItemInfo> GetItemInfoAsync()
    {
        return Task.FromResult(State);
    }

    /// <summary>
    /// 添加道具
    /// </summary>
    /// <param name="itemList"></param>
    /// <returns></returns>
    public async Task AddItemAsync(List<Item> itemList)
    {
        if (State == null || State.Items.Count == 0)
        {
            State = new ItemInfo
            {
                Items = new Dictionary<int, int>()
            };
        }
        foreach (var item in itemList)
        {
            if (item.Count <= 0)
                continue;

            if (State.Items.TryGetValue(item.ItemId, out int existingCount))
            {
                State.Items[item.ItemId] = existingCount + item.Count;
            }
            else
            {
                State.Items[item.ItemId] = item.Count;
            }
        }
        await WriteStateAsync();
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