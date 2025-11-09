using IGrains.GrainState;
using Orleans;
using Item = IGrains.Models.Item;

namespace IGrains;

public interface IItemGrain:IGrainWithStringKey
{
    Task<ItemInfo> GetItemInfoAsync();
    
    Task AddItemAsync(List<Item> itemList);
}