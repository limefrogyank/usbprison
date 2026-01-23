using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

using ReactiveUI;
using ReactiveUI.Builder;
using Serilog;
using Shiny;
using Splat;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using System.Reactive.Concurrency;
using usbprison.lib.Services;
using usbprison.maui.Pages;
using usbprison.maui.Services;

namespace usbprison.maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseShiny() // THIS IS REQUIRED FOR SHINY ON MAUI
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureSyncfusionCore()
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
            Locator.CurrentMutable.RegisterConstant<UDPService>(new UDPService());
#if WINDOWS

            Locator.CurrentMutable.RegisterConstant<IUSBService>(new USBService());
#endif
            
            Locator.CurrentMutable.RegisterConstant<ISettingsService>( new SettingsService(true));

#if !WINDOWS
            builder.Services.AddNotifications();
            //builder.Services.AddJob(ReceiveUDPJob.Job);
#endif
            //builder.Services.AddSingleton<ProjectRepository>();
            //builder.Services.AddSingleton<TaskRepository>();
            //builder.Services.AddSingleton<CategoryRepository>();
            //builder.Services.AddSingleton<TagRepository>();
            //builder.Services.AddSingleton<SeedDataService>();
            //builder.Services.AddSingleton<ModalErrorHandler>();
            //builder.Services.AddSingleton<MainPageModel>();
            //builder.Services.AddSingleton<ProjectListPageModel>();
            //builder.Services.AddSingleton<ManageMetaPageModel>();

            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Verbose() // Verbose includes Trace and Debug
                 .WriteTo.File(Path.Combine(FileSystem.Current.AppDataDirectory, "logs/logfile.txt"), rollingInterval: RollingInterval.Day)
                 .CreateLogger();

            var app = RxAppBuilder.CreateReactiveUIBuilder()
            .WithMaui()
            .WithViewsFromAssembly(typeof(App).Assembly)
            .WithRegistration(locator =>
            {
                locator.RegisterLazySingleton<MainViewModel>(() => new MainViewModel());
                locator.RegisterLazySingleton<DevicesViewModel>(()=> new DevicesViewModel());
                locator.RegisterLazySingleton<TrackingViewModel>(() => new TrackingViewModel());

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
            //builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
            //builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");
            //builder.Services.AddSingleton<RootViewModel>();

            //            builder.Services.AddTransient<IViewFor<SingleDeviceViewModel>>(x=> new SingleDeviceView());

            return builder.Build();
        }
    }
}
