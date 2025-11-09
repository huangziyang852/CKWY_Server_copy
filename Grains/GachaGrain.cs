using cfg.hero;
using Common;
using Grains.Service.Interface;
using IGrains;
using IGrains.Models;
using Orleans;
using static Grains.Enum.GameEnum;
using Hero = IGrains.Models.Hero;

namespace Grains
{
    public class GachaGrain : Grain, IGachaGrain
    {
        private readonly IGachaService _gachaService;
        public GachaGrain(IGachaService gachaService)
        {
            _gachaService = gachaService;
        }

        public async Task<GachaResult> GachaExcuteAsync(int gachaId, int times)
        {
            List<int> heroResult = new List<int>();
            List<Item> addItem = new List<Item>();
            List<Hero> addHero = new List<Hero>();
            string openId = this.GetPrimaryKeyString();
            for (int i = 0; i < times; i++)
            {
                try
                {
                    int heroId = _gachaService.GachaOnce(gachaId);
                    cfg.hero.Hero heroMaster = TableLoader.Instance.MasterTables.TbHero.Get(heroId);
                    if (heroMaster is { Star: <= 3 })
                    {
                        Logger.Instance.Information($"英雄{heroId}被转换成道具");
                        HeroExchange heroExchangeMaster = TableLoader.Instance.MasterTables.TbHeroExchange.Get(heroMaster.Star);
                        addItem = heroExchangeMaster.Item
                            .Where(kv => kv.Value > 0)
                            .GroupBy(kv => kv.Key)
                            .Select(g => new Item
                            {
                                ItemId = g.Key,
                                Count = g.Sum(x => x.Value)
                            })
                            .ToList();
                        
                        IItemGrain itemGrain = GrainFactory.GetGrain<IItemGrain>(openId);
                        itemGrain.AddItemAsync(addItem);
                        heroResult.Add(heroId);
                    }
                    else
                    {
                        heroResult.Add(heroId);
                        IHeroGrain userHeroGrain = GrainFactory.GetGrain<IHeroGrain>(openId);
                        addHero = await userHeroGrain.AddNewHero(heroResult);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Information($"Gacha 第{i + 1}次失败: {ex.Message}");
                }
            }

            GachaResult gachaResult = new GachaResult();
            gachaResult.HeroResult = heroResult;
            gachaResult.AddItem = addItem;
            gachaResult.AddHero = addHero;
            return gachaResult;
        }
    }
}
