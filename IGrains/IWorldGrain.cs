using IGrains.GrainState;
using Orleans;

namespace IGrains;

public interface IWorldGrain:IGrainWithStringKey
{
    Task<WorldInfo> GetWorldInfoAsync();
    Task DeactivateAsync();
}