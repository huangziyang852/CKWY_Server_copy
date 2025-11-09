using IGrains.Models;

namespace IGrains.GrainState;

public class HeroInfo
{
    public List<Hero> Heroes { get; set; } = new List<Hero>();
}