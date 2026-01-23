using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables.Fluent;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace usbprison
{
    public partial class DevicesViewModel : ReactiveObject
    {
        private readonly IUSBService uSBService;
        private readonly ISettingsService _settingsService;

        public ReactiveCommand<int?, Unit> ListViewSelectionChangedCommand { get; private set; }
       

        public ObservableCollectionExtended<DeviceModel> Devices => uSBService.Devices;

        [Reactive] private DeviceModel? _selectedDevice;

        [ObservableAsProperty] private SingleDeviceViewModel? _singleDeviceViewModel = null;

        // public DeviceModel? SelectedDevice
        // {
        //     get => _selectedDevice;
        //     set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
        // }



        // public bool IsSelectedDeviceTracked
        // {
        //     get
        //     {
        //         if (SelectedDevice == null) return false;
        //         return _settingsService.TrackedDevices.Lookup(SelectedDevice.Id ?? string.Empty).HasValue;
        //     }
        // }

        public DevicesViewModel()
        {
            var service = Splat.Locator.Current.GetService(typeof (IUSBService)) as IUSBService;
            uSBService = service!;
            var settingsService = Splat.Locator.Current.GetService(typeof(ISettingsService)) as ISettingsService;
            _settingsService = settingsService!;

            // _isSelectedDeviceTrackedHelper = this.WhenAnyValue(x => x.SelectedDevice).Select(x =>
            //     {
            //         if (x == null) return false;
            //         return _settingsService.TrackedDevices.Lookup(x.Id ?? string.Empty).HasValue;
            //     })
            //     .ToProperty(this, x => x.IsSelectedDeviceTracked);

            ListViewSelectionChangedCommand = ReactiveCommand.Create<int?>((index) =>
            {
                if (index.HasValue)
                {
                    SelectedDevice = uSBService.Devices[index.Value];
                }
                else
                {
                    SelectedDevice = null;
                }
            });

            _singleDeviceViewModelHelper = this.WhenAnyValue(x => x.SelectedDevice)
                .WhereNotNull()
                .Select(x =>
                {
                    var viewModel = new SingleDeviceViewModel(x);
                    return viewModel;
                })
                .ToProperty(this, x => x.SingleDeviceViewModel);

        }

        
    }
}