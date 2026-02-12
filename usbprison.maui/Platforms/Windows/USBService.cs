using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Timers;
using DynamicData;
//using LibUsbDotNet;
//using LibUsbDotNet.LibUsb;
//using LibUsbDotNet.Info;
//using LibUsbDotNet.Main;
using DynamicData.Binding;
using System.Reactive.Linq;
using ReactiveUI;
using System.Reactive.Concurrency;
using Serilog;
using Microsoft.Maui.Controls;
using Splat;
using usbprison.lib.Services;
using System.Text.RegularExpressions;
using System.Globalization;





#if WINDOWS
using System.Management;
#endif

namespace usbprison
{
    public class USBService : IUSBService
    {
        //private System.Reactive.Subjects.Subject<UsbDevice> usbDevices = new System.Reactive.Subjects.Subject<UsbDevice>();

        public DynamicData.Binding.ObservableCollectionExtended<DeviceModel> Devices { get; } = new DynamicData.Binding.ObservableCollectionExtended<DeviceModel>();
        private System.Timers.Timer _timer;

        private SourceCache<DeviceModel, string> _deviceCache = new SourceCache<DeviceModel, string>(dev => dev.Id ?? Guid.NewGuid().ToString());
        public ISourceCache<DeviceModel, string> DeviceCache => _deviceCache;

        private Subject<string> _deviceEvents = new Subject<string>();
   

        public IObservable<string> DeviceEvents => _deviceEvents;

        private Regex IdParseRegex = new Regex(@"USB\\VID_(\w{4})&PID_(\w{4})(?:&MI_(\w{2}))?\\(.+)");

        public USBService()
        {
            _deviceCache.Connect()
                
                //.ObserveOn(scheduler)
                .Bind(Devices)
                .Subscribe();

            _timer = new System.Timers.Timer(5000);
            _timer.AutoReset = true;
            _timer.Elapsed += Timer_Elapsed;
            _timer.Enabled = true;
#pragma warning disable CS8604 // Possible null reference argument.
            Timer_Elapsed(this, ElapsedEventArgs.Empty as ElapsedEventArgs);
#pragma warning restore CS8604 // Possible null reference argument.

            _deviceEvents.Subscribe(async x =>
            {
                Log.Information(x);
                //await _udpService.BroadcastMessageAsync(new lib.Models.UDBMessage { MessageType = lib.Models.UDBMessageType.Notify, Message=x});
            });
        }

        public void Test()
        {
            _deviceEvents.OnNext("READY");
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var devicesPresent = new List<DeviceModel>();
            _deviceEvents.OnNext("CLEAR");
            using (var searcher = new ManagementObjectSearcher(
            "SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE 'USB%'"))
            {
                
                foreach (var device in searcher.Get())
                {                    
                    var id = (string)device.GetPropertyValue("DeviceID");
                    var pnp = (string)device.GetPropertyValue("PNPDeviceID");
                    var desc = (string)device.GetPropertyValue("Description");
                    //var serialNumber = (string)device.GetPropertyValue("HardwareID");
                    _deviceEvents.OnNext(string.Format("DeviceID: {0}, PNPDeviceID: {1}, Description: {2}", id, pnp, desc));
                    var deviceModel = new DeviceModel(desc, 0, 0, id);
                    deviceModel.WindowsId = id;

                    var match = IdParseRegex.Match(id);
                    if (match.Success && match.Groups.Count == 5)
                    {
                        ushort.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var vid);
                        deviceModel.Vid = vid;
                        ushort.TryParse(match.Groups[2].Value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var pid);
                        deviceModel.Pid = pid;
                        ushort.TryParse(match.Groups[3].Value, out var mi);
                        deviceModel.Mi = mi;
                        deviceModel.SerialNumber = match.Groups[4].Value.ToString();


                        devicesPresent.Add(deviceModel);
                    }

                    
                }
                //_deviceCache.AddOrUpdate(devicesPresent);
            }
                        
            //compare current devices to existing devices
            var lastDevices = _deviceCache.Items.ToList();
            var toRemove = lastDevices.Where(ed => !devicesPresent.Any(dp => dp.Id == ed.Id)).ToList();
            var toAdd = devicesPresent.Where(ed => !lastDevices.Any(ld => ld.Id == ed.Id)).ToList();
            _deviceCache.Edit(updater =>
            {
                updater.RemoveKeys(toRemove.Select(ed => ed.Id));
                updater.AddOrUpdate(toAdd);
            });
           
        }


    }
}