using IGrains.Handler;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGrains
{
    public interface IPacketObserver:IGrainObserver
    {
        /// <summary>
        /// 当网关收到游戏服的消息时
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        public void OnReceivePacket(NetPackage netPackage);
    }
}
