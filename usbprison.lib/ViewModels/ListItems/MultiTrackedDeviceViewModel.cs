using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Serilog;

namespace usbprison
{
    public partial class MultiTrackedDeviceViewModel : ReactiveObject
    {
        public TrackedDeviceModel Device { get; }
        
        //[ObservableAsProperty] private string _name = "";

        [ObservableAsProperty] private string _displayName = "";
        public string Id => Device.Id;
        public bool InPrison { get; set; }
        public string MachineId { get; set; }

        private readonly IObservable<long> _timeout;
        private Subject<Unit> _update = new Subject<Unit>();

        public ReadOnlyObservableCollection<string> Machines { get; }

        private SourceCache<string, string> _machinesCache = new SourceCache<string, string>(x => x);
        

        public void AddMachines(IEnumerable<string> machineNames)
        {
            _machinesCache.AddOrUpdate(machineNames);
        }
        public void SetOnlyMachine(string machineName)
        {
            MachineId = machineName;
            _machinesCache.Edit((updater) =>
            {
                updater.Clear();
                updater.AddOrUpdate(machineName);
            });
        }

            
        public bool MachinesMatch(MultiTrackedDeviceViewModel other)
        {
            return new HashSet<string>(this._machinesCache.Items).SetEquals(other._machinesCache.Items);
        }

        public void Update()
        {
            _update.OnNext(Unit.Default);
        }
            
        public void SelfClear(SourceCache<MultiTrackedDeviceViewModel, string> cache)
        {
            _update.Select(x => Observable.Timer(TimeSpan.FromSeconds(10))).Switch().Subscribe(_ => cache.Remove(this));
        }

        public MultiTrackedDeviceViewModel(TrackedDeviceModel device, bool inPrison, string machineId)
        {
            Device = device;
            InPrison = inPrison;
            MachineId = machineId;

            _timeout = Observable.Timer(TimeSpan.FromSeconds(10));
            //var debugService = Splat.Locator.Current.GetService(typeof(DebugService)) as DebugService;

            _machinesCache.Connect()
                .ExpireAfter(x=>TimeSpan.FromMinutes(10), RxSchedulers.MainThreadScheduler)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .SortAndBind(out var machines, SortExpressionComparer<string>.Ascending(x => x))
                .Subscribe();

            _machinesCache.AddOrUpdate(machineId);

            Machines = machines;




            _displayNameHelper = this.WhenAnyValue(x => x.Device.Name, x => x.Device.CustomText).Select(x => string.IsNullOrWhiteSpace(x.Item2) ? x.Item1 : x.Item2)
                .ToProperty(this, x => x.DisplayName);

            
        }


    }
}