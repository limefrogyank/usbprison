using CommunityToolkit.Maui;
using MauiWifiManager;
using Microsoft.Extensions.Logging;

using ReactiveUI;
using ReactiveUI.Builder;
using Serilog;
using Shiny;
using Splat;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using System.Reactive.Concurrency;
using usbprison.lib.Models;
using usbprison.lib.Services;

namespace usbprison.maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseShiny() // THIS IS REQUIRED FOR SHINY ON MAUI, Shiny is a library for simpler notifications
                .UseMauiWifiManager() // needed to get IP address to calculate subnet mask for UDP broadcasting, helps get around virtual adapters which fail when using 255.255.255.255
                .UseMauiCommunityToolkit()  // MAYBE NOT NEEDED
                .ConfigureSyncfusionToolkit()  // MAYBE NOT NEEDED
                .ConfigureSyncfusionCore()  // MAYBE NOT NEEDED
                .ConfigureMauiHandlers(handlers =>
                {
#if WINDOWS
    				Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
    				{
    					handler.PlatformView.SingleSelectionFollowsFocus = false;
    				});


#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif
            var databaseService = new DatabaseService(Path.Combine(FileSystem.CacheDirectory, "Data.sqlite"));
            Locator.CurrentMutable.RegisterConstant<DatabaseService>(databaseService);

            var settingsService = new SettingsService(true);
            Locator.CurrentMutable.RegisterConstant<ISettingsService>(settingsService);

            var deviceInfo = new GenericDeviceInfo
            {
                Name = DeviceInfo.Name,
                Manufacturer = DeviceInfo.Manufacturer,
                Model = DeviceInfo.Model,
                Version = DeviceInfo.VersionString,
                Platform = DeviceInfo.Platform.ToString(),
                Idiom = DeviceInfo.Idiom.ToString(),
                DeviceType = DeviceInfo.DeviceType.ToString()

            };
            var ipService = new IPService();
            Locator.CurrentMutable.RegisterConstant<IIPService>(ipService);

            var udpService = new UDPService(deviceInfo, ipService);
            Locator.CurrentMutable.RegisterConstant<UDPService>(udpService);

#if WINDOWS
            var usbService = new USBService();
            Locator.CurrentMutable.RegisterConstant<IUSBService>(usbService);
            Locator.CurrentMutable.RegisterConstant<BroadcastService>( new BroadcastService(settingsService, udpService));
#else            
            builder.Services.AddNotifications();
#endif


            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Verbose() // Verbose includes Trace and Debug
                 .WriteTo.File(Path.Combine(FileSystem.Current.AppDataDirectory, "logs/logfile.txt"), rollingInterval: RollingInterval.Day)
                 .CreateLogger();

            var app = RxAppBuilder.CreateReactiveUIBuilder()
            .WithMaui()
            .WithViewsFromAssembly(typeof(App).Assembly)
            .WithRegistration(locator =>
            {
#if WINDOWS
                locator.RegisterLazySingleton<MainViewModel>(() => new MainViewModel());
                locator.RegisterLazySingleton<DevicesViewModel>(()=> new DevicesViewModel());
                locator.RegisterLazySingleton<TrackingViewModel>(() => new TrackingViewModel());
                locator.RegisterLazySingleton<ScheduleViewModel>(() => new ScheduleViewModel());
#endif
                locator.RegisterLazySingleton<ReceiverViewModel> (()=> new ReceiverViewModel());
                locator.RegisterLazySingleton<LogViewModel>(() => new LogViewModel());
                //locator.RegisterViewForViewModel<RootPage,RootViewModel>();
                //locator.RegisterViewForViewModel<MainView, MainViewModel>(); 
                //locator.RegisterViewForViewModel<SingleDeviceView, SingleDeviceViewModel>();
                ////locator.RegisterViewForViewModel<DevicesView, DevicesViewModel>();
                //locator.RegisterViewForViewModel<TrackingView, TrackingViewModel>();
            })
            .BuildApp();

#if WINDOWS
            RxSchedulers.MainThreadScheduler = new WaitForDispatcherScheduler(static () => DispatcherQueueScheduler.Current);
            var s = RxSchedulers.MainThreadScheduler.GetType();
#elif ANDROID
            RxSchedulers.MainThreadScheduler = HandlerScheduler.MainThreadScheduler;
#endif
           

            return builder.Build();
        }
    }
}
