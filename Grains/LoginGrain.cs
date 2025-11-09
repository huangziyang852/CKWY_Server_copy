using Common;
using Google.Protobuf;
using IGrains;
using IGrains.Handler;
using LaunchPB;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGrains.GrainState;

namespace Grains
{
    public class LoginGrain :Grain, ILoginGrain
    {
        private IUserManagerGrain userManagerGrain;

        public override async Task OnActivateAsync()
        {
            var grainId = this.GetPrimaryKeyString(); // 获取 Grain 的 ID
            
            Logger.Instance.Information($"Grain '{grainId}' initialized.");
            
            await base.OnActivateAsync();
        }
        
        public async Task<NetPackage> OnLogin(NetPackage netPackage)
        {
            userManagerGrain = GrainFactory.GetGrain<IUserManagerGrain>("SingleUserManagerGrain");
            
            IMessage message = new Login();

            Login login = message.Descriptor.Parser.ParseFrom(netPackage.bodyData, 0, netPackage.bodyData.Length) as Login;

            if (login == null || string.IsNullOrEmpty(login.LoginToken))
            {
                return new NetPackage()
                {
                    protoID = (int)ProtoCode.ELoginResp,

                    bodyData = new LoginResp()
                    {
                        Result = LoginResult.EOpenIdWrong
                    }.ToByteArray()
                };
            }

            string userId = TokenService.Instance.DecodeUserIdFromToken(login.LoginToken);

            if (String.IsNullOrEmpty(userId))
            {
                return new NetPackage()
                {
                    protoID = (int)ProtoCode.ELoginResp,

                    bodyData = new LoginResp()
                    {
                        Result = LoginResult.ETokenWrong
                    }.ToByteArray()
                };
            }

            IUserGrain userGrain = await userManagerGrain.GetUserGrain(login.OpenId.ToString());

            UserInfo userInfo = await userGrain.GetUserInfoAsync();

            if (String.IsNullOrEmpty(userInfo.UsertId))
            {
                UserInfo newUserInfo= new UserInfo(userId,login.OpenId);
                await userGrain.SetUserInfoAsync(newUserInfo);
            }
            Logger.Instance.Information($"登录成功" + login.OpenId);
            Logger.Instance.Information($"{userId} 登录成功");

            return new NetPackage()
            {
                protoID = (int)ProtoCode.ELoginResp,

                bodyData = new LoginResp()
                {
                    Result = LoginResult.ELoginSuccess,

                    OpenId = login.OpenId,

                    UserId = userId,

                    LoginToken = login.LoginToken,
                }.ToByteArray()
            };
        }
    }
}
