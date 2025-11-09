using Microsoft.AspNetCore.Mvc;
using Hzy.Common.Utils;
using Hzy.Common.Proto;
using Hzy.Model.User;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using LoginServer.ODM;
using Common;
using LoginServer.Models;
using IdGen;

[ApiController]
[Route("[controller]")]
public class UserRegisterController : ControllerBase
{
    private static MongoRepository<UserRegisterInfo> _userRegisterInfoTable;

    public UserRegisterController(MongoDbContext dbContext)
    {
        _userRegisterInfoTable= new MongoRepository<UserRegisterInfo>(dbContext);
    }

    [HttpPost(Name = "registerUser")]
    public IActionResult Post([FromForm] RegisterUserRequest request)
    {
        string account = request.Account;
        string password = request.Password;
        //string sign = request.Sign;

        Logger.Instance.Information("api:registerUser,parameter: account={Account}, password={Password}", account, password);

        List<string> errors = new List<string>();

        if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
        {
            return new JsonResult(new ApiResponse<string>
            {
                Success = false,
                ErrorCode = ErrorCode.BadRegistRequest,
                Message = "Account or password cannot be null"
            });
        }

        // 检查账号密码
        bool isAccountValid = CommonUtils.ValidateAccount(account);
        bool isPasswordValid = CommonUtils.ValidatePassword(password);
        if (!isAccountValid)
        {
            return new JsonResult(new ApiResponse<string>
            {
                Success = false,
                ErrorCode = ErrorCode.BadRegistRequest,
                Message = "The account is 6 to 15 digits of numbers or letters"
            });
        }
        if (!isPasswordValid)
        {
            return new JsonResult(new ApiResponse<string>
            {
                Success = false,
                ErrorCode = ErrorCode.BadRegistRequest,
                Message = "The password is 8 to 20 digits of numbers or letters"
            });
        }

        if (IsAccountExist(account))
        {
            return new JsonResult(new ApiResponse<string>
            {
                Success = false,
                ErrorCode = ErrorCode.BadRegistRequest,
                Message = "Account Exist"
            });
        }

        //int random = new Random(1024).Next();
        var generator = new IdGenerator(0);
        long id = generator.CreateId();
        long openId = id;

        UserRegisterInfo newUser = new UserRegisterInfo(account,password,openId);
        _userRegisterInfoTable.Add(newUser);

        // 验证通过，返回成功响应
        ApiResponse<string> response = new ApiResponse<string>
        {
            Success = true,
            ErrorCode = ErrorCode.NONE,
            Message = "Validation successful"
        };

        return new JsonResult(response);
    }

    public bool IsAccountExist(string account)
    {
        List<UserRegisterInfo> userRegisterInfoList = _userRegisterInfoTable.Find(x=>x.Account==account);
        // 如果找到匹配文档，则返回 true，表示账号已存在
        return userRegisterInfoList.Count >= 1;
    }

    public class RegisterUserRequest
    {
        public required string Account { get; set; }
        public required string Password { get; set; }
        //public required string Sign { get; set; }
    }
}