using GateServer.Net;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Configuration;
using Common;
using IGrains;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GateServer
{
    class Program
    {
        private static IClusterClient client;

        private static TcpServer tcpServer;
        
        private static WebSocketServer webSocketServer;
        static async Task Main(string[] args)
        {
            Logger.Create("GateServer");

            await ConnectClient();

            Logger.Instance.Information("网关服务器链接游戏服务器");

            tcpServer = new TcpServer(client);
            await tcpServer.StartAsync();
            webSocketServer = new WebSocketServer(client);
            await webSocketServer.StartAsync();

            //保持程序运行
            await Task.Delay(-1);
        }

        /// <summary>
        /// 链接游戏服务器
        /// </summary>
        /// <returns></returns>
        private static async Task<IClusterClient> ConnectClient()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // 设置基础路径
                .AddJsonFile("appsettings.json") // 读取配置文件
                .Build();
            var mysqlConfig = configuration.GetSection("MySql");
            client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "ClusterId";
                    options.ServiceId = "ServiceId";
                }).Build();
                // .UseAdoNetClustering(options =>
                // {
                //     options.Invariant = "MySql.Data.MySqlClient";
                //     options.ConnectionString = mysqlConfig.GetValue<string>("ConnectionString");
                // }).Build();

            await client.Connect();

            return client;
        }
    }
}