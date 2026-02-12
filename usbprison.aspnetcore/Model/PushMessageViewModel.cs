using Lib.Net.Http.WebPush;

namespace usbprison.aspnetcore.Model
{
    public class PushMessageViewModel
    {
        public string Topic { get; set; }

        public string Notification { get; set; }

        public PushMessageUrgency Urgency { get; set; } = PushMessageUrgency.Normal;
    }
}
