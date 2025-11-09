using Google.Protobuf;
using IGrains;
using IGrains.GrainState;
using IGrains.Handler;
using LaunchPB;
using Orleans;
using BattleSlot = LaunchPB.BattleSlot;

namespace Grains.Handler
{
    public class GetPlayerInfoHandler : IMessageHandler
    {
        public async Task<NetPackage> HandlePlayerMessage(NetPackage netPackage, string openId, IGrainFactory grainFactory)
        {
            IUserGrain userGrain = grainFactory.GetGrain<IUserGrain>(openId);

            UserInfo userInfo = await userGrain.GetUserInfoAsync();

            var resp = new GetPlayerInfoResp()
            {
                UserId = userInfo.UsertId,
                OpenId = userInfo.OpenId,
                UserName = userInfo.Name,
                Level = userInfo.Level,
                Exp = userInfo.Exp,
                Gold = userInfo.Gold,
                Diamond = userInfo.Diamond,
            };
            
            resp.Deck.AddRange(userInfo.Deck.Select(slot => new LaunchPB.BattleSlot
            {
                Position = slot.position,
                HeroCd = slot.heroCd
            }));

            return new NetPackage
            {
                protoID = (int)ProtoCode.EGetPlayerInfoResp,
                bodyData = resp.ToByteArray()
            };

        }
    }
}
