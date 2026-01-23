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

namespace usbprison
{
    public partial class ReceiverViewModel : ReactiveObject
    {
        private readonly UDPService _udpService;

        [ObservableAsProperty] private string _latestMessage = string.Empty;

        SourceCache<TrackedDeviceModel, string> _devicesCache = new SourceCache<TrackedDeviceModel,string>(x=>x.Id);

        public ReadOnlyObservableCollection<SimpleTrackedDeviceViewModel> Devices { get; }

        private readonly Interaction<UDPMessage, Task> testNotification = new Interaction<UDPMessage, Task>();
        public Interaction<UDPMessage, Task> TestNotification => this.testNotification;
        public ReceiverViewModel()
        {
            var udpservice = Locator.Current.GetService<UDPService>();
            _udpService = udpservice!;

            _devicesCache.Connect()
                .Transform(x=> new SimpleTrackedDeviceViewModel(x))
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Bind(out var devices)
                .Subscribe();
            Devices = devices;
            Log.Information("ReceiverViewModel initialized");
            _latestMessageHelper = _udpService.NotificationReceived.ObserveOn(RxSchedulers.MainThreadScheduler).ToProperty(this, x => x.LatestMessage);

            _udpService.LastestDevicesReceived.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(async devicesPresent =>
            {
                var lastDevices = _devicesCache.Items;
                var toRemove = lastDevices.Where(ed => !devicesPresent.Any(dp => dp.Id == ed.Id)).ToList();
                var toAdd = devicesPresent.Where(ed => !lastDevices.Any(ld => ld.Id == ed.Id)).ToList();
                _devicesCache.Edit(updater =>
                {
                    updater.RemoveKeys(toRemove.Select(ed => ed.Id));
                    updater.AddOrUpdate(toAdd);
                });

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
