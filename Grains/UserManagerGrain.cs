using Common;
using Google.Protobuf;
using IGrains;
using LaunchPB;
using Orleans;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Grains;

public class UserManagerGrain: Grain, IUserManagerGrain
{
    private static ConcurrentDictionary<string, DateTime> _clientLastHeartbeats = new ConcurrentDictionary<string, DateTime>();

    public override async Task OnActivateAsync()
    {
        var grainId = this.GetPrimaryKeyString(); // 获取 Grain 的 ID
            
        Logger.Instance.Information($"Grain '{grainId}' initialized.");
            
        await base.OnActivateAsync();
    }

    public Task<IUserGrain> GetUserGrain(string openId)
    {
        IUserGrain userGrain = GrainFactory.GetGrain<IUserGrain>(openId);

        return Task.FromResult(userGrain);
    }

    public async Task DeactivateUserGrain(string openId)
    {
        var userGrain = GrainFactory.GetGrain<IUserGrain>(openId);
        await userGrain.SaveAndDeactivate();
    }

    /// <summary>
    /// 记录每个玩家的心跳包的时间
    /// </summary>
    /// <param name="openId"></param>
    /// <returns></returns>
    public Task RefreshHeartBeat(string openId)
    {
        // 更新客户端最后心跳时间
        if (!string.IsNullOrEmpty(openId))
        {
            _clientLastHeartbeats[openId] = DateTime.Now;
        }
        return Task.CompletedTask;
    }

    public static void CleanupHeartbeats()
    {
        DateTime now = DateTime.Now;
        foreach (var kv in _clientLastHeartbeats.ToArray()) // 避免并发修改
        {
            if (now - kv.Value > TimeSpan.FromMinutes(2))
            {
                Logger.Instance.Information($"客户端 {kv.Key} 超过最大心跳时间，已断开");
                //TODO
                //这里主动断开连接
                _clientLastHeartbeats.TryRemove(kv.Key, out _);
            }
        }
    }
}