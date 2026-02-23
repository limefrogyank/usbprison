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

        public bool IsLockdown { get; set; }
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
            _update.Select(x => Observable.Timer(TimeSpan.FromSeconds(10))).Switch().Subscribe(_ => 
            { 
                cache.Remove(this); 
            });
        }

        [Reactive] private double _timeLeft = ReceiverViewModel.TimeoutSeconds;
        [ObservableAsProperty] private double _timeLeftDouble = 1.0;

        public MultiTrackedDeviceViewModel(TrackedDeviceModel device, bool inPrison, bool isLockdown, string machineId)
        {
            Device = device;
            InPrison = inPrison;
            IsLockdown = isLockdown;
            MachineId = machineId;

            var interval = Observable.Interval(TimeSpan.FromSeconds(1)).ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(x => TimeLeft--);
            Observable.Timer(TimeSpan.FromSeconds(ReceiverViewModel.TimeoutSeconds)).Subscribe(x => interval.Dispose());
            //var debugService = Splat.Locator.Current.GetService(typeof(DebugService)) as DebugService;

            _timeLeftDoubleHelper = this.WhenAnyValue(x=> x.TimeLeft).Select(x=> x/(double)ReceiverViewModel.TimeoutSeconds).ToProperty(this, x => x.TimeLeftDouble);

            _machinesCache.Connect()
                
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