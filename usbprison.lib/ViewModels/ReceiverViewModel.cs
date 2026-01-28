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

namespace usbprison
{
    public partial class ReceiverViewModel : ReactiveObject
    {
        private readonly UDPService _udpService;

        [ObservableAsProperty] private string _latestMessage = string.Empty;

        SourceCache<SimpleTrackedDeviceViewModel, string> _devicesCache = new SourceCache<SimpleTrackedDeviceViewModel, string>(x=>x.Device.Id);

        public ReadOnlyObservableCollection<SimpleTrackedDeviceViewModel> Devices { get; }
        public ReadOnlyObservableCollection<GroupedItems<SimpleTrackedDeviceViewModel,string,bool>> GroupedDevices { get; }

        private readonly Interaction<UDPMessage, Task> testNotification = new Interaction<UDPMessage, Task>();
        public Interaction<UDPMessage, Task> TestNotification => this.testNotification;
        public ReceiverViewModel()
        {
            var udpservice = Locator.Current.GetService<UDPService>();
            _udpService = udpservice!;

            _devicesCache.Connect()
                .ExpireAfter(x => TimeSpan.FromSeconds(5), RxSchedulers.MainThreadScheduler)
                .Group(x=>x.InPrison)
                .Transform(x=>new GroupedItems<SimpleTrackedDeviceViewModel,string, bool>(x.Key ? "In Prison" : "Free", x, RxSchedulers.MainThreadScheduler))
                
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .SortAndBind(out var devices, SortExpressionComparer<GroupedItems<SimpleTrackedDeviceViewModel, string,bool>>.Descending(x=>x.Name))
                .Subscribe();
            GroupedDevices = devices;
            Log.Information("ReceiverViewModel initialized");
            _latestMessageHelper = _udpService.NotificationReceived.ObserveOn(RxSchedulers.MainThreadScheduler).ToProperty(this, x => x.LatestMessage);

            _udpService.LastestDevicesReceived.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(async devices =>
            {
               
                var lastDevices = _devicesCache.Items;
                //var toRemove = lastDevices.Where(ed => !devices.Any(dp => dp.Id == ed.Id)).ToList();
                //var toAdd = devices.Where(ed => !lastDevices.Any(ld => ld.Id == ed.Id)).ToList();
                //_devicesCache.Edit(updater =>
                //{
                //    updater.RemoveKeys(toRemove.Select(ed => ed.Id));
                //    updater.AddOrUpdate(toAdd);
                //});
                _devicesCache.AddOrUpdate(devices);

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
