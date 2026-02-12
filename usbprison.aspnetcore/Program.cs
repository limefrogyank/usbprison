using Lib.AspNetCore.WebPush;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using usbprison.aspnetcore;

[assembly: ApiController]
namespace usbprison.aspnetcore;

public class WebAppHostProgram
{

    public static async Task<WebApplication> CreateWebApp(
        int httpPort, 
        int httpsPort, 
        string applicationName, 
        //X509Certificate2 certificate, 
        MessageDispatcher messageDispatcher, 
        CallbackLoggerProvider loggerProvider)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = applicationName
        });

        builder.WebHost.ConfigureKestrel((context, serverOptions) =>
        {
            serverOptions.Listen(IPAddress.Loopback, httpPort);
            serverOptions.Listen(IPAddress.Loopback, httpsPort, listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                //listenOptions.UseHttps(certificate);
            });
        });

        builder.Services.AddMemoryCache();
        builder.Services.AddMemoryVapidTokenCache();

        var dbPath = Path.Combine(AppContext.BaseDirectory, "pushService.db");
        builder.Services.AddSingleton(new PushServiceStoreService(dbPath));
        builder.Services.AddSingleton(new PushNotificationsQueue());

        builder.Services.AddPushServiceClient(options =>
        {
            IConfigurationSection pushNotificationServiceConfigurationSection = builder.Configuration.GetSection(nameof(PushServiceClient));

            options.Subject = "https://localhost:5001/";
            options.PublicKey = "a";
            options.PrivateKey = "a";
        });

        builder.Services.AddTransient<PushNotificationService>();

        builder.Services.AddControllers();


        var app = builder.Build();
        app.MapGet("/", () => "Hello World!");
        app.MapControllers();

        

        app.Run();


        return app;


    }
}