using Orleans;

namespace IGrains;

public interface IUserManagerGrain:IGrainWithStringKey
{
    Task<IUserGrain> GetUserGrain(string openId);
    
    Task DeactivateUserGrain(string openId);

    Task RefreshHeartBeat(string openId);
}