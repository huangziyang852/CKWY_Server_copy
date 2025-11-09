using IGrains.GrainState;
using IGrains.Models;
using Orleans;

namespace IGrains;

public interface IHeroGrain:IGrainWithStringKey
{
    Task<HeroInfo> GetHeroInfoAsync();

    Task InitHeroOfEmpty();
    
    Task<List<Hero>> AddNewHero(List<int> heroList);
    Task DeactivateAsync();
}