using Common;
using Google.Protobuf;
using IGrains;
using IGrains.Handler;
using LaunchPB;
using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grains.Handler;

namespace Grains
{
    public class PacketRouterGrain : Grain,IPacketRouterGrain
    {
        private IPacketObserver observer;
        /// <summary>
        /// 记录此Grain对应的玩家是否在线
        /// </summary>
        public bool onLine {  get; set; }
        /// <summary>
        /// 保存这个Grain对应的玩家的OpenId
        /// </summary>
        private string openId; // 存储玩家的 OpenId

        private static readonly Dictionary<int, IMessageHandler> handlers = new();
        private static readonly HeartBeatHandler heartbeatHandler = new HeartBeatHandler();

        public override Task OnActivateAsync()
        {
            openId = this.GetPrimaryKeyString(); // 记录 OpenId
            Logger.Instance.Information($"PacketRouterGrain 被创建，OpenId: {openId}");

            // 这里手动注册所有的消息处理器
            handlers[(int)ProtoCode.EGetPlayerInfo] = new GetPlayerInfoHandler();
            handlers[(int)ProtoCode.EGetHeroInfo] = new GetHeroInfoHandler();
            handlers[(int)ProtoCode.EGacha] = new GachaHandler();
            handlers[(int)ProtoCode.EGetWorldInfo] = new GetWorldInfoHandler();
            handlers[(int)ProtoCode.EGetItemInfo] = new GetItemInfoHandler();
            return base.OnActivateAsync();
        }

        /// <summary>
        /// 设置观察者，由网关服务器调用
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public Task BindPacketObserver(IPacketObserver observer)
        {
            this.observer = observer;

            return Task.CompletedTask;
        }

        /// <summary>
        /// 当游戏服收到来自网关的消息
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task OnReceivePacket(NetPackage netPackage)
        {
            try
            {
                // 解析 netPackage 获取消息类型
                int messageType = netPackage.protoID;

                if(netPackage.protoID == (int)ProtoCode.EHeartBeat)
                {
                    await heartbeatHandler.Handle(netPackage,openId,GrainFactory);
                }

                // 根据类型找到对应的 Handler
                if (handlers.TryGetValue(messageType, out var handler))
                {
                    NetPackage response = await handler.HandlePlayerMessage(netPackage,openId,GrainFactory);

                    // 处理完成后，返回结果给客户端
                    await SendPacket(response);
                }
                else
                {
                    Logger.Instance.Error($"未找到处理器: {messageType}");
                    
                    NetPackage errorPackage = new NetPackage()
                    {
                        protoID = (int)ProtoCode.EErrorMessage,

                        bodyData = BitConverter.GetBytes(1001)
                    };
                    await SendPacket(errorPackage);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"处理消息 {netPackage.protoID} 出错: {ex}");
            }

        }

        public Task OnLine()
        {
            onLine = true;

            string openId = GrainReference.GrainIdentity.PrimaryKeyString;

            Logger.Instance.Information($"{openId}上线了");

            return Task.CompletedTask;
        }


        public Task OffLine()
        {
            onLine = false;

            string openId = GrainReference.GrainIdentity.PrimaryKeyString;

            Logger.Instance.Information($"{openId}下线了");

            return Task.CompletedTask;
        }

        public Task SendPacket(NetPackage package)
        {
            if (observer != null)
            {
                observer.OnReceivePacket(package);
            }
            return Task.CompletedTask;
        }
    }
}
