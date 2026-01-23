using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Serilog;

namespace usbprison
{
    public partial class TrackedDeviceViewModel: ReactiveObject
    {
        public TrackedDeviceModel Device { get; }
        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

        [ObservableAsProperty] private bool _isPluggedIn;
        private readonly IUSBService _usbService;

        [ObservableAsProperty] private string _name = "";

        [ObservableAsProperty] private string _displayName = "";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public TrackedDeviceViewModel(TrackedDeviceModel device)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            Device = device;
            //var debugService = Splat.Locator.Current.GetService(typeof(DebugService)) as DebugService;
            Log.Information("created device: " + this.Device.Name);
            var service = Splat.Locator.Current.GetService(typeof (IUSBService)) as IUSBService;
            _usbService = service!;

            
            var currentList = _usbService.DeviceCache.Connect().ToCollection().StartWithEmpty();

            _isPluggedInHelper = currentList.Select(x=> 
                {
                    //Log.Information("device tracked possible change: " + this.Device.Name);
                    
                    //Terminal.Gui.Views.MessageBox.Query(Globals.App,"Change", "Changed!", "Ok");
                    if (x == null) return false;
                    return x.FirstOrDefault(curr => curr.Id == this.Device.Id && curr.SerialNumber == this.Device.SerialNumber) != null;
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.IsPluggedIn);

            _displayNameHelper = this.WhenAnyValue(x => x.Device.Name, x => x.Device.CustomText).Select(x => string.IsNullOrWhiteSpace(x.Item2) ? x.Item1 : x.Item2)
                .ToProperty(this, x => x.DisplayName);

            RemoveCommand = ReactiveCommand.Create(() =>
            {
                var settingsService = Splat.Locator.Current.GetService(typeof(ISettingsService)) as ISettingsService;
                settingsService!.TrackedDevices.Remove(Device);
            }, outputScheduler: RxSchedulers.MainThreadScheduler);
        }


    }
}