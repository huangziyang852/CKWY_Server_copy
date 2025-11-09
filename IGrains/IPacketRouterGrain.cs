using IGrains.Handler;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGrains
{
    /// <summary>
    /// Actor接口，所有Actor都必须继承这个接口用于处理消息
    /// </summary>
    public interface IPacketRouterGrain:IGrainWithStringKey
    {
        /// <summary>
        /// 当游戏服收到网关的消息时
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        Task OnReceivePacket(NetPackage netPackage);

        /// <summary>
        /// 给游戏服绑定一个观察者observer,方便游戏服给网关推送消息
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        Task BindPacketObserver(IPacketObserver observer);

        /// <summary>
        /// 网关通知:当前Grain对应的玩家上线了
        /// </summary>
        /// <returns></returns>
        Task OnLine();

        /// <summary>
        /// 网关通知:当前Grain对应的玩家下线了
        /// </summary>
        /// <returns></returns>
        Task OffLine();
    }
}
