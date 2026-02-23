using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using usbprison.lib.Services;

namespace usbprison
{
    public partial class SingleDeviceViewModel : ReactiveObject, IActivatableViewModel
    {
        //private readonly ISettingsService _settingsService;
        public ReactiveCommand<Unit, Unit> ActivateDeviceCommand { get; }
        public ReactiveCommand<Unit, Unit> DeactivateDeviceCommand { get; }
        public DeviceModel Device { get; set; }

        public ViewModelActivator Activator => new ViewModelActivator();

        [Reactive] private bool _isDeviceTracked;

        [Reactive] private string _customText = "";
        private DatabaseService _databaseService;
        private readonly MonitoringService _monitoringService;

        [ObservableAsProperty] private string _name = string.Empty;

        public SingleDeviceViewModel(DeviceModel device)
        {
            Device = device;
            //if (Device == null) return;

            //CustomText = Device.CustomText ?? "";

            _nameHelper = this.WhenAnyValue(x => x.Device.Name, x => x.CustomText).Select(x => string.IsNullOrWhiteSpace(x.Item2) ? x.Item1 : $"{x.Item1} ({x.Item2})")
                .ToProperty(this, x => x.Name);

            //var settingsService = Splat.Locator.Current.GetService(typeof(ISettingsService)) as ISettingsService;
            //_settingsService = settingsService!;
            var databaseService = Locator.Current.GetService<DatabaseService>();
            _databaseService = databaseService!;
            var monitoringService = Locator.Current.GetService<MonitoringService>();
            _monitoringService = monitoringService!;

            _monitoringService.TrackedDevicesCache.Connect()
                .ToCollection()
                .Subscribe(x =>
                {                    
                    IsDeviceTracked = x.Any(d => d.Id == Device.Id);
                    if (IsDeviceTracked)
                    {
                        var custom = x.First(d => d.Id == Device.Id).CustomText;
                        CustomText = custom ?? string.Empty;
                    }
                });


            //this.WhenAnyValue(x=>x.CustomText).Subscribe(async x=> {
            //    //Device.CustomText = x;
            //    if (_settingsService != null)   
            //        await _settingsService.SaveSettingsAsync();
            //});

            // _isDeviceTracked = _settingsService.TrackedDevices.Lookup(Device.Id ?? string.Empty).HasValue;
            ActivateDeviceCommand = ReactiveCommand.Create(() =>
            {
                _monitoringService.TrackedDevicesCache.AddOrUpdate(new TrackedDeviceModel(Device) { CustomText = CustomText });
                //_settingsService.TrackedDevices.AddOrUpdate(new TrackedDeviceModel(Device) { CustomText = CustomText});
            }, outputScheduler: RxSchedulers.MainThreadScheduler);
            DeactivateDeviceCommand = ReactiveCommand.Create(() =>
            {
                _monitoringService.TrackedDevicesCache.RemoveKey(Device.Id);
                //_settingsService.TrackedDevices.RemoveKey(Device.Id);
            }, outputScheduler: RxSchedulers.MainThreadScheduler);
        }
    }
}