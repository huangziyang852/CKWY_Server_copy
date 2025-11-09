using Google.Protobuf;
using IGrains;
using IGrains.Handler;
using LaunchPB;
using Orleans;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grains.Handler
{
    public class HeartBeatHandler
    {
        // 每接收到心跳包就更新客户端最后一次心跳时间
        public async Task Handle(NetPackage netPackage, string openId, IGrainFactory grainFactory)
        {
            HeartBeat heartBeat = HeartBeat.Parser.ParseFrom(netPackage.bodyData);

            IUserManagerGrain userManagerGrain = grainFactory.GetGrain<IUserManagerGrain>("SingleUserManagerGrain");

            await userManagerGrain.RefreshHeartBeat(openId);
        }
    }
}
