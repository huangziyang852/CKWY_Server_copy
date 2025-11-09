using Common;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using IGrains;
using IGrains.Handler;
using LaunchPB;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServer.Net
{
    /// <summary>
    /// 网关服务器跟游戏客户端的一个链路Session对象
    /// </summary>
    public class Session
    {
        /// <summary>
        /// 代表这个链路的IChannelHandlerContext
        /// </summary>
        private IChannelHandlerContext context;

        /// <summary>
        /// 这个Session的观察者对象,Silo节点可以通过这个观察者对象向这个观察者对象发消息
        /// </summary>
        private PacketObserver packetObserver;

        /// <summary>
        /// 本进程(GateServer)向Silo节点发送数据包的目标Actor
        /// </summary>
        private IPacketRouterGrain routerGrain;

        /// <summary>
        /// 标记这个Session对象是否经过鉴权
        /// </summary>
        private bool isLogin;

        /// <summary>
        /// 这个网关服务器和Silo主机节点之间的链路
        /// </summary>
        private IClusterClient client;

        public Session(IClusterClient client,IChannelHandlerContext context)
        {
            this.client = client;

            this.context = context;

            isLogin = false;
        }

        /// <summary>
        /// 处理收到来自游戏客户端的数据包
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        public async Task DispatchReceivePacket(NetPackage netPackage)
        {
            try
            {
                //未鉴权的Session对象 只能接收Login消息
                if (isLogin == false)
                {
                    //登录Grain在集群中只需要一个
                    ILoginGrain loginGrain = client.GetGrain<ILoginGrain>("SingleLoginGrain");
                    //用户管理目前也只设置一个
                    IUserManagerGrain userManagerGrain = client.GetGrain<IUserManagerGrain>("SingleUserManagerGrain");
                    if(netPackage.protoID == (int)ProtoCode.ELogin)
                    {
                        Logger.Instance.Information($"收到登录请求:{netPackage.protoID}");
                        NetPackage resultPackage = await loginGrain.OnLogin(netPackage);

                        //await context.WriteAndFlushAsync(resultPackage);
                        await context.WriteAndFlushAsync(resultPackage).ContinueWith(task =>
                        {
                            if (task.IsCompletedSuccessfully)
                                Logger.Instance.Information("数据已经写入 socket buffer");
                            else
                                Logger.Instance.Error("发送失败: " + task.Exception);
                        });

                        await NotifyOnLine(resultPackage);
                    }
                    else
                    {
                        Logger.Instance.Information($"未被鉴权的Session发送协议:{netPackage.protoID}忽略此协议");
                    }
                }
                else
                {
                    await routerGrain.OnReceivePacket(netPackage);
                }

            }catch(Exception ex)
            {
                Logger.Instance.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 通知Silo主机节点 某个账号上线
        /// </summary>
        /// <param name="resultPackage"></param>
        /// <returns></returns>
        private async Task NotifyOnLine(NetPackage resultPackage)
        {
            IMessage message = new LoginResp();

            LoginResp loginResp = message.Descriptor.Parser.ParseFrom(resultPackage.bodyData,0,resultPackage.bodyData.Length) as LoginResp;
            Logger.Instance.Information($"进入NotifyOnLine方法:登录结果{loginResp.Result},{loginResp.OpenId}");
            if(loginResp.Result == LoginResult.ELoginSuccess)
            {
                isLogin = true;
                
                //如果actor不存在，系统会自动创建
                routerGrain = client.GetGrain<IPacketRouterGrain>(loginResp.OpenId.ToString());
                //创建位于网关服务器的观察者
                packetObserver = new PacketObserver(context);
                //创建观察者的远程引用
                IPacketObserver observerRef = await client.CreateObjectReference<IPacketObserver>(packetObserver);
                //绑定观察者
                await routerGrain.BindPacketObserver(observerRef);

                await routerGrain.OnLine();
            }
        }

        /// <summary>
        /// 通知Silo主机节点 某个账号下线
        /// </summary>
        public void Disconnect()
        {
            //IUserManagerGrain userManagerGrain = client.GetGrain<IUserManagerGrain>("SingleUserManagerGrain");

            //userManagerGrain.DeactivateUserGrain(routerGrain.GetPrimaryKeyString());
            
            if(routerGrain != null)
            {
                routerGrain.OffLine();
            }
        }
    }
}
