using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Work;
using Java.Util.Concurrent;
using usbprison.maui.Platforms.Android;

namespace usbprison.maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter(
      new[] {
Shiny.ShinyNotificationIntents.NotificationClickAction
      },
      Categories = new[] {
          "android.intent.category.DEFAULT"
      }
  )]
    public class MainActivity : MauiAppCompatActivity
    {


        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
#pragma warning disable CS8604 // Possible null reference argument.
            Constraints constraints = new Constraints.Builder()
        .SetRequiredNetworkType(NetworkType.Unmetered)
        .Build();
#pragma warning restore CS8604 // Possible null reference argument.

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var request = PeriodicWorkRequest.Builder
                .From<BackgroundService>(TimeSpan.FromMinutes(15))
                .SetConstraints(constraints)
                .SetInitialDelay(1, TimeUnit.Minutes)
                .Build();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            var workManager = WorkManager.GetInstance(this);
            var op = workManager.EnqueueUniquePeriodicWork(
                "BackgroundServiceWork",
                ExistingPeriodicWorkPolicy.CancelAndReenqueue!,
                (PeriodicWorkRequest)request
            );
        }
    }
}
