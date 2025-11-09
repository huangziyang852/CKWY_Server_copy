using DotNetty.Transport.Channels;
using IGrains;
using IGrains.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServer.Net
{
    public class PacketObserver:IPacketObserver
    {
        private readonly IChannelHandlerContext context;

        public PacketObserver(IChannelHandlerContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// 当网关收到来自游戏服的消息时调用
        /// </summary>
        /// <param name="netPackage"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void OnReceivePacket(NetPackage netPackage)
        {
            context.WriteAndFlushAsync(netPackage);
        }
    }
}
