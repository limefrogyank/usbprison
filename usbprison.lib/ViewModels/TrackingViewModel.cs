using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Serilog;
using Splat;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using usbprison.lib.Models;
using usbprison.lib.Services;

namespace usbprison
{
    public class TrackingViewModel : ReactiveObject
    {
        private readonly IUSBService _usbService;
        private readonly ISettingsService _settingsService;
        private readonly UDPService _udpService;

        public DynamicData.Binding.ObservableCollectionExtended<TrackedDeviceViewModel> TrackedDevices { get; } = new DynamicData.Binding.ObservableCollectionExtended<TrackedDeviceViewModel>();
        
        private Subject<Unit> _manualRefreshSubject = new Subject<Unit>();
        public IObservable<Unit> ManualRefreshRequested => _manualRefreshSubject.AsObservable();

        public ReadOnlyObservableCollection<GroupedItems<TrackedDeviceViewModel, string,bool>> Groups { get; }

        public TrackingViewModel()
        {
            var service = Splat.Locator.Current.GetService(typeof (IUSBService)) as IUSBService;
            _usbService = service!;
            var settingsService = Splat.Locator.Current.GetService(typeof(ISettingsService)) as ISettingsService;
            _settingsService = settingsService!;
            var udpservice = Locator.Current.GetService<UDPService>();
            _udpService = udpservice!;

            var transformedTrackedDevices = _settingsService.TrackedDevices.Connect()
                .Transform(dev => new TrackedDeviceViewModel(dev))
                .AutoRefresh(x => x.IsPluggedIn)
                .Do(x =>
                {
                    _manualRefreshSubject.OnNext(Unit.Default);
                    Log.Information("TrackingViewModel detected changed to tracked devices: " + x.Count);
                }).Publish();

            transformedTrackedDevices.ObserveOn(RxApp.MainThreadScheduler)
                .Bind(TrackedDevices)
                .Subscribe();

            //transformedTrackedDevices.GroupOnProperty(x=>x.IsPluggedIn)
            //    .Transform(g =>
            //    {
            //        //g.Cache.Connect()
            //        //    .ToCollection()
            //        //    .Subscribe(x =>
            //        //    {
            //        //        Debug.WriteLine($"Got {x.Count} items");
            //        //    });

            //        DynamicData.Binding.ObservableCollectionExtended<TrackedDeviceViewModel> grouped = new  DynamicData.Binding.ObservableCollectionExtended<TrackedDeviceViewModel>();
            //        var sub = g.Cache.Connect()
            //        //    .ObserveOn(RxSchedulers.MainThreadScheduler)
            //            .SortAndBind(grouped, SortExpressionComparer<TrackedDeviceViewModel>.Ascending(x => x.DisplayName))
            //            .Subscribe();
            //        return new GroupedItems<TrackedDeviceViewModel>(g.Key ? "Imprisoned" : "Free", grouped);
            //    })
            //     //   .Sort(SortExpressionComparer<TrackedDeviceViewModel>.Ascending(x=>x.DisplayName))))
            //    .ObserveOn(RxSchedulers.MainThreadScheduler)
            //    .Bind(out var groups)
            //    .Subscribe();
            //Groups = groups;

            transformedTrackedDevices.Connect();

            //var timer = new System.Timers.Timer(5000); //every 5 seconds
            //timer.Elapsed += async (s, e) =>
            //{
            //    // regular app use
            //    await _udpService.BroadcastMessageAsync(new lib.Models.UDPMessage
            //    {
            //        MessageType = lib.Models.UDPMessageType.Notify,
            //        Message = $"There are {TrackedDevices.Where(x => x.IsPluggedIn).Count()} device(s) in prison"
            //    });
            //    await _udpService.BroadcastMessageAsync(new lib.Models.UDPMessage 
            //    { 
            //        MessageType = lib.Models.UDPMessageType.List, 
            //        PluggedDevices = TrackedDevices.Where(x => x.IsPluggedIn).Select(x=> x.Device).ToList(),
            //        MissingDevices = TrackedDevices.Where(x => !x.IsPluggedIn).Select(x => x.Device).ToList()
            //    });

            //    // background monitoring use
            //    await _udpService.BroadcastMessageAsync(new lib.Models.UDPMessage
            //    {
            //        MessageType = lib.Models.UDPMessageType.Alert,
            //        Message = $"There are {TrackedDevices.Where(x => x.IsPluggedIn).Count()} device(s) in prison",
            //        PluggedDevices = TrackedDevices.Where(x => x.IsPluggedIn).Select(x => x.Device).ToList(),
            //        MissingDevices = TrackedDevices.Where(x => !x.IsPluggedIn).Select(x => x.Device).ToList()
            //    });
               

            //};
            //timer.Enabled = true;
        }
    }
}