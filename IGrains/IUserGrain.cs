using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGrains.GrainState;

namespace IGrains
{
    public interface IUserGrain:IGrainWithStringKey
    {
        Task<UserInfo> GetUserInfoAsync();

        Task SaveAndDeactivate();
        Task SetUserInfoAsync(UserInfo userInfo);
        
        Task AddOrReplaceBattleSlot(int position, string heroCd);
    }
}
