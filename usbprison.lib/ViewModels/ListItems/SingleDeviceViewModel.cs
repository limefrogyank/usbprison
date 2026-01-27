using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using LibUsbDotNet;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace usbprison
{
    public partial class SingleDeviceViewModel : ReactiveObject, IActivatableViewModel
    {
        private readonly ISettingsService _settingsService;
        public ReactiveCommand<Unit, Unit> ActivateDeviceCommand { get; }
        public ReactiveCommand<Unit, Unit> DeactivateDeviceCommand { get; }
        public DeviceModel Device { get; set; }

        public ViewModelActivator Activator => new ViewModelActivator();

        [Reactive] private bool _isDeviceTracked;

        [Reactive] private string _customText = "";

        public string Test { get; set; } = "TEST";

        public SingleDeviceViewModel(DeviceModel device)
        {
            Device = device;
            //if (Device == null) return;

            //CustomText = Device.CustomText ?? "";

            var settingsService = Splat.Locator.Current.GetService(typeof(ISettingsService)) as ISettingsService;
            _settingsService = settingsService!;
            
            _settingsService.TrackedDevices.Connect()
                .ToCollection()
                .Subscribe(x =>
                {
                    IsDeviceTracked = x.Any(d => d.Id == Device.Id);
                });


            this.WhenAnyValue(x=>x.CustomText).Subscribe(async x=> {
                //Device.CustomText = x;
                if (_settingsService != null)   
                    await _settingsService.SaveSettingsAsync();
            });

            // _isDeviceTracked = _settingsService.TrackedDevices.Lookup(Device.Id ?? string.Empty).HasValue;
            ActivateDeviceCommand = ReactiveCommand.Create(() =>
            {
                _settingsService.TrackedDevices.AddOrUpdate(new TrackedDeviceModel(Device) { CustomText = CustomText});
            }, outputScheduler: RxApp.MainThreadScheduler);
            DeactivateDeviceCommand = ReactiveCommand.Create(() =>
            {
                _settingsService.TrackedDevices.RemoveKey(Device.Id);
            }, outputScheduler: RxApp.MainThreadScheduler);
        }
    }
}