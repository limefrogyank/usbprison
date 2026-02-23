using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ReactiveUI.Builder;
using Serilog;
using Splat;
using Syncfusion.Blazor;
using usbprison;
using usbprison.blazor;
using usbprison.lib.Models;
using usbprison.lib.Services;

var rxuibuilder = RxAppBuilder.CreateReactiveUIBuilder()
    .WithBlazorWasm()
    .WithViewsFromAssembly(typeof(App).Assembly)
    .WithRegistration(locator =>
    {
        locator.RegisterLazySingleton<ReceiverViewModel>(() => new ReceiverViewModel());
        locator.RegisterLazySingleton<LogViewModel>(() => new LogViewModel());
    })
    .BuildApp();

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSyncfusionBlazor();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


var settingsService = new SettingsService(true);
Locator.CurrentMutable.RegisterConstant<ISettingsService>(settingsService);

var deviceInfo = new GenericDeviceInfo
{
    Name = "Browser",
    Manufacturer = "etc",
    Model = "etc",
    Version = "etc",
    Platform = "etc",
    Idiom = "etc",
    DeviceType = "etc"

};
var ipService = new IPService();
Locator.CurrentMutable.RegisterConstant<IIPService>(ipService);

var udpService = new UDPServiceEx(deviceInfo, ipService);
Locator.CurrentMutable.RegisterConstant<UDPService>(udpService);

//Locator.CurrentMutable.RegisterLazySingleton<ReceiverViewModel>(() => new ReceiverViewModel());

Log.Logger = new LoggerConfiguration()
     .MinimumLevel.Verbose() // Verbose includes Trace and Debug
     .WriteTo.Sink(new BrowserConsoleSink())
     .CreateLogger();

Log.Information("test!");

await builder.Build().RunAsync();
