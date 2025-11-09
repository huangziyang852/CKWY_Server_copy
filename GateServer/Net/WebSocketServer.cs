using System.Security.Cryptography.X509Certificates;
using Common;
using DotNetty.Buffers;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Orleans;

namespace GateServer.Net
{
    public class WebSocketServer
    {
        private IEventLoopGroup bossGroup;
        private IEventLoopGroup workerGroup;
        private IChannel serverChannel;
        private readonly IClusterClient client;
        
        public WebSocketServer(IClusterClient client)
        {
            this.client = client;
        }

        public async Task StartAsync()
        {
            //主工作组
            bossGroup = new MultithreadEventLoopGroup(1);
            //子工作组 cpu核数*2
            workerGroup = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new ServerBootstrap();
                //设置线程组模型为：主从模型
                bootstrap.Group(bossGroup, workerGroup);
                //设置通道类型
                bootstrap.Channel<TcpServerSocketChannel>();

                bootstrap
                    //半连接队列的元素上限 也就是说已经再操作系统层面完成了3次握手，等待当前进程取走的操作系统中的链路的个数
                    .Option(ChannelOption.SoBacklog, 4096)
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildOption(ChannelOption.SoKeepalive, true)
                    .ChildOption(ChannelOption.TcpNodelay, true)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        
                        //心跳监测暂时不需要
                        //WebSocket 需要先经过 HTTP 握手
                        pipeline.AddLast(new WebSocketHandshakeHandler());
                        //自定义解码器
                        pipeline.AddLast(new WebSocketServerDecoder());
                        // WebSocket 数据帧编码
                        pipeline.AddLast(new WebSocketServerEncoder());
                        //分发网络包
                        pipeline.AddLast(new WebSocketServerHandler(client));

                    }));
                
                serverChannel = await bootstrap.BindAsync(8898);

                Logger.Instance.Information("WebSocket网关服务器启动成功! 监听端口: 8898");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());

                throw new Exception("启动 WebSocketServer 失败! \n" + ex.StackTrace);
            }
        }
        
        public async Task StopAsync()
        {
            try
            {
                await serverChannel.CloseAsync();
            }
            finally
            {
                await Task.WhenAll(
                    bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))
                );

                Logger.Instance.Information("WebSocket 服务器关闭成功!");
            }
        }
    }
}