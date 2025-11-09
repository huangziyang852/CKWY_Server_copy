using Google.Protobuf;
using IGrains;
using IGrains.GrainState;
using IGrains.Handler;
using LaunchPB;
using Orleans;

namespace Grains.Handler;

public class GetItemInfoHandler: IMessageHandler
{
    public async Task<NetPackage> HandlePlayerMessage(NetPackage netPackage, string openId, IGrainFactory grainFactory)
    {
        IItemGrain itemGrain = grainFactory.GetGrain<IItemGrain>(openId);
        
        ItemInfo itemInfo = await itemGrain.GetItemInfoAsync();
        
        return new NetPackage
        {
            protoID = (int)ProtoCode.EGetItemInfoResp,
            bodyData = ConvertToProto(itemInfo).ToByteArray()
        };
    }
    
    private static GetItemInfoResp ConvertToProto(ItemInfo itemInfo)
    {
        var resp = new GetItemInfoResp();
        foreach (var kvp in itemInfo.Items)
        {
            resp.Items.Add(new Item { ItemId = kvp.Key, ItemCount = kvp.Value });
        }
        return resp;
    }
}