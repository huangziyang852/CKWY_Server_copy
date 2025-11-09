namespace IGrains.Models;

public class GachaResult
{
    public List<int> HeroResult { get; set; } = new();
    public List<Item> AddItem { get; set; } = new();
    
    public List<Hero> AddHero { get; set; } = new();
}