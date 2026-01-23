using ReactiveUI.Maui;
using Serilog;
using Shiny.Notifications;
using Splat;
using usbprison.lib.Models;


namespace usbprison.maui.Pages
{
    public partial class LogView : ReactiveContentPage<LogViewModel>
    {
        public LogView() : base()
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<LogViewModel>();
            if (ViewModel != null)
            {
                _ = ViewModel.InitializeAsync(FileSystem.Current.AppDataDirectory);
            }
        }

        public async Task TestNotificationAsync(UDPMessage message)
        {
#if ANDROID
            var notificationHandler = Handler?.GetService<INotificationManager>();
            var notification = new AndroidNotification { Ticker = "Ticker Value", UseBigTextStyle = true };
            notification.Title = "USB Prison ALERT";
            notification.Message = message?.Message ?? "No message received";
            if (notificationHandler == null)
            {
                Log.Error("Notification handler is null");
                return;
            }
            var result = await notificationHandler.RequestRequiredAccess(notification);
            if (result != Shiny.AccessState.Available)
            {
                Log.Error("Unable to send notification, no permissions");
            }
            else
            {
                await notificationHandler.Send(notification);
                Log.Information($"Notification sent: {notification.Message}");
            }
#endif
        }
    }
}