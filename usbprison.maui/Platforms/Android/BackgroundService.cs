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
using usbprison.lib.Models;
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
            var udpService = new usbprison.lib.Services.UDPService();
            var cts = new CancellationTokenSource();
            var message = await udpService.ListenOnceAsync(cts.Token);

            if (message != null && message.MessageType == usbprison.lib.Models.UDPMessageType.Alert && !string.IsNullOrEmpty(message.Message))
            {
                Log.Information("BackgroundService received alert notification: {0}", message.Message);
                await SendNotificationAsync(message);
            }
            else
            {
                Log.Information("BackgroundService received no valid notification.");
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
            var notification = new AndroidNotification { Ticker = "Ticker Value", UseBigTextStyle = true };
            notification.Title = "USB Prison ALERT";
            notification.Message = message?.Message ?? "No message received";
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
