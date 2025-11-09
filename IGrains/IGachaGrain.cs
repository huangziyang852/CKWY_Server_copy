using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGrains.Models;

namespace IGrains
{
    public interface IGachaGrain :IGrainWithStringKey
    {
        Task<GachaResult> GachaExcuteAsync(int gachaId,int times);
    }
}
