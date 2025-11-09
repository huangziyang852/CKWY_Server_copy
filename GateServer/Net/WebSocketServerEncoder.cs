using System.Net;
using Common;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using IGrains.Handler;

namespace GateServer.Net
{
    /// <summary>
    /// 将 NetPackage 编码为 WebSocket 二进制帧
    /// </summary>
    public class WebSocketServerEncoder:MessageToByteEncoder<NetPackage>
    {
        protected override void Encode(IChannelHandlerContext context, NetPackage netPackage, IByteBuffer output)
        {
            try
            {
                // ======== 1. 构造 payload（大端） ========
                byte[] bodyLenBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(netPackage.bodyData.Length));
                byte[] protoIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(netPackage.protoID));
                byte[] payload = new byte[bodyLenBytes.Length + protoIDBytes.Length + netPackage.bodyData.Length];
                Logger.Instance.Information($"bodyData length = {netPackage.bodyData.Length}");

                Array.Copy(bodyLenBytes, 0, payload, 0, bodyLenBytes.Length);
                Array.Copy(protoIDBytes, 0, payload, bodyLenBytes.Length, protoIDBytes.Length);
                Array.Copy(netPackage.bodyData, 0, payload, bodyLenBytes.Length + protoIDBytes.Length, netPackage.bodyData.Length);

                int payloadLen = payload.Length;
                
                // ✅ 输出 payload 的十六进制查看（核心调试点）
                string payloadHex = BitConverter.ToString(payload).Replace("-", " ");
                Logger.Instance.Information($"[WebSocket Encoder] protoID={netPackage.protoID}, bodyLen={netPackage.bodyData.Length}, payloadLen={payloadLen}");
                Logger.Instance.Information($"[Payload HEX] {payloadHex}");

                // ======== 2. WebSocket 帧头 ========
                // FIN + binary opcode
                output.WriteByte(0x82);

                if (payloadLen <= 125)
                {
                    output.WriteByte((byte)payloadLen); // 服务器发给客户端不用掩码
                }
                else if (payloadLen <= ushort.MaxValue)
                {
                    output.WriteByte(126);
                    //output.WriteShort(IPAddress.HostToNetworkOrder((short)payloadLen)); // 大端
                    output.WriteShort((short)payloadLen);
                }
                else
                {
                    output.WriteByte(127);
                    //output.WriteLong(IPAddress.HostToNetworkOrder((long)payloadLen)); // 大端
                    output.WriteLong((long)payloadLen); 
                }

                // ======== 3. 写入 payload ========
                output.WriteBytes(payload);
                Logger.Instance.Information(context.Channel.Pipeline.ToString());

                Logger.Instance.Information($"{context.Channel.RemoteAddress} 发送协议 {netPackage.protoID} 数据, 长度 {payloadLen}");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("WebSocket Encoder 异常: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}