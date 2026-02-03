using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using usbprison.lib.Models;
using Serilog;
using usbprison.lib.Services;
using DynamicData.Binding;
using System.Diagnostics;

namespace usbprison
{
    public partial class ReceiverViewModel : ReactiveObject
    {
        private readonly UDPService _udpService;

        [ObservableAsProperty] private string _latestMessage = string.Empty;

        SourceCache<MultiTrackedDeviceViewModel, string> _devicesCache = new SourceCache<MultiTrackedDeviceViewModel, string>(x=>x.Device.Id);

        //public ReadOnlyObservableCollection<SimpleTrackedDeviceViewModel> Devices { get; }
        public ReadOnlyObservableCollection<GroupedMultiTrackedViewModel> GroupedDevices { get; }

        private readonly Interaction<UDPMessage, Task> testNotification = new Interaction<UDPMessage, Task>();
        public Interaction<UDPMessage, Task> TestNotification => this.testNotification;
        public ReceiverViewModel()
        {
            var udpservice = Locator.Current.GetService<UDPService>();
            _udpService = udpservice!;

            _devicesCache.Connect()
                .OnItemAdded(x =>
                {
                    x.SelfClear(_devicesCache);
                })
                .Group(x=>x.InPrison)
                .Transform(x=>new GroupedMultiTrackedViewModel(x.Key ? "In Prison" : "Free", x, RxSchedulers.MainThreadScheduler))
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .SortAndBind(out var devices, SortExpressionComparer<GroupedMultiTrackedViewModel>.Descending(x=>x.Name))
                .Subscribe();
            GroupedDevices = devices;

            _latestMessageHelper = _udpService.NotificationReceived.ObserveOn(RxSchedulers.MainThreadScheduler).ToProperty(this, x => x.LatestMessage);

            _udpService.LastestDevicesReceived.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(async devices =>
            {
               
                var lastDevices = _devicesCache.Items;
                var currentDevices = devices.ToList();


                var matchingPairs = devices.Join(lastDevices, x => x.Id, x => x.Id, (x, y) => new { Current = x, Previous = y });

                foreach (var pair in matchingPairs)
                {
                    if (pair.Previous.InPrison == true && pair.Current.InPrison == false && pair.Previous.MachineId != pair.Current.MachineId)
                    {
                        // revert to previous state avoid wrong grouping
                        //pair.Current.InPrison = true;
                        //pair.Current.SetOnlyMachine(pair.Previous.MachineId);
                        currentDevices.Remove(pair.Current);
                    }
                    else if (!pair.Previous.InPrison && !pair.Current.InPrison && pair.Previous.MachineId != pair.Current.MachineId)
                    {
                        pair.Current.AddMachines(pair.Previous.Machines);
                    }
                }





                _devicesCache.AddOrUpdate(currentDevices);

                //RxSchedulers.MainThreadScheduler.Schedule("TEST", (x,y) =>
                //{

                //await this.testNotification.Handle(new lib.Models.UDPMessage
                //{
                //    MessageType = lib.Models.UDPMessageType.Alert,
                //    Message = $"There are {Devices.Count()} device(s) in prison",
                //    Devices = Devices.Select(x => x.Device).ToList()
                //});
                
                //.Subscribe();
               // });
            });


            _ = _udpService.StartListening();
        }
    }
}
