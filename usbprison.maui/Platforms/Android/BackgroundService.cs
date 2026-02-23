using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Concurrent.Futures;
using AndroidX.Core.App;
using AndroidX.Work;
using AndroidX.Work.Impl.Utils.Futures;
using Google.Common.Util.Concurrent;
using Java.Interop;
using Java.Util.Concurrent;
using Kotlin.Coroutines;
using Serilog;
using Splat;

//using Shiny.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using usbprison;
using usbprison.lib.Models;
using usbprison.lib.Services;
using usbprison.maui.Services;
using static Android.Icu.Text.CaseMap;

namespace usbprison.maui.Platforms.Android
{
    public class BackgroundService : AndroidX.Work.Worker, IBackgroundService
    {
        //private readonly INotificationManager? _notificationManager;
        private readonly NotificationManagerCompat? _notificationMangerCompat;
        private bool channelInitialized;

        const string channelId = "default";
        const string channelName = "Default";
        const string channelDescription = "The default channel for notifications.";

        public const string TitleKey = "title";
        public const string MessageKey = "message";

        int messageId = 0;
        int pendingIntentId = 0;

        public BackgroundService(Context context, WorkerParameters workerParams):base(context, workerParams)
        {
            Log.Information("BackgroundService initializing");
            //_notificationManager = notificationManager;
            //_notificationManager = IPlatformApplication.Current?.Services.GetService<INotificationManager>();

            _notificationMangerCompat = NotificationManagerCompat.From(Platform.AppContext);
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
            Log.Information("Starting BackgroundWork with RunAsyncTask");
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
            if (IPlatformApplication.Current == null)
            {
                Log.Error("IPlatformApplication.Current is null, cannot proceed with background work.");
#pragma warning disable CS8603 // Possible null reference return.
                return Result.InvokeFailure();
#pragma warning restore CS8603 // Possible null reference return.
            }
            var ipService = Locator.Current.GetService<IIPService>();
            if (ipService == null)
            {
                Log.Error("IPService is null, cannot proceed with background work.");
#pragma warning disable CS8603 // Possible null reference return.
                return Result.InvokeFailure();
#pragma warning restore CS8603 // Possible null reference return.
            }
            //var ipService = new IPService();
            //var udpService = new UDPService(deviceInfo, ipService);
            var udpService = Locator.Current.GetService<UDPService>();
            if (udpService == null)
            {
                Log.Error("UDPService is null, cannot proceed with background work.");
#pragma warning disable CS8603 // Possible null reference return.
                return Result.InvokeFailure();
#pragma warning restore CS8603 // Possible null reference return.
            }
            Log.Information("OK so far.");
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
            //if (_notificationManager == null)
            //{
            //    Log.Error("NotificationManager is null, cannot send notification.");
            //    return;
            //}
            if (_notificationMangerCompat == null)
            {
                Log.Error("NotificationManager is null, cannot send notification.");
                return;
            }

            if (!channelInitialized)
            {
                CreateNotificationChannel();
            }
            var title = $"USBPrison: {message.MissingDevices!.Count} escaped!";
            var notifmessage = "Missing: \n" + message.MissingDevices!.Aggregate("", (x, y) => x + (!string.IsNullOrEmpty(x) ? "\n" : "") + (string.IsNullOrWhiteSpace(y.CustomText) ? y.Name : y.CustomText));


            Intent intent = new Intent(Platform.AppContext, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, notifmessage);
            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

            var pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.UpdateCurrent;

            PendingIntent? pendingIntent = PendingIntent.GetActivity(Platform.AppContext, pendingIntentId++, intent, pendingIntentFlags);

            var snoozeIntent = new Intent(Platform.AppContext, typeof(MainActivity));
            snoozeIntent.SetAction("SNOOZE");
            var snoozePendingIntent = PendingIntent.GetActivity(Platform.AppContext, pendingIntentId++, snoozeIntent, pendingIntentFlags);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            NotificationCompat.Builder builder = new NotificationCompat.Builder(Platform.AppContext, channelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(notifmessage))
                .SetContentText(notifmessage)
                .AddAction( Resource.Drawable.iconbetterblack, "Ignore This Period", snoozePendingIntent)
                .SetLargeIcon(BitmapFactory.DecodeResource(Platform.AppContext.Resources, Resource.Drawable.iconbetterblack))
                .SetSmallIcon(Resource.Drawable.iconbetterblack);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            Notification? notification = builder?.Build();
            _notificationMangerCompat.Notify(messageId++, notification);

            Log.Information($"Notification sent: {notifmessage}");


            //var notification = new AndroidNotification {  UseBigTextStyle = true };
            //notification.Title = $"USBPrison: {message.MissingDevices!.Count} escaped!";
            //notification.Message = "Missing: \n" + message.MissingDevices!.Aggregate("", (x, y) => x + (!string.IsNullOrEmpty(x) ? "\n" : "") + (string.IsNullOrWhiteSpace(y.CustomText) ? y.Name : y.CustomText));


            //var result = await _notificationManager.RequestRequiredAccess(notification);
            //if (result != Shiny.AccessState.Available)
            //{
            //    Log.Error("Unable to send notification, no permissions");
            //}
            //else
            //{
            //    await _notificationManager.Send(notification);
            //    Log.Information($"Notification sent: {notification.Message}");
            //}
        }

        void CreateNotificationChannel()
        {
            // Create the notification channel, but only on API 26+.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = channelDescription
                };
                // Register the channel
                NotificationManager manager = (NotificationManager)Platform.AppContext.GetSystemService(Context.NotificationService)!;
                manager.CreateNotificationChannel(channel);
                channelInitialized = true;
            }
        }

    }

    

}
