using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Serilog;

namespace usbprison
{
    public partial class SimpleTrackedDeviceViewModel : ReactiveObject
    {
        public TrackedDeviceModel Device { get; }
        
        [ObservableAsProperty] private string _name = "";

        [ObservableAsProperty] private string _displayName = "";
        public string Id => Device.Id;
        public bool InPrison { get; set; }
        public string MachineId { get; set; } 

        public SimpleTrackedDeviceViewModel(TrackedDeviceModel device, bool inPrison, string machineId)
        {
            Device = device;
            InPrison = inPrison;
            MachineId = machineId;
            //var debugService = Splat.Locator.Current.GetService(typeof(DebugService)) as DebugService;
            Log.Information("created device: " + this.Device.Name);
            

            _displayNameHelper = this.WhenAnyValue(x => x.Device.Name, x => x.Device.CustomText).Select(x => string.IsNullOrWhiteSpace(x.Item2) ? x.Item1 : x.Item2)
                .ToProperty(this, x => x.DisplayName);

            
        }


    }
}