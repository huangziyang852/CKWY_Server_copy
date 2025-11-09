using Common;
using Grains.Service.Interface;
using Microsoft.AspNetCore.Routing.Matching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Grains.Enum.GameEnum;

namespace Grains.Service
{
    public class GachaService : IGachaService
    {
        private static Random _random = new Random();
        public int[] GachaMultiple(int gachaId)
        {
            throw new NotImplementedException();
        }

        public int GachaOnce(int gachaId)
        {
            List<cfg.hero.Hero> heroMasters = TableLoader.Instance.MasterTables.TbHero.DataList;
            cfg.gacha.GachaRate gachaRate = GetGachaRate(gachaId);

            int totalWeight = gachaRate.Rate.Values.Sum();
            int roll = _random.Next(1, totalWeight + 1);

            int accumulated = 0;
            int selectedStar = 1;

            foreach (var kvp in gachaRate.Rate.OrderBy(kvp => kvp.Key))
            {
                accumulated += kvp.Value;
                if (roll <= accumulated)
                {
                    selectedStar = kvp.Key;
                    break;
                }
            }

            var targetHeros = heroMasters.Where(h => h.Star == selectedStar).ToList();
            if (targetHeros.Count == 0)
            {
                throw new Exception($"can not find hero with start {selectedStar} ！");
            }

            int index = _random.Next(targetHeros.Count);
            cfg.hero.Hero heroResult = targetHeros[index];

            return heroResult.HeroID;
        }

        private cfg.gacha.GachaRate GetGachaRate(int gachaId)
        {
            cfg.gacha.GachaRate gachaRate = TableLoader.Instance.MasterTables.TbGachaRate.Get(1);
            switch ((GachaId)gachaId)
            {
                case GachaId.NORMAL:
                    gachaRate = TableLoader.Instance.MasterTables.TbGachaRate.Get(1);
                    break;
                case GachaId.SUPER:
                    gachaRate = TableLoader.Instance.MasterTables.TbGachaRate.Get(2);
                    break;
                case GachaId.FRIEND:
                    gachaRate = TableLoader.Instance.MasterTables.TbGachaRate.Get(3);
                    break;
            }
            return gachaRate;
        }
    }
}
