using Common;
using IGrains;
using IGrains.GrainState;
using Newtonsoft.Json.Linq;
using Orleans;
using Orleans.Providers;
using Serilog.Core;
using System.Text.Json;
using IGrains.Models;
using Logger = Common.Logger;

namespace Grains;

[StorageProvider(ProviderName = "MongoDBStore")]
public class HeroGrain:Grain<HeroInfo>,IHeroGrain
{
    public async Task<HeroInfo> GetHeroInfoAsync()
    {
        if (State.Heroes.Count == 0)
        {
            await InitHeroOfEmpty();
        }
        return State; 
    }

    public async Task InitHeroOfEmpty()
    {
        string openId = this.GetPrimaryKeyString();
        IUserGrain userGrain = GrainFactory.GetGrain<IUserGrain>(openId);

        if (State == null || State.Heroes.Count == 0)
        {
            string uniqueId = Guid.NewGuid().ToString();
            cfg.hero.Hero heroMaster = TableLoader.Instance.MasterTables.TbHero.Get(304002);

            State = new HeroInfo
            {
                Heroes = new List<Hero>
                {
                    new Hero {HeroCd = uniqueId, HeroId = heroMaster.HeroID, Level = 1 ,Star = 4, Quality = 0}
                }
            };
            await WriteStateAsync(); // 先保存 Hero 状态
            await userGrain.AddOrReplaceBattleSlot(0, uniqueId); // 然后再写 User
        }
    }

    public Task DeactivateAsync()
    {
        Logger.Instance.Information($"HeroGrain '{this.GetPrimaryKeyString()}' Deactivate.");
        DeactivateOnIdle();
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// 当Grain被停用时，确保状态持久化
    /// </summary>
    public override async Task OnDeactivateAsync()
    {
        await WriteStateAsync();
        await base.OnDeactivateAsync();
    }

    /// <summary>
    /// 添加复数个英雄
    /// </summary>
    /// <param name="heroResult"></param>
    public async Task<List<Hero>> AddNewHero(List<int> heroResult)
    {
        List<cfg.hero.Hero> heroMasters = TableLoader.Instance.MasterTables.TbHero.DataList;

        List<Hero> newHeroes = new List<Hero>();
        foreach (int heroId in heroResult)
        {
            var heroMaster = heroMasters.FirstOrDefault(h => h.HeroID == heroId);
            if (heroMaster != null)
            {
                string uniqueId = Guid.NewGuid().ToString();
                Hero newHero = new Hero
                {
                    HeroCd = uniqueId,
                    HeroId = heroMaster.HeroID,
                    Level = 1,
                    Star = heroMaster.Star,
                    Quality = heroMaster.Quality,
                };
                newHeroes.Add(newHero);
            }
            else
            {
                Logger.Instance.Error($"找不到HeroID={heroId}的配置数据！");
            }
        }

        if (newHeroes.Count > 0)
        {
            State.Heroes.AddRange(newHeroes);
            await WriteStateAsync();
        }
        else
        {
            Logger.Instance.Error($"user heroList is empty or null ！");
        }
        
        return newHeroes;
    }
}