using DynamicData;
using System;
using System.Collections.Generic;
using System.Text;

namespace usbprison.lib.Services
{
    public class BroadcastService
    {
        private readonly ISettingsService _settingsService;
        private readonly UDPService _udpService;

        public BroadcastService(ISettingsService settingsService, UDPService udpService) 
        {
            _settingsService = settingsService;
            _udpService = udpService;

            _settingsService.TrackedDevices.Connect()
                .Transform(dev => new TrackedDeviceViewModel(dev))
                .AutoRefresh(x => x.IsPluggedIn)
                .Bind(out var trackedDevices)
                .Subscribe();

            _settingsService.DailySchedule.Connect()
                .Transform(x => new DailyScheduleViewModel(x))
                .AutoRefresh(x => x.StartTime)
                .AutoRefresh(x => x.EndTime)
                .Bind(out var dailySchedule)
                .Subscribe();

            var timer = new System.Timers.Timer(5000); //every 5 seconds
            timer.Elapsed += async (s, e) =>
            {
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
                
                
                await _udpService.BroadcastMessageAsync(new lib.Models.UDPMessage
                {
                    MessageType = lib.Models.UDPMessageType.Alert,
                    Message = $"There are {trackedDevices.Where(x => x.IsPluggedIn).Count()} device(s) in prison",
                    PluggedDevices = trackedDevices.Where(x => x.IsPluggedIn).Select(x => x.Device).ToList(),
                    MissingDevices = trackedDevices.Where(x => !x.IsPluggedIn).Select(x => x.Device).ToList()
                });


            };
            timer.Enabled = true;
        }
    }
}
