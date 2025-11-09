using System.Text;
using Common;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Orleans;

namespace GateServer.Net
{
    public class WebSocketHandshakeHandler:SimpleChannelInboundHandler<IByteBuffer>
    {
        protected override void ChannelRead0(IChannelHandlerContext ctx, IByteBuffer msg)
        {
            int readable = msg.ReadableBytes;
            byte[] data = new byte[readable];
            msg.ReadBytes(data);

            string request = Encoding.UTF8.GetString(data);

            // 检查是否是 WebSocket Upgrade 请求
            if (request.StartsWith("GET") && request.Contains("Upgrade: websocket"))
            {
                string key = null;
                foreach (var line in request.Split("\r\n"))
                {
                    if (line.StartsWith("Sec-WebSocket-Key:"))
                    {
                        key = line.Substring("Sec-WebSocket-Key:".Length).Trim();
                        break;
                    }
                }

                if (key == null)
                {
                    ctx.CloseAsync();
                    return;
                }

                // 生成 Sec-WebSocket-Accept
                string acceptKey = ComputeWebSocketAcceptKey(key);

                // 构造响应
                string response = "HTTP/1.1 101 Switching Protocols\r\n" +
                                  "Upgrade: websocket\r\n" +
                                  "Connection: Upgrade\r\n" +
                                  $"Sec-WebSocket-Accept: {acceptKey}\r\n\r\n";

                IByteBuffer buffer = Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes(response));
                ctx.WriteAndFlushAsync(buffer);

                Logger.Instance.Information($"{ctx.Channel.RemoteAddress} WebSocket 握手成功!");

                // 握手完成后，把自己从 pipeline 移除
                ctx.Channel.Pipeline.Remove(this);
                Logger.Instance.Information(ctx.Channel.Pipeline.ToString());
            }
            else
            {
                // 非 WebSocket 请求直接关闭
                Logger.Instance.Information($"{ctx.Channel.RemoteAddress} 接收到了非WebSocket请求，关闭链接!");
                ctx.CloseAsync();
            }
        }

        private string ComputeWebSocketAcceptKey(string key)
        {
            string concat = key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            byte[] hash = System.Security.Cryptography.SHA1.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(concat));
            return Convert.ToBase64String(hash);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Logger.Instance.Error($"{context.Channel.RemoteAddress} 握手异常: {exception}");
            context.CloseAsync();
        }
    }
}