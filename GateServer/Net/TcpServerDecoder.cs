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
    public class TcpServerDecoder : ByteToMessageDecoder
    {
        /// <summary>
        /// 字节流解码成TcpMessage
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input">字节流</param>
        /// <param name="output"></param>
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                //网络包的包头约定8个字节
                if (input.ReadableBytes < 8)
                {
                    return;
                }

                //获取包体的长度
                //dotnettty不用关心大小端，字节流会被自动转换为本机字节序
                int bodyLength = input.GetInt(input.ReaderIndex);

                if (bodyLength < 0 || bodyLength >(1024 * 8))
                {
                    //包体长度不合法
                    context.CloseAsync();

                    return;
                }

                //检查现在长度是不是一个完成的网络包
                if (input.ReadableBytes < (8 + bodyLength))
                {
                    //现在的长度不是完整的网络包
                    return;
                }

                //读取包头中记录包体长度的部分
                input.ReadInt();
                //读取协议号的部分,注意指针的移动
                int protoID = input.ReadInt();
                //读取包体
                byte[] bodyData = new byte[bodyLength];
                input.ReadBytes(bodyData);

                //包装成一个NetPackage对象

                NetPackage netPackage = new NetPackage()
                {
                    protoID = protoID,
                    bodyData = bodyData
                };

                output.Add(netPackage);

            }catch (Exception ex)
            {
                Logger.Instance.Error("解析数据异常," + ex.Message+"/n"+ex.StackTrace);
            }
        }
    }
}
