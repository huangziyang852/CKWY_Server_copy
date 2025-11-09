using Common;
using DotNetty.Transport.Channels;
using IGrains.Handler;
using Orleans;

namespace GateServer.Net
{
    public class WebSocketServerHandler:SimpleChannelInboundHandler<NetPackage>
    {
        private readonly IClusterClient client;
        private Session session;

        public WebSocketServerHandler(IClusterClient client)
        {
            this.client = client;
        }

        protected override async void ChannelRead0(IChannelHandlerContext context, NetPackage netPackage)
        {

            Logger.Instance.Information($"{context.Channel.RemoteAddress.ToString()},分发网络包{netPackage.protoID}！");
            await session.DispatchReceivePacket(netPackage);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);
            
            session = new Session(client, context);
            
            Logger.Instance.Information($"{context.Channel.RemoteAddress} WebSocket 链接成功！");
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
            
            session?.Disconnect();
            
            Logger.Instance.Information($"{context.Channel.RemoteAddress}WebSocket 链接断开!");
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            base.ExceptionCaught(context, exception);
            Logger.Instance.Error($"{context.Channel.RemoteAddress} 链接异常: {exception}");
            context.CloseAsync();
        }
    }
}