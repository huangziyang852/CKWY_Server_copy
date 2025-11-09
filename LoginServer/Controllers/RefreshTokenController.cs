using Common;
using Hzy.Common.Proto;
using LoginServer.Models;
using LoginServer.ODM;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LoginServer.Controllers;

[ApiController]
[Route("[controller]")]
public class RefreshTokenController: ControllerBase
{
    private static MongoRepository<UserInfo> _userInfoTable;
    
    public RefreshTokenController(MongoDbContext dbContext)
    {
        _userInfoTable = new MongoRepository<UserInfo>(dbContext);
    }
    
    [HttpPost(Name = "refreshToken")]
    public async Task<IActionResult> Post([FromForm] RefreshTokenRequest request)
    {
        string openId = request.OpenId;
        string refreshToken = request.RefreshToken;
        
        Logger.Instance.Information("api:RefreshToken,parameter: openId={openId}, refreshToken={refreshToken}", openId, refreshToken);
        
        string userId = TokenService.Instance.DecodeUserIdFromToken(request.RefreshToken);

        if (String.IsNullOrEmpty(userId))
        {
            return new JsonResult(new ApiResponse<string>
            {
                Success = false,
                ErrorCode = ErrorCode.TokenOutOfTime,
                Message = "token out of time"
            });
        }
        
        string newToken = TokenService.Instance.GenerateToken(userId.ToString());
        string newRefreshToken = TokenService.Instance.GenerateToken(userId.ToString(),43200);
        
        RefreshTokenResponse refreshTokenResponse = new RefreshTokenResponse
        {
            LoginToken = newToken,
            RefreshToken = newRefreshToken
        };
        
        ApiResponse<RefreshTokenResponse> response = new ApiResponse<RefreshTokenResponse>
        {
            Success = true,
            ErrorCode = ErrorCode.NONE,
            Message = "refresh token success",
            Data = refreshTokenResponse
        };
        
        string jsonRes = JsonConvert.SerializeObject(response);
        Logger.Instance.Information("send to client:" + jsonRes);

        return new JsonResult(response);
    }
    
    public class RefreshTokenRequest
    {
        public required string OpenId { get; set; }
        public required string RefreshToken { get; set; }
    }

    public class RefreshTokenResponse
    {
        public required string LoginToken { get; set; }
        
        public required string RefreshToken { get; set; }
    }
}