using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grains.Service.Interface
{
    public interface IGachaService
    {
        int GachaOnce(int gachaId);

        int[] GachaMultiple(int gachaId);
    }
}
