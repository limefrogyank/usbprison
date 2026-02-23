using DynamicData;
using DynamicData.Binding;
using DynamicData.List;

using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace usbprison.lib.Services
{
    public class ReportService
    {
        private readonly DatabaseService _databaseService;
        private readonly MonitoringService _monitoringService;

        SourceList<PrisonLog> _logCache = new();
        public ObservableCollectionExtended<GroupedDeviceLogViewModel> GroupedLogs { get; private set; } = new ObservableCollectionExtended<GroupedDeviceLogViewModel>();

        public ObservableCollectionExtended<FlatDeviceLogViewModel> FlattenedLogs { get; private set; } = new ObservableCollectionExtended<FlatDeviceLogViewModel>();

        private BehaviorSubject<DateTime> _dateSubject = new(DateTime.Now.Date);
        public IObservable<DateTime> DateSelected => _dateSubject.AsObservable();

        public void SetDate(DateTime date)
        {
            _dateSubject.OnNext(date);
        }

        public ReportService(DatabaseService databaseService, MonitoringService monitoringService, bool flattenGroups = false)
        {
            _databaseService = databaseService;
            _monitoringService = monitoringService;

            var logCachePublication = _logCache.Connect()
                .FilterOnObservable(x=> DateSelected.Select(date => x.Timestamp.ToLocalTime().Date == date  || x.Timestamp == DateTime.MinValue))
                .GroupOn(x => x.DeviceId)
                .Publish();

            if (!flattenGroups)
            {
                logCachePublication
                .Transform(x => new GroupedDeviceLogViewModel(x))
                .DisposeMany()
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Bind(GroupedLogs)
                .Subscribe();
            }
            if (flattenGroups)
            {
                logCachePublication

                    .OnItemAdded(x =>
                    {
                        _logCache.Add(new PrisonLog { DeviceId = x.GroupKey, MachineId = "__GROUP__" });
                    })
                    .OnItemRemoved(x =>
                    {
                        // this never seems to run...
                        var exiting = _logCache.Items.FirstOrDefault(existing => existing.MachineId == "__GROUP__" && x.GroupKey == existing.DeviceId);
                        if (exiting != null)
                        {
                            Log.Information("Group removed : " + x.GroupKey);
                            _logCache.Remove(exiting);
                        }
                    })
                    .FilterOnObservable(x => x.List.CountChanged.Select(x => x > 1))  // this removes the group header with the group doesn't have any logs
                    .TransformMany(x =>
                    {
                        return x.List;
                    })
                    .Transform(x => new FlatDeviceLogViewModel(x))
                    .Sort(SortExpressionComparer<FlatDeviceLogViewModel>.Ascending(x => x.Log.DeviceId).ThenByAscending(x => x.Log.Timestamp))
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Bind(FlattenedLogs)
                    .Subscribe();
            }


            _ = InitializeAsync();
            
            logCachePublication.Connect();

        }



        public async Task InitializeAsync()
        {
            //var dic = await _databaseService.GetLogsByTrackedDeviceAsync(DateTime.Now - TimeSpan.FromDays(1));
            _monitoringService.TrackedDevicesCache.Connect()
                .Transform(async trackedDevice =>
                {
                    var logsObs = await _databaseService.GetLogsForTrackedDeviceAsync(trackedDevice.Id);//, DateTime.Now - TimeSpan.FromDays(1));
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
