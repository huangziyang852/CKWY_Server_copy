using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Common
{
    public class TokenService
    {
        private static readonly TokenService tokenService = new TokenService();
        private const string SecretKey = ""; // 32+字符长度的密钥
        private readonly SymmetricSecurityKey _signingKey;
        private readonly string appName = "ckwy";
        private readonly string audience = "player";

        public TokenService()
        {
            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        }

        public static TokenService Instance => tokenService;
        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="expireMinutes">过期时间</param>
        /// <returns></returns>
        public string GenerateToken(string userId, int expireMinutes = 5)
        {
            var claims = new List<Claim>
            {
                new Claim("UserId",userId.ToString()),
            };

            var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: appName,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        /// <summary>
        /// 验证token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = appName,
                ValidAudience = audience,
                IssuerSigningKey = _signingKey,
                ClockSkew = TimeSpan.Zero // 防止时间误差
            };

            try
            {
                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                Logger.Instance.Error("Token验证失败");
                return null; // 验证失败
            }
        }

        /// <summary>
        /// 解码 token 并提取 userId
        /// </summary>
        /// <param name="token">要解码的 JWT</param>
        /// <returns>解码出的 userId 或 null（如果验证失败）</returns>
        public string DecodeUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = appName,
                ValidAudience = audience,
                IssuerSigningKey = _signingKey,
                ClockSkew = TimeSpan.Zero // 防止时间误差
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                foreach (var claim in principal.Claims)
                {
                    Console.WriteLine($"{claim.Type}: {claim.Value}");
                }

                // 从 Claims 中提取 userId
                var userIdClaim = principal.FindFirst("UserId");
                return userIdClaim?.Value;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Token 验证失败: {ex.Message}");
                return null; // 验证失败或解码失败
            }
        }

    }
}
