using Microsoft.AspNetCore.Mvc;
using Hzy.Service.Redis;
using Hzy.Model;
using Newtonsoft.Json;
using Hzy.Common.Proto;
using Common;
using LoginServer.ODM;

[ApiController]
[Route("[controller]")]
public class GetServerListController : ControllerBase
{
    private static MongoRepository<ServerInfo> _serverInfoTable;
    private readonly RedisService _redis;

    private static readonly string _COLLECTION_NAME = "server_info";
    private static readonly string _WHITE_LIST = "white_user_list";
    private static readonly string _BLACK_LIST = "black_user_list";
    private static volatile string recommend = ""; //推荐列表 //volatile 并不能保证线程安全，只是确保变量的最新值在不同线程之间被正确读取

    private static long lastRefreshTime = 0;
    //自动调用
    public static long LastRefreshTime
    {
        get => Interlocked.Read(ref lastRefreshTime);
        set => Interlocked.Exchange(ref lastRefreshTime, value);
    }

    public GetServerListController(RedisService redis, MongoDbContext dbContext)
    {
        _redis = redis;
        _serverInfoTable = new MongoRepository<ServerInfo>(dbContext); 
    }

    [HttpGet(Name = "getServerList")]
    public async Task<IActionResult> GetAsync([FromQuery] string channel, [FromQuery] string sub_channel, [FromQuery] string server_version, [FromQuery] string plat)
    {
        //检查参数
        Logger.Instance.Information("api:getServerList,parameter: openId={OpenId}, channel={Channel}, sub_channel={SubChannel}, server_version={ServerVersion}, plat={Plat}",  channel, sub_channel, server_version, plat);
        if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(sub_channel) || string.IsNullOrEmpty(plat) || string.IsNullOrEmpty(server_version))
        {
            return BadRequest("parameters are wrong");
        }

        //获取服务器列表
        List<ServerInfo> serverInfoList = await _serverInfoTable.FindAsync();
        Random random = new Random();
        int randomIndex = random.Next(serverInfoList.Count);
        ServerInfo serverInfo = serverInfoList[randomIndex];
        //获取新服
        ApiResponse<ServerInfo> response = new ApiResponse<ServerInfo>
        {
            Success = true,
            ErrorCode = ErrorCode.NONE,
            Message = "",
            Data = serverInfo
        };

        string jsonRes = JsonConvert.SerializeObject(response);
        Logger.Instance.Information("send to client:" + jsonRes);

        return new JsonResult(response);
    }

}