using Common;
using Hzy.Service.Redis;
using LoginServer.ODM;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Loopback, 8080);  //监听ip
});

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<RedisService>();
builder.Services.AddRazorPages();

// Forwarded Headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(System.Net.IPAddress.Parse("127.0.0.1")); // 只信任nginx来自nginx的转发
});


var app = builder.Build();
//app.UsePathBase("/tk");
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "/{controller=File}/{action=Index}/{id?}");
Logger.Create("LoginServer");

//启用“转发头中间件（Forwarded Headers Middleware）”让应用信任nginx转发的消息
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

// �� .NET Core �Զ���ת HTTPS����ѡ��Nginx �Ѵ���
//app.UseHttpsRedirection();

app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

app.Run();



