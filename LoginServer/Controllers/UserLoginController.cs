using Common;
using Hzy.Common.Const;
using Hzy.Common.Proto;
using Hzy.Model.User;
using Hzy.Service.Redis;
using LoginServer.Models;
using LoginServer.ODM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("[controller]")]
public class UserLoginController : ControllerBase
{
    
    private readonly RedisService _redis;

    private static MongoRepository<UserRegisterInfo> _userRegisterInfoTable;

    private static MongoRepository<UserInfo> _userInfoTable;

    private static readonly string _COLLECTION_NAME = "user_register_info";


    public UserLoginController(RedisService redis, MongoDbContext dbContext)
    {
        _redis = redis;
        _userRegisterInfoTable = new MongoRepository<UserRegisterInfo>(dbContext);
        _userInfoTable = new MongoRepository<UserInfo>(dbContext);

    }

    [HttpPost(Name = "userLogin")]
    public async Task<IActionResult> Post([FromForm] UserLoginRequest request)
    {
        string account = request.Account;
        string sign = request.Sign;
        //int serverId = request.ServerId;

        Logger.Instance.Information("api:userLogin,parameter: account={Account}, sign={Sign}", account, sign);

        if (string.IsNullOrEmpty(account))
        {
            return new JsonResult(new ApiResponse<string>
            {
                Success = false,
                ErrorCode = ErrorCode.BadRegistRequest,
                Message = "Account cannot be null"
            });
        }

        if (string.IsNullOrEmpty(sign))
        {
            return new JsonResult(new ApiResponse<string>
            {
                Success = false,
                ErrorCode = ErrorCode.BadRegistRequest,
                Message = "Sign is null"
            });
        }

        UserRegisterInfo userRegisterInfo = await _userRegisterInfoTable.Collection.Find(x => x.Account == account).FirstOrDefaultAsync();
        if (userRegisterInfo == null)
        {
            return new JsonResult(new ApiResponse<string>
            {
                Success = false,
                ErrorCode = ErrorCode.BadRegistRequest,
                Message = "account not exist"
            });
        }

        string uid = await GetOrCreateUserId(userRegisterInfo);

        if (String.IsNullOrEmpty(uid))
        {
            return new JsonResult(new ApiResponse<string>
            {
                Success = false,
                ErrorCode = ErrorCode.SystemError,
                Message = "create uid failed"
            });
        }

        string token = TokenService.Instance.GenerateToken(uid.ToString());
        string refreshToken = TokenService.Instance.GenerateToken(uid.ToString(),43200);

        UserLoginResponse userLoginResponse = new UserLoginResponse
        {
            OpenId = userRegisterInfo.OpenId.ToString(),
            LoginToken = token,
            RefreshToken = refreshToken
        };

        //暂时先不缓存
        //_redis.Set(RedisKey.LOGIN_TOKEN, userLoginResponse.OpenId, int.Parse(userLoginResponse.LoginToken), -1, false);

        //string jsonParameters = JsonConvert.SerializeObject(parameters);
        ApiResponse<UserLoginResponse> response = new ApiResponse<UserLoginResponse>
        {
            Success = true,
            ErrorCode = ErrorCode.NONE,
            Message = "login successful",
            Data = userLoginResponse
        };

        string jsonRes = JsonConvert.SerializeObject(response);
        Logger.Instance.Information("send to client:" + jsonRes);

        return new JsonResult(response);

    }

    public async Task<string> GetOrCreateUserId(UserRegisterInfo userRegisterInfo)
    {
        UserInfo existingUser = await _userInfoTable.Collection.Find(x => x.OpenId == userRegisterInfo.OpenId).FirstOrDefaultAsync();

        if (existingUser != null)
        {
            return existingUser.UId;
        }

        // 如果用户不存在，插入新用户
        UserInfo newUserInfo = new UserInfo(userRegisterInfo.OpenId, 10001, "pc", ""); // 先给 UId 赋 0，稍后用 MongoDB 生成的 _id 赋值
        await _userInfoTable.Collection.InsertOneAsync(newUserInfo);

        string uid = "";
        var insertedUser = await _userInfoTable.Collection.Find(x => x.OpenId == userRegisterInfo.OpenId).FirstOrDefaultAsync();
        if (insertedUser != null)
        {
            uid = newUserInfo.ID; // 获取 _id 的哈希值
            var update = Builders<UserInfo>.Update.Set(u => u.UId, uid);

            await _userInfoTable.Collection.UpdateOneAsync(x => x.OpenId == newUserInfo.OpenId, update);
        }

        return uid;

    }

    public class UserLoginRequest
    {
        public required string Account { get; set; }
        public required string Sign { get; set; }
        //public required int ServerId {  get; set; }
    }

    public class UserLoginResponse
    {
        public required string OpenId { get; set; }
        public required string LoginToken { get; set; }
        
        public required string RefreshToken { get; set; }
    }
}