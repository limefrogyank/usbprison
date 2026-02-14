using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;

namespace usbprison.lib.Services
{
    public class ReportService
    {
        private readonly DatabaseService _databaseService;
        private readonly MonitoringService _monitoringService;

        SourceList<PrisonLog> _logCache = new SourceList<PrisonLog>();
        public ReadOnlyObservableCollection<GroupedDeviceLogViewModel> GroupedLogs1 { get; }
        public ObservableCollectionExtended<GroupedDeviceLogViewModel> GroupedLogs { get; private set; } = new ObservableCollectionExtended<GroupedDeviceLogViewModel>();

        public ObservableCollectionExtended<PrisonLog> FlattenedLogs { get; private set; } = new ObservableCollectionExtended<PrisonLog>();

        public ReportService(DatabaseService databaseService, MonitoringService monitoringService)
        {
            _databaseService = databaseService;
            _monitoringService = monitoringService;

            _logCache.Connect()
                .GroupOn(x => x.DeviceId)
                .Transform(x => new GroupedDeviceLogViewModel(x))
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Bind(GroupedLogs)
                .Subscribe();

            _logCache.Connect()
                .GroupOn(x=> x.DeviceId)
                // .OnItemAdded(x =>
                // {
                //     Log.Information($"Log Added for Device {x.GroupKey}: {x.List.Count} items");
                //     x.List.
                // })
                .TransformMany(x =>
                {
                    var bindingList = new System.ComponentModel.BindingList<PrisonLog>();
                    x.List.Connect()
                    
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Bind(bindingList)
                    .Subscribe();
                    bindingList.Insert(0, new PrisonLog { DeviceId = x.GroupKey, MachineId="__GROUP__" });
                    return bindingList;
                })
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Bind(FlattenedLogs)
                .Subscribe();
            //GroupedLogs = groupedlogs;

            _ = InitializeAsync();

        }



        public async Task InitializeAsync()
        {
            var dic = await _databaseService.GetLogsByTrackedDeviceAsync(DateTime.Now - TimeSpan.FromDays(1));
            _monitoringService.TrackedDevicesCache.Connect()
                .Transform(async trackedDevice =>
                {
                    var logsObs = await _databaseService.GetLogsForTrackedDeviceAsync(trackedDevice.Id, DateTime.Now - TimeSpan.FromDays(1));
                    return logsObs.Merge(_monitoringService.PrisonLog.Where(x => x.DeviceId == trackedDevice.Id))
                        .Scan(new PrisonLog(), (x, y) =>
                        {
                            if (x.Timestamp == DateTime.MinValue)
                                return y;

                            if (y.Status == x.Status)
                            {
                                y.Timestamp = DateTime.MaxValue;
                            }
                            return y;
                        })
                    .Where(x => x.Timestamp != DateTime.MaxValue)
                    .Buffer(TimeSpan.FromSeconds(1))
                    .Subscribe(x =>
                    {
                        _logCache.AddRange(x);
                    });
                })
                .DisposeMany()
                .Subscribe();

            //foreach (var device in dic)
            //{


            //    //// BEFORE LIMITING DATABASE ADDS
            //    device.Value.Merge(_monitoringService.PrisonLog.Where(x => x.DeviceId == device.Key))
            //        .Scan(new PrisonLog(), (x, y) =>
            //        {
            //            if (x.Timestamp == DateTime.MinValue)
            //                return y;

            //            if (y.Status == x.Status)
            //            {
            //                y.Timestamp = DateTime.MaxValue;
            //            }
            //            return y;
            //        })
            //        .Where(x => x.Timestamp != DateTime.MaxValue)
            //        .Buffer(TimeSpan.FromSeconds(1))
            //        .Subscribe(x =>
            //        {
            //            _logCache.AddRange(x);
            //        });


            //}
        }
    }
}
