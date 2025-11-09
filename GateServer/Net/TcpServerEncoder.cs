using Common;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using IGrains.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServer.Net
{
    /// <summary>
    /// 解码器
    /// </summary>
    public class TcpServerEncoder:MessageToByteEncoder<NetPackage>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">表示通道（Channel）中某个具体处理器（ChannelHandler）的上下文对象</param>
        /// <param name="message">消息包</param>
        /// <param name="output">字节流</param>
        protected override void Encode(IChannelHandlerContext context, NetPackage netPackage, IByteBuffer output)
        {
            output.WriteInt(netPackage.bodyData.Length);
            output.WriteInt(netPackage.protoID);
            output.WriteBytes(netPackage.bodyData);
            Logger.Instance.Information($"{context.Channel.RemoteAddress.ToString()} 发送协议{netPackage.protoID} 数据!");
        }
    }
}
