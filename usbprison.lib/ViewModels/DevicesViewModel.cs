using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables.Fluent;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;

namespace usbprison
{
    public partial class DevicesViewModel : ReactiveObject
    {
        private readonly IUSBService uSBService;
       // private readonly ISettingsService _settingsService;

        public ReactiveCommand<int?, Unit> ListViewSelectionChangedCommand { get; private set; }
       

        public ObservableCollectionExtended<DeviceModel> Devices1 => uSBService.Devices;

        public ReadOnlyObservableCollection<SingleDeviceViewModel> Devices { get; set; }

        [Reactive] private DeviceModel? _selectedDevice1;
        [Reactive] private SingleDeviceViewModel? _selectedDevice;

        [ObservableAsProperty] private SingleDeviceViewModel? _singleDeviceViewModel = null;

        public DevicesViewModel()
        {
            var service = Splat.Locator.Current.GetService(typeof (IUSBService)) as IUSBService;
            uSBService = service!;

            uSBService.DeviceCache.Connect()
                .Transform(x => new SingleDeviceViewModel(x))
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Bind(out var devices)
                .Subscribe();
            Devices = devices;


            ListViewSelectionChangedCommand = ReactiveCommand.Create<int?>((index) =>
            {
                if (index.HasValue)
                {
                    SelectedDevice = Devices[index.Value];
                    //SelectedDevice = uSBService.Devices[index.Value];
                }
                else
                {
                    SelectedDevice = null;
                }
            });

            _singleDeviceViewModelHelper = this.WhenAnyValue(x => x.SelectedDevice)
                .WhereNotNull()
                //.Select(x =>
                //{
                //    var viewModel = new SingleDeviceViewModel(x);
                //    return viewModel;
                //})
                .ToProperty(this, x => x.SingleDeviceViewModel);

        }

        
    }
}