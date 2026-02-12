using ReactiveUI;
using ReactiveUI.Builder;
using ReactiveUI.TerminalGui;
using Serilog;
using Splat;
using System.Reactive.Concurrency;
using System.Reflection;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using usbprison;
using usbprison.lib.Models;
using usbprison.lib.Services;
//using Splat.Serilog;


//Logging.Logger = CreateLogger();
Log.Logger = new LoggerConfiguration()
         .MinimumLevel.Verbose() // Verbose includes Trace and Debug
         .WriteTo.File("logs/logfile.txt", rollingInterval: RollingInterval.Day)
         .CreateLogger();


using (Globals.App)
{
    var rxapp = RxAppBuilder.CreateReactiveUIBuilder()     
            //.WithViewsFromAssembly(typeof(Program).Assembly)
            .WithRegistration(locator =>
            {
                locator.RegisterLazySingleton<MainViewModel>(() => new MainViewModel());
                locator.RegisterLazySingleton<DevicesViewModel>(() => new DevicesViewModel());
                locator.RegisterLazySingleton<TrackingViewModel>(() => new TrackingViewModel());
                locator.RegisterLazySingleton<ScheduleViewModel>(() => new ScheduleViewModel());

                locator.RegisterLazySingleton<ReceiverViewModel>(() => new ReceiverViewModel());
                locator.RegisterLazySingleton<LogViewModel>(() => new LogViewModel());
                //locator.RegisterViewForViewModel<RootPage,RootViewModel>();
                //locator.RegisterViewForViewModel<MainView, MainViewModel>(); 
                //locator.RegisterViewForViewModel<SingleDeviceView, SingleDeviceViewModel>();
                ////locator.RegisterViewForViewModel<DevicesView, DevicesViewModel>();
                //locator.RegisterViewForViewModel<TrackingView, TrackingViewModel>();
            })
            .BuildApp();

    RxApp.MainThreadScheduler = TerminalScheduler.Default;
    RxApp.TaskpoolScheduler = TaskPoolScheduler.Default;
    //Splat.Locator.CurrentMutable.UseSerilogFullLogger();
    Splat.Locator.CurrentMutable.UnregisterAll<IActivationForViewFetcher>();
    Splat.Locator.CurrentMutable.RegisterConstant<IActivationForViewFetcher>(new ActivationForViewFetcher());

    var ipService = new IPService();
    Locator.CurrentMutable.RegisterConstant<IIPService>(ipService);

    var databaseService = new DatabaseService("Data.sqlite");
    Locator.CurrentMutable.RegisterConstant<DatabaseService>(databaseService);

    
    var settingsService = new SettingsService(true);
    Locator.CurrentMutable.RegisterConstant<ISettingsService>(settingsService);

    var deviceInfo = new GenericDeviceInfo
    {
        Name = Environment.MachineName,
        Manufacturer = "",
        Model = "",
        Version = Environment.OSVersion.VersionString,
        Platform = "",
        Idiom = "",
        DeviceType = ""

    };
    var udpService = new UDPService(deviceInfo,ipService);
    Locator.CurrentMutable.RegisterConstant<UDPService>(udpService);

    Splat.Locator.CurrentMutable.RegisterConstant<DebugService>(new DebugService());
    Splat.Locator.CurrentMutable.RegisterConstant<IUSBService>(new USBService());

    var monitoringService = new MonitoringService(deviceInfo, settingsService, udpService, databaseService);
    Splat.Locator.CurrentMutable.RegisterConstant<MonitoringService>(monitoringService);

    var reportService = new ReportService(databaseService, monitoringService);
    Locator.CurrentMutable.RegisterConstant<ReportService>(reportService);


    Splat.Locator.CurrentMutable.Register<SingleDeviceView>(() => new SingleDeviceView());



    var top = new Window();
    top.Add(new MainView(new MainViewModel()));
    Globals.App.Run(top);
    top.Dispose();

}

public static class Globals
{
    private static IApplication? _app;
    public static IApplication App => _app ??= Application.Create().Init();


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