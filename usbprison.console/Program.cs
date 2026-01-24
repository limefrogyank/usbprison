using usbprison;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ReactiveUI;
using System.Reactive.Concurrency;

using Microsoft.Extensions.Logging;
using Splat;
using Serilog;
using System.Reflection;
using Microsoft.JavaScript.NodeApi.Runtime;
using Microsoft.JavaScript.NodeApi;
using usbprison.lib.Services;
using ReactiveUI.TerminalGui;
//using Splat.Serilog;


//Logging.Logger = CreateLogger();
Log.Logger = new LoggerConfiguration()
         .MinimumLevel.Verbose() // Verbose includes Trace and Debug
         .WriteTo.File("logs/logfile.txt", rollingInterval: RollingInterval.Day)
         .CreateLogger();


// // Find the path to the libnode binary for the current platform.
// string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
// string libnodePath = Path.Combine(baseDir,  "libnode.dll");
// NodeEmbeddingPlatformSettings platformSettings = new NodeEmbeddingPlatformSettings
// {
//     LibNodePath = libnodePath
// };
// NodeEmbeddingPlatform nodejsPlatform = new(platformSettings);
// Globals.NodejsRuntime = nodejsPlatform.CreateThreadRuntime(baseDir,
//     new NodeEmbeddingRuntimeSettings
//     {
//         MainScript =
//             "globalThis.require = require('module').createRequire(process.execPath);\n"
//     });


using (Globals.App)
{
    RxApp.MainThreadScheduler = TerminalScheduler.Default;
    RxApp.TaskpoolScheduler = TaskPoolScheduler.Default;
    //Splat.Locator.CurrentMutable.UseSerilogFullLogger();
    Splat.Locator.CurrentMutable.UnregisterAll<IActivationForViewFetcher>();
    Splat.Locator.CurrentMutable.RegisterConstant<IActivationForViewFetcher>(new ActivationForViewFetcher());
    Splat.Locator.CurrentMutable.RegisterConstant<UDPService>(new UDPService());
    Splat.Locator.CurrentMutable.RegisterConstant<DebugService>(new DebugService());
    Splat.Locator.CurrentMutable.RegisterConstant<IUSBService>(new USBService());
    Splat.Locator.CurrentMutable.RegisterConstant<ISettingsService>(new SettingsService(true));
    
    Splat.Locator.CurrentMutable.Register<SingleDeviceView>(() => new SingleDeviceView());

    // Globals.NodejsRuntime.RunAsync(() =>
    // {
    //     JSValue sendmailPackage = Globals.NodejsRuntime.Import("sendmail");
    //     sendmailPackage.As<
    //     JSValue.RunScript("console.log('Hello from JS!');");
    //     return Task.CompletedTask;
    // });

    var top = new Window();
    top.Add(new MainView(new MainViewModel()));
    Globals.App.Run(top);
    top.Dispose();
    //Globals.NodejsRuntime.Dispose();
}

public static class Globals
{
    private static IApplication? _app;
    public static IApplication App => _app ??= Application.Create().Init();

    public static NodeEmbeddingThreadRuntime? NodejsRuntime {get;set;} = null;
}


// Application.Init();

// try
// {
//     Application.Run(new MyView());
// }
// finally
// {
//     Application.Shutdown();
// }