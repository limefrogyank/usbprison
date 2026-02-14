using DynamicData;
using Serilog;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using usbprison.lib.Models;

namespace usbprison.lib.Services
{
    public class MonitoringService
    {
        private readonly GenericDeviceInfo _machineInfo;
        private readonly ISettingsService _settingsService;
        private readonly UDPService _udpService;
        private readonly DatabaseService _databaseService;

        private Subject<PrisonLog> _prisonLogSubject = new Subject<PrisonLog>();
        public IObservable<PrisonLog> PrisonLog => _prisonLogSubject.AsObservable();

        public SourceCache<TrackedDeviceModel, string> TrackedDevicesCache { get; } = new SourceCache<TrackedDeviceModel, string>(dev => dev.Id ?? System.Guid.NewGuid().ToString());

        public MonitoringService(GenericDeviceInfo info, ISettingsService settingsService, UDPService udpService, DatabaseService databaseService) 
        {
            _machineInfo = info;
            _settingsService = settingsService;
            _udpService = udpService;
            _databaseService = databaseService;

            TrackedDevicesCache.Connect()
               .Transform(dev => new TrackedDeviceViewModel(dev))
               .AutoRefresh(x => x.IsPluggedIn)
               .Bind(out var trackedDevices)
               .Subscribe();

            _settingsService.DailySchedule.Connect()
                .Transform(x => new DailyScheduleViewModel(x))
                .AutoRefresh(x => x.StartTime)
                .AutoRefresh(x => x.EndTime)
                .Bind(out var dailySchedules)
                .Subscribe();

            _ = Task.Run(async () =>
            {
                var trackedDevices = await _databaseService.DB.Table<TrackedDeviceModel>().ToListAsync();
                using (TrackedDevicesCache.SuspendNotifications())
                {
                    TrackedDevicesCache.Clear();
                    TrackedDevicesCache.AddOrUpdate(trackedDevices);
                }
                
            //now create a mechanism that updates the database for new adds and removes and updates
            //TrackedDevicesCache.CountChanged.Take(1).Subscribe(x =>
            TrackedDevicesCache.Connect()
                
                .OnItemAdded(async x =>
                {
                    var t = await _databaseService.DB.InsertAsync(x);
                })
                .OnItemRemoved(async x =>
                {
                    var t = await _databaseService.DB.DeleteAsync(x);
                })
                .OnItemUpdated(async (curr, prev) =>
                {
                    var t = await _databaseService.DB.UpdateAsync(curr);
                })
                .Subscribe();
                    //);
            });

            var timer = new System.Timers.Timer(5000); //every 5 seconds
            timer.Elapsed += async (s, e) =>
            {
                // logging to database
                var currentDay = DateTime.Now.DayOfWeek;
                var now = DateTime.Now;
                var dateNow = now.Date;
                var timeNow = now.TimeOfDay;
                var currentDaySchedule = dailySchedules.First(x=>x.DayOfWeek == currentDay);
                DateTime lockdownStart = default;
                DateTime lockdownEnd = default;
                if (timeNow > currentDaySchedule.EndTime && timeNow < currentDaySchedule.StartTime)
                {
                    // NOT LOCKDOWN
                    lockdownStart = dateNow + currentDaySchedule.StartTime;
                    lockdownEnd = dateNow + currentDaySchedule.EndTime;
                }
                else if (timeNow < currentDaySchedule.EndTime)
                {
                    // early in day
                    // Lockdown, but need previous day's start time
                    lockdownEnd = dateNow + currentDaySchedule.EndTime;
                    var previousDay = (int)currentDay - 1;
                    if (previousDay < 0) previousDay = 6;
                    lockdownStart = dateNow + dailySchedules.First(x => x.DayOfWeek == (DayOfWeek)previousDay).StartTime - TimeSpan.FromDays(1);
                }
                else if (timeNow > currentDaySchedule.StartTime)
                {
                    // late in day
                    // Lockdown, but need next day's end time
                    lockdownStart = dateNow + currentDaySchedule.StartTime;
                    var nextDay = (int)currentDay + 1;
                    if (nextDay > 6) nextDay = 0;
                    lockdownEnd = dateNow + dailySchedules.First(x => x.DayOfWeek == (DayOfWeek)nextDay).EndTime + TimeSpan.FromDays(1);
                }

                var prisonLogs = trackedDevices.Select(x => new PrisonLog
                {
                    DeviceId = x.Device.Id,
                    MachineId = _machineInfo.SenderId,
                    Timestamp = DateTime.UtcNow,
                    LockdownStart = lockdownStart,
                    LockdownEnd = lockdownEnd,
                    Status = (now < lockdownStart ? (x.IsPluggedIn ? PrisonStatus.Home : PrisonStatus.Free) : (x.IsPluggedIn ? PrisonStatus.Locked : PrisonStatus.Escaped))
                }).ToList();
                prisonLogs.ForEach(x => _prisonLogSubject.OnNext(x));
                await _databaseService.AddLogsAsync(prisonLogs);

                // regular app use
                await _udpService.BroadcastMessageAsync(new lib.Models.UDPMessage
                {
                    MessageType = lib.Models.UDPMessageType.Notify,
                    Message = $"There are {trackedDevices.Where(x => x.IsPluggedIn).Count()} device(s) in prison"
                });
                await _udpService.BroadcastMessageAsync(new lib.Models.UDPMessage
                {
                    MessageType = lib.Models.UDPMessageType.List,
                    PluggedDevices = trackedDevices.Where(x => x.IsPluggedIn).Select(x => x.Device).ToList(),
                    MissingDevices = trackedDevices.Where(x => !x.IsPluggedIn).Select(x => x.Device).ToList()
                });

                // background monitoring use
                // check schedule before sending out message

                var date = DateTime.Now;
                var time = date.TimeOfDay;
                var dayOfWeek = date.DayOfWeek;
                var schedule = dailySchedules.FirstOrDefault(x => x.DayOfWeek == dayOfWeek);
                if (schedule == null)
                {
                    Log.Error($"Couldn't find a DailySchedule that matched DayOfWeek = {dayOfWeek}.");
                    return; 
                }
                if (time < schedule.EndTime || time > schedule.StartTime)
                {
                    // are there any tracked devices that are free??
                    var escapedDevices = trackedDevices.Where(x => !x.IsPluggedIn);
                    if (escapedDevices.Any())
                    {
                        await _udpService.BroadcastMessageAsync(new lib.Models.UDPMessage
                        {
                            MessageType = lib.Models.UDPMessageType.Alert,
                            Message = $"{escapedDevices.Count()} device(s) have escaped lockdown!",
                            PluggedDevices = trackedDevices.Where(x => x.IsPluggedIn).Select(x => x.Device).ToList(),
                            MissingDevices = escapedDevices.Select(x => x.Device).ToList()
                        });
                    } 
                    else
                    {
                        await _udpService.BroadcastMessageAsync(new UDPMessage
                        {
                            MessageType = UDPMessageType.OK
                        });
                    }
                }
                else
                {
                    // TESTING 
                    var escapedDevices = trackedDevices.Where(x => !x.IsPluggedIn);
                    await _udpService.BroadcastMessageAsync(new lib.Models.UDPMessage
                    {
                        MessageType = lib.Models.UDPMessageType.Alert,
                        Message = $"{escapedDevices.Count()} device(s) have escaped lockdown!",
                        PluggedDevices = trackedDevices.Where(x => x.IsPluggedIn).Select(x => x.Device).ToList(),
                        MissingDevices = escapedDevices.Select(x => x.Device).ToList()
                    });
                    // not during lockdown, send OK
                    //await _udpService.BroadcastMessageAsync(new UDPMessage
                    //{
                    //    MessageType = UDPMessageType.OK
                    //});
                }

            };
            timer.Enabled = true;
        }


        //public async Task RemoveDeviceAsync(TrackedDeviceModel device)
        //{
        //    TrackedDevicesCache.Remove(device.Id);
        //    var result = await _databaseService.DB.Table<TrackedDeviceModel>().DeleteAsync(x=>x.Id == device.Id);
        //    if (result == 0)
        //    {
        //        Log.Error($"Couldn't delete TrackedDeviceModel with Id of {device.Id} from database.");
        //    }
        //}
    }
}
