using StackExchange.Redis;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Runtime.CompilerServices;
using Common;

namespace Hzy.Service.Redis
{
    public class RedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration;
        private readonly IDatabase _redisDatabase;
        private static int MAX_TRY_TIMES = 3;
        private static int FAILED_SLEEP = 2;

        private static readonly string DelimiterColon = ":";
        //private RedisUtil redisUtil = new RedisUtil();
        private string areaId = "0";

        //private static int MAX_TRY_TIMES = 3;       //最大尝试次数，保障获取/存储成功
        //private static int FAILED_SLEEP = 2;       //每次失败最大停顿时间

        public RedisService(ILogger<RedisService> logger, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("Redis");

            if (string.IsNullOrEmpty(redisConnectionString))
            {
                throw new ArgumentNullException("Redis connection string is not configured.");
            }
            var options = ConfigurationOptions.Parse(redisConnectionString);

            // 如果需要单独配置参数，可以直接设置
            options.ConnectTimeout = 20; // 连接超时时间
            options.SyncTimeout = 20;    // 同步超时时间
            options.AbortOnConnectFail = false; // 连接失败时是否中止

            _configuration = configuration;

            _redis = ConnectionMultiplexer.Connect(options);
            if (_redis.IsConnected)
            {
                logger.LogInformation("redis connected" + configuration.GetConnectionString("Redis"));
            }
            else
            {
                logger.LogInformation("redis connected failed");
            }

            _redisDatabase = GetDatabase();

        }

        public IDatabase GetDatabase()
        {
            return _redis.GetDatabase();
        }

        private string GetKey<K>(string type, K key, bool withServerId)
        {
            if (withServerId)
            {
                return areaId + DelimiterColon + type + DelimiterColon + key?.ToString();
            }
            else
            {
                return type + DelimiterColon + key?.ToString();
            }
        }

        public void Set<T, K>(string type, K key, T value, int seconds, bool withServerId)
        {
            // 将对象转换为 JSON 字符串
            string strValue = JsonConvert.SerializeObject(value);
            string fullKey = GetKey(type, key, withServerId);

            if (seconds <= 0)
            {
                // 不设置过期时间
                _redisDatabase.StringSet(fullKey, strValue);
            }
            else
            {
                // 设置过期时间
                _redisDatabase.StringSet(fullKey, strValue, TimeSpan.FromSeconds(seconds));
            }
        }

        public T Get<K, T>(string type,K key,Type clazz,int seconds)
        {
            return Get<K,T>(type, key,clazz,seconds,false);
        }

        public T Get<K,T>(string type,K key,Type clazz,int seconds,bool withServerId)
        {
            string fullKey=GetKey(type, key,withServerId);
            string strValue = GetValue(fullKey);
            if(strValue == null)
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(strValue);
        }

        //获取值
        protected string GetValue(string key)
        {
            for(int i = 0; i < MAX_TRY_TIMES; i++)
            {
                try
                {
                    return _redisDatabase.StringGet(key);
                }catch (Exception e)
                {
                    Logger.Instance.Error($"Get->i={i}, key={key}", e);
                    Task.Delay(FAILED_SLEEP).Wait();
                }
            }
            throw new Exception("RedisUtil::Get exception");
        }



        //获取redis Dic全部数据的直接方法
        public Dictionary<string, T> hgetAll<T>(string type, string key, int seconds, bool withServerId)
        {
            string fullKey = GetKey(type, key, withServerId);
            var hashEntries = _redisDatabase.HashGetAll(fullKey);

            // Convert HashEntry[] to Dictionary<string, T>
            var result = new Dictionary<string, T>();
            foreach (var entry in hashEntries)
            {
                if (entry.Value.HasValue)
                {
                    T value = JsonConvert.DeserializeObject<T>(entry.Value);
                    result.Add(entry.Name, value);
                }
            }
            return result;
        }
    }
}
