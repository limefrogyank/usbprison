using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
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
    public partial class TrackingViewModel : ReactiveObject
    {
        //private readonly IUSBService _usbService;
        //private readonly ISettingsService _settingsService;
        private readonly MonitoringService _monitoringService;
        //private readonly UDPService _udpService;

        public DynamicData.Binding.ObservableCollectionExtended<TrackedDeviceViewModel> TrackedDevices { get; } = new DynamicData.Binding.ObservableCollectionExtended<TrackedDeviceViewModel>();
        
        public IObservable<IReadOnlyCollection<TrackedDeviceViewModel>> TrackedDevicesObservable {get;set;}

        private Subject<Unit> _manualRefreshSubject = new Subject<Unit>();
        public IObservable<Unit> ManualRefreshRequested => _manualRefreshSubject.AsObservable();

        [Reactive] private TrackedDeviceViewModel? _selectedDevice;
       

        public TrackingViewModel()
        {
            //var service = Splat.Locator.Current.GetService(typeof (IUSBService)) as IUSBService;
            //_usbService = service!;
            //var settingsService = Splat.Locator.Current.GetService(typeof(ISettingsService)) as ISettingsService;
            //_settingsService = settingsService!;
            var monitoringService = Splat.Locator.Current.GetService<MonitoringService>();
            _monitoringService = monitoringService!;
            //var udpservice = Locator.Current.GetService<UDPService>();
            //_udpService = udpservice!;

            var transformedTrackedDevices = _monitoringService.TrackedDevicesCache.Connect()
                .Transform(dev => new TrackedDeviceViewModel(dev))
                .AutoRefresh(x => x.IsPluggedIn)
                
                .ObserveOn(RxApp.MainThreadScheduler)
                .Publish();

            transformedTrackedDevices
                .Bind(TrackedDevices)
                .Do(x =>
                {
                    _manualRefreshSubject.OnNext(Unit.Default);
                    Log.Information("TrackingViewModel detected changed to tracked devices: " + x.Count);
                })
                .Subscribe();


            TrackedDevicesObservable = transformedTrackedDevices.ToCollection();

            transformedTrackedDevices.Connect();
            
        }
    }
}