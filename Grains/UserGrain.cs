using IGrains;
using Orleans;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using IGrains.GrainState;

namespace Grains
{
    [StorageProvider(ProviderName = "MongoDBStore")]
    public class UserGrain : Grain<UserInfo>, IUserGrain
    {
        /// <summary>
        /// 初始化所有的Grain
        /// </summary>
        /// <returns></returns>
        public override async Task OnActivateAsync()
        {
            string openId = State.OpenId.ToString();
            Logger.Instance.Information($"User Grain initialized.{this.GetPrimaryKeyString()}");
            //IHeroGrain heroGrain = GrainFactory.GetGrain<IHeroGrain>(openId);
            //await heroGrain.GetHeroInfoAsync();
            await base.OnActivateAsync();
        }
        
        public Task<UserInfo> GetUserInfoAsync()
        {
            return Task.FromResult(State);
        }
        

        public async Task SetUserInfoAsync(UserInfo userInfo)
        {
            State = userInfo;
            await WriteStateAsync();
        }
        
        public async Task SaveAndDeactivate()
        {
            string openId = State.OpenId.ToString();
            IHeroGrain heroGrain = GrainFactory.GetGrain<IHeroGrain>(openId);
            await heroGrain.DeactivateAsync();
            await WriteStateAsync();
            DeactivateOnIdle();
        }

        public async Task AddOrReplaceBattleSlot(int position,string heroCd)
        {
            BattleSlot battleSlot = State.Deck.FirstOrDefault(slot => slot.position == position);
            if (battleSlot != null)
            {
                battleSlot.heroCd = heroCd;
            }
            else
            {
                State.Deck.Add(new BattleSlot
                {
                    position = position,
                    heroCd = heroCd
                });
            }
            await WriteStateAsync();
        }
    }
}
