using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Hzy.Common.Utils
{
    public class CommonUtils
    {
        public static string salt = "1fa8fc0775e81dbdf23d14668f7440c6";

        public static string CalculateMD5(string input)
        {
            // 创建MD5对象
            using (MD5 md5 = MD5.Create())
            {
                string InputWithSalt = input + salt;
                // 将输入字符串转换为字节数组并计算哈希
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(InputWithSalt));

                // 将字节数组转换为十六进制字符串
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

        // 账号验证方法
        public static bool ValidateAccount(string account)
        {
            // 账号长度限制：6 到 15 个字符
            if (account.Length < 6 || account.Length > 15)
            {
                return false;
            }

            // 允许的字符：字母、数字和下划线
            string pattern = @"^[a-zA-Z0-9_]+$";
            return Regex.IsMatch(account, pattern);
        }

        // 密码验证方法
        public static bool ValidatePassword(string password)
        {
            // 密码长度限制：8 到 20 个字符
            //if (password.Length < 8 || password.Length > 20)
            //{
            //    return false;
            //}

            // 密码必须包含至少一个大写字母、一个小写字母、一个数字
            //bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            //bool hasLower = Regex.IsMatch(password, @"[a-z]");
            //bool hasDigit = Regex.IsMatch(password, @"\d");

            //return hasUpper && hasLower && hasDigit;
            return Regex.IsMatch(password, @"^[A-Za-z0-9]{8,20}$");
        }
    }
}