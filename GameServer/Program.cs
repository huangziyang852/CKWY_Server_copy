using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common;
using Grains.Service;
using Grains.Service.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.CodeGeneration;
using Orleans.Configuration;
using Orleans.Hosting;

namespace GameServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // 设置基础路径
                .AddJsonFile("appsettings.json") // 读取配置文件
                .Build();

            Logger.Create("GameServer");

            var mongoConfig = configuration.GetSection("MongoDB");
            var mysqlConfig = configuration.GetSection("MySql");
            var endpointOptionsConfig = configuration.GetSection("EndpointOptions");

            var host = Host.CreateDefaultBuilder()
                .UseOrleans(siloBuilder =>
                {
                    //本地时下面这行解注释
                    siloBuilder.UseLocalhostClustering();
                    siloBuilder.Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "ClusterId";
                        options.ServiceId = "ServiceId";
                    });
                    siloBuilder.Configure<EndpointOptions>(options =>
                    {
                        options.SiloPort = endpointOptionsConfig.GetValue<int>("SiloPort"); // Silo 内部通讯端口（Cluster 内部通讯）
                        options.GatewayPort = endpointOptionsConfig.GetValue<int>("GatewayPort"); // Gateway 端口，供 Client 连接
                        options.AdvertisedIPAddress = IPAddress.Parse(endpointOptionsConfig.GetValue<string>("AdvertisedIPAddress")); // Silo 广播的 IP
                    });
                    //本地时注释掉
                    //siloBuilder.UseAdoNetClustering(options =>
                    //{
                    //    options.Invariant = "MySql.Data.MySqlClient";
                    //    options.ConnectionString = mysqlConfig.GetValue<string>("ConnectionString");
                    //});
                    var mongoConnectionString = mongoConfig.GetValue<string>("ConnectionString");
                    var mongoDatabaseName = mongoConfig.GetValue<string>("DatabaseName");
                    var createShardKeyForCosmos = mongoConfig.GetValue<bool>("CreateShardKeyForCosmos");

                    siloBuilder.UseMongoDBClient(mongoConnectionString);
                    siloBuilder.AddMongoDBGrainStorage("MongoDBStore", options =>
                    {
                        options.DatabaseName = mongoDatabaseName;
                        options.CreateShardKeyForCosmos = createShardKeyForCosmos;
                    });
                    siloBuilder.ConfigureLogging(logging =>
                    {
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Debug);  // 设置为 Debug 或更低级别，以获得详细日志
                    });
                    siloBuilder.ConfigureServices(services =>
                    {
                        //注册单例
                        services.AddSingleton<IGachaService, GachaService>();
                    });
                })
                .Build();

            Logger.Instance.Information("开启GameServer");

            Logger.Instance.Information("开始加载数据表");
            TableLoader.Instance.LoadMasterTable();

            await host.RunAsync();//启动当前silo


            Console.ReadLine();
        }

    }
}
