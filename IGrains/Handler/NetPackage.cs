using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGrains.Handler
{
    /// <summary>
    /// 网关和游戏服之间的网络包
    /// </summary>
    public class NetPackage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public int protoID;

        /// <summary>
        /// 消息体
        /// </summary>
        public byte[] bodyData;
    }
}
