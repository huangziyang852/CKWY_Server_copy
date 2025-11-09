using Google.Protobuf;
using IGrains;
using IGrains.GrainState;
using IGrains.Handler;
using LaunchPB;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGrains.Models;
using Hero = LaunchPB.Hero;
using Item = IGrains.Models.Item;

namespace Grains.Handler
{
    public class GachaHandler : IMessageHandler
    {
        public async Task<NetPackage> HandlePlayerMessage(NetPackage netPackage, string openId, IGrainFactory grainFactory)
        {
            GachaReq gachaReq = GachaReq.Parser.ParseFrom(netPackage.bodyData);

            IGachaGrain gachaGrain = grainFactory.GetGrain<IGachaGrain>(openId);

            GachaResult gachaResult = await gachaGrain.GachaExcuteAsync(gachaReq.GachaId,gachaReq.GachaTimes);

            //这里的转换，重新写一下
            return new NetPackage
            {
                protoID = (int)ProtoCode.EGachaResultResp,
                bodyData = ConvertToProto(gachaResult).ToByteArray()
            };
        }

        public static GachaResultResp ConvertToProto(GachaResult gachaResult)
        {

            GachaResultResp resp = new GachaResultResp();
            resp.GachaHeroResult.AddRange(gachaResult.HeroResult);
            foreach (var item in gachaResult.AddItem)
            {
                LaunchPB.Item pbItem = new LaunchPB.Item
                {
                    ItemId = item.ItemId,
                    ItemCount = item.Count
                };
                resp.AddItem.Add(pbItem);
            }
            foreach (var hero in gachaResult.AddHero)
            {
                Hero pbHero = new Hero()
                {
                    HeroId = hero.HeroId, Level = hero.Level,HeroCd = hero.HeroCd,Star = hero.Star
                };
                resp.AddHero.Add(pbHero);
            }
            return resp;
        }
    }

}
