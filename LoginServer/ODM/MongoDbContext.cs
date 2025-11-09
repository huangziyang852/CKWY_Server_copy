using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using StackExchange.Redis;

namespace LoginServer.ODM
{
    public class MongoDbContext
    {
        private static IConfiguration _configuration;

        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            var mongoConfig = _configuration.GetSection("MongoDB");

            var settings = MongoClientSettings.FromConnectionString(mongoConfig["ConnectionString"]);
            // 直接获取已转换的值
            settings.MaxConnectionPoolSize = mongoConfig.GetValue<int>("MaxConnectionPoolSize", 100);
            settings.MinConnectionPoolSize = mongoConfig.GetValue<int>("MinConnectionPoolSize", 10);
            settings.ConnectTimeout = TimeSpan.FromMilliseconds(mongoConfig.GetValue<int>("ConnectTimeoutMS", 1200000));
            settings.SocketTimeout = TimeSpan.FromMilliseconds(mongoConfig.GetValue<int>("SocketTimeoutMS", 0));
            settings.MaxConnectionLifeTime = TimeSpan.FromMilliseconds(mongoConfig.GetValue<int>("MaxConnectionLifeTimeMS", 1800000));

            MongoClient client = new MongoClient(settings);
            _database = client.GetDatabase(mongoConfig["DatabaseName"]);
        }

        // 获取集合
        public IMongoCollection<T> GetCollection<T>(string collectionName = null)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                collectionName = typeof(T).Name; // 默认使用类型名作为集合名
            }
            return _database.GetCollection<T>(collectionName);
        }
    }
}
