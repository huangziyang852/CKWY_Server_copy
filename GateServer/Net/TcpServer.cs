using Common;
using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GateServer.Net
{
    public class TcpServer
    {

        private  IEventLoopGroup bossGroup;

        private  IEventLoopGroup workerGroup;

        private  IChannel bootstrapChannel;

        private readonly IClusterClient client;

        public TcpServer(IClusterClient client)
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
                ServerBootstrap bootstrap = new ServerBootstrap();
                //设置线程组模型为：主从模型
                bootstrap.Group(bossGroup, workerGroup);
                //设置通道类型
                bootstrap.Channel<TcpServerSocketChannel>();

                bootstrap
                    //半连接队列的元素上限 也就是说已经再操作系统层面完成了3次握手，等待当前进程取走的操作系统中的链路的个数
                    .Option(ChannelOption.SoBacklog, 4096)
                    //用于设置Channel接收字节流时的缓冲区大小
                    .Option(ChannelOption.RcvbufAllocator, new AdaptiveRecvByteBufAllocator())
                    //用于设置重用缓冲区
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    //保持长连接
                    .ChildOption(ChannelOption.SoKeepalive, true)
                    //取消延迟发送 也就是关闭Nagle算法(操作系统层面不一定会立即发送)
                    .ChildOption(ChannelOption.TcpNodelay, true)
                    //用于对单个通道的处理
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast("IdleChecker",new IdleStateHandler(50,50,0));
                        pipeline.AddLast(new TcpServerEncoder(), new TcpServerDecoder(), new TcpServerHandler(client));
                    }));
                bootstrapChannel = await bootstrap.BindAsync(8899);

                Logger.Instance.Information($"启动tcp网关服务器成功!监听端口号：{8899}");

            }catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());

                throw new Exception("启动 TcpServer 失败! \n" + ex.StackTrace);
            }
        }

        public async Task StopAsync()
        {
            try
            {
                await bootstrapChannel.CloseAsync();
            }
            finally
            {
                await Task.WhenAll(
                    bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(100)),
                    workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(100)));

                Logger.Instance.Information("关闭网关服务器成功!");
            }
        }
    }
}
