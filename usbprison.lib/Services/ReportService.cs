using DynamicData;
using ReactiveUI;
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
        public ReadOnlyObservableCollection<GroupedDeviceLogViewModel> GroupedLogs { get; }

        public ReportService(DatabaseService databaseService, MonitoringService monitoringService) 
        {
            _databaseService = databaseService;
            _monitoringService = monitoringService;

            _logCache.Connect()
                .GroupOn(x => x.DeviceId)
                .Transform(x => new GroupedDeviceLogViewModel(x))
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Bind(out var groupedlogs)
                .Subscribe();
            GroupedLogs = groupedlogs;

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
