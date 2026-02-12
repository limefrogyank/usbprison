using Android.Content;
using AndroidX.Concurrent.Futures;
using AndroidX.Work;
using AndroidX.Work.Impl.Utils.Futures;
using Google.Common.Util.Concurrent;
using Java.Interop;
using Java.Util.Concurrent;
using Kotlin.Coroutines;
using Serilog;
using Shiny.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using usbprison;
using usbprison.lib.Models;
using usbprison.lib.Services;
using usbprison.maui.Services;

namespace usbprison.maui.Platforms.Android
{
    public class BackgroundService : AndroidX.Work.Worker, IBackgroundService
    {
        private readonly INotificationManager? _notificationManager;

        public BackgroundService(Context context, WorkerParameters workerParams):base(context, workerParams)
        {
            Log.Information("BackgroundService initializing");
            //_notificationManager = notificationManager;
            _notificationManager = IPlatformApplication.Current?.Services.GetService<INotificationManager>();
        }


        public override Result DoWork()
        {
            try
            {
                // Run async code synchronously here
                var task = RunAsyncTask();
                task.Wait(); // Wait for async to complete
                return task.Result;
            }
            catch (Exception ex)
            {
                Log.Error("MyAsyncWorker: {0}", ex.ToString());
#pragma warning disable CS8603 // Possible null reference return.
                return Result.InvokeFailure();
#pragma warning restore CS8603 // Possible null reference return.
            }
            

        }

        private  async Task<Result> RunAsyncTask()
        {
            var deviceInfo = new GenericDeviceInfo
            {
                Name = DeviceInfo.Name,
                Manufacturer = DeviceInfo.Manufacturer,
                Model = DeviceInfo.Model,
                Version = DeviceInfo.VersionString,
                Platform = DeviceInfo.Platform.ToString(),
                Idiom = DeviceInfo.Idiom.ToString(),
                DeviceType = DeviceInfo.DeviceType.ToString()

            };
            var ipService = new IPService();
            var udpService = new UDPService(deviceInfo, ipService);
            var cts = new CancellationTokenSource();
            var message = await udpService.ListenOnceAsync(cts.Token);

            if (message != null && message.MessageType == usbprison.lib.Models.UDPMessageType.Alert && !string.IsNullOrEmpty(message.Message))
            {
                Log.Information("BackgroundService received alert notification: {0}", message.Message);
                await SendNotificationAsync(message);
            }
            else if (message != null && message.MessageType == UDPMessageType.OK)
            {
                Log.Information("BackgroundService received an OK.");
            }
            else
            {
                Log.Information("BackgroundService didn't receive a message.");
            }

#pragma warning disable CS8603 // Possible null reference return.
            return Result.InvokeSuccess();
#pragma warning restore CS8603 // Possible null reference return.
        }

        private async Task SendNotificationAsync(UDPMessage message)
        {
            if (_notificationManager == null)
            {
                Log.Error("NotificationManager is null, cannot send notification.");
                return;
            }
            var notification = new AndroidNotification {  UseBigTextStyle = true };
            //notification.LargeIconResourceName
            notification.Title = $"USBPrison: {message.MissingDevices!.Count} escaped!";
            notification.Message = "Missing: \n" + message.MissingDevices!.Aggregate("", (x, y) => x + (!string.IsNullOrEmpty(x) ? "\n" : "") + (y.CustomText ?? y.Name));
            var result = await _notificationManager.RequestRequiredAccess(notification);
            if (result != Shiny.AccessState.Available)
            {
                Log.Error("Unable to send notification, no permissions");
            }
            else
            {
                await _notificationManager.Send(notification);
                Log.Information($"Notification sent: {notification.Message}");
            }
        }


    }

    

}
