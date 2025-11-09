using IGrains.Models;

namespace IGrains.GrainState;

public class ItemInfo
{
    public Dictionary<int,int> Items { get; set; } = new Dictionary<int, int>();
}