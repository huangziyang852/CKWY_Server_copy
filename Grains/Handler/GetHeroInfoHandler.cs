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
using Hero = LaunchPB.Hero;

namespace Grains.Handler
{
    public class GetHeroInfoHandler : IMessageHandler
    {
        public async Task<NetPackage> HandlePlayerMessage(NetPackage netPackage, string openId, IGrainFactory grainFactory)
        {
            IHeroGrain heroGrain = grainFactory.GetGrain<IHeroGrain>(openId);

            HeroInfo heroInfo = await heroGrain.GetHeroInfoAsync();

            return new NetPackage
            {
                protoID = (int)ProtoCode.EGetHeroInfoResp,
                bodyData = ConvertToProto(heroInfo).ToByteArray()
            };
        }

        private static GetHeroInfoResp ConvertToProto(HeroInfo heroInfo)
        {
            return new GetHeroInfoResp
            {
                Heroes = { heroInfo.Heroes.Select(h => new Hero { HeroId = h.HeroId, Level = h.Level,HeroCd = h.HeroCd,Star = h.Star}) }
            };
        }
    }
}
