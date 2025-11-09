using IGrains.Handler;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGrains
{
    public interface ILoginGrain:IGrainWithStringKey
    {
        /// <summary>
        /// 请求登录
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        Task<NetPackage> OnLogin(NetPackage package);
    }
}
