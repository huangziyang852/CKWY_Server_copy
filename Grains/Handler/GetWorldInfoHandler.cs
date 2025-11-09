using Google.Protobuf;
using IGrains;
using IGrains.GrainState;
using IGrains.Handler;
using LaunchPB;
using Orleans;

namespace Grains.Handler;

public class GetWorldInfoHandler:IMessageHandler
{
    public async Task<NetPackage> HandlePlayerMessage(NetPackage netPackage, string openId, IGrainFactory grainFactory)
    {
        IWorldGrain  worldGrain = grainFactory.GetGrain<IWorldGrain>(openId);

        WorldInfo worldInfo = await worldGrain.GetWorldInfoAsync();
        
        return new NetPackage
        {
            protoID = (int)ProtoCode.EGetWorldInfoResp,
            bodyData = ConvertToProto(worldInfo).ToByteArray()
        };
    }

    private static GetWorldInfoResp ConvertToProto(WorldInfo worldInfo)
    {
        return new GetWorldInfoResp
        {
            Worlds = {worldInfo.worlds.Select(world=>new World{WorldId = world.worldId})},
            Stages = {worldInfo.stages.Select(stage => new Stage{StageId = stage.stageId})}
        };
    }
}