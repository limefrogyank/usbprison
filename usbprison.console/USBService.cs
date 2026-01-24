using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Timers;
using DynamicData;
using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using DynamicData.Binding;
using System.Reactive.Linq;
using ReactiveUI;
using System.Reactive.Concurrency;
using Serilog;

namespace usbprison
{
    public class USBService : IUSBService
    {
        private System.Reactive.Subjects.Subject<UsbDevice> usbDevices = new System.Reactive.Subjects.Subject<UsbDevice>();

        public DynamicData.Binding.ObservableCollectionExtended<DeviceModel> Devices { get; } = new DynamicData.Binding.ObservableCollectionExtended<DeviceModel>();
        private System.Timers.Timer _timer;

        private SourceCache<DeviceModel, string> _deviceCache = new SourceCache<DeviceModel, string>(dev => dev.Id ?? Guid.NewGuid().ToString());
        public ISourceCache<DeviceModel, string> DeviceCache => _deviceCache;

        private Subject<string> _deviceEvents = new Subject<string>();
        public IObservable<string> DeviceEvents => _deviceEvents;

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
            Timer_Elapsed(this, (ElapsedEventArgs.Empty as ElapsedEventArgs)!);
            // notifier = LibUsbDotNet.DeviceNotify.DeviceNotifier();
            // notifier = new LibUsbDotNet.DeviceNotify.Linux.LinuxDeviceNotifier();
            // notifier.OnDeviceNotify += OnDeviceNotifyEvent;

            _deviceEvents.Subscribe(ev =>
            {
                //Log.Information("USB Event: " + ev);
            });
        }

        public void Test()
        {
            _deviceEvents.OnNext("READY");
        }

        public void GetInfoForDevice(DeviceModel device)
        {
            using (UsbContext context = new UsbContext())
            {
                var finder = new UsbDeviceFinder() { Vid = device.Vid, Pid = device.Pid , SerialNumber = device.SerialNumber};
                using (var foundDevice = context.Find(finder))
                {
                    //  foundDevice.
                }

            }
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            _deviceEvents.OnNext("CLEAR");
            var devicesPresent = new List<DeviceModel>();

            using (UsbContext context = new UsbContext())
            {
                using (var allDevices = context.List())
                {
                    _deviceEvents.OnNext(string.Format("Found {0} Devices", allDevices.Count));
                    foreach (UsbDevice device in allDevices)
                    {
                        bool openedDevice = device.TryOpen();

                        _deviceEvents.OnNext(device.Info.ToString() + "\n");

                        _deviceEvents.OnNext($"LocationId: {device.BusNumber}");
                        for (int i = 0; i < device.PortNumbers.Count; i++)
                        {
                            if (i == 0)
                                _deviceEvents.OnNext("-");
                            _deviceEvents.OnNext($"{device.PortNumbers[i]}");
                            if (i != device.PortNumbers.Count - 1)
                                _deviceEvents.OnNext(".");
                        }
                        _deviceEvents.OnNext("\n");

                        //Log.Information("Device Found: {0}", device.Info.Product ?? "Unknown");
                        devicesPresent.Add(new DeviceModel(device.Info.Product ?? "Unknown", device.ProductId, device.VendorId, device.Info.SerialNumber));
                        

                        if (!openedDevice)
                        {
                            _deviceEvents.OnNext(new string('-', 50) + "\n");
                            continue;
                        }



                        foreach (var configInfo in device.Configs)
                        {
                            _deviceEvents.OnNext($"\t{configInfo.ToString().ReplaceLineEndings("\n\t")}");

                            ReadOnlyCollection<UsbInterfaceInfo> interfaceList = configInfo.Interfaces;
                            for (int iInterface = 0; iInterface < interfaceList.Count; iInterface++)
                            {
                                UsbInterfaceInfo interfaceInfo = interfaceList[iInterface];
                                _deviceEvents.OnNext($"\t\t{interfaceInfo.ToString().ReplaceLineEndings("\n\t\t")}");

                                ReadOnlyCollection<UsbEndpointInfo> endpointList = interfaceInfo.Endpoints;
                                for (int iEndpoint = 0; iEndpoint < endpointList.Count; iEndpoint++)
                                {
                                    _deviceEvents.OnNext($"\t\t\tEndpoint: {iEndpoint}");
                                    _deviceEvents.OnNext($"\t\t\t{endpointList[iEndpoint].ToString().ReplaceLineEndings("\n\t\t\t")}");
                                }
                            }
                        }

                        device.Close();
                        _deviceEvents.OnNext(new string('-', 50) + "\n");
                    }
                }
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

        // void OnDeviceNotifyEvent(object? sender, DeviceNotifyEventArgs e)
        // {

        //     Console.SetCursorPosition(0, Console.CursorTop);

        //     _deviceEvents.OnNext(e.Object is UsbDevice device ? $"Device: {device.Info}" : "No device info");
        //     _deviceEvents.OnNext(e.ToString()); // Dump the event info to output.

        //     _deviceEvents.OnNext();
        //     //Console.Write("[Press any key to exit]");
        // }
    }
}