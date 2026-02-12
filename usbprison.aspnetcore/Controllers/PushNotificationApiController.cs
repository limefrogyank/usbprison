
using Microsoft.AspNetCore.Mvc;
using usbprison.aspnetcore.Model;

namespace usbprison.aspnetcore.Controllers
{
    [Route("push-notifications-api")]
    [ApiController]
    public class PushNotificationsApiController : ControllerBase
    {
        private readonly PushServiceStoreService _subscriptionStore;
        private readonly PushNotificationService _notificationService;
        private readonly PushNotificationsQueue _pushNotificationsQueue;

        public PushNotificationsApiController(PushServiceStoreService subscriptionStore, PushNotificationService notificationService, PushNotificationsQueue pushNotificationsQueue)
        {
            _subscriptionStore = subscriptionStore;
            _notificationService = notificationService;
            _pushNotificationsQueue = pushNotificationsQueue;
        }

        // GET push-notifications-api/public-key
        [HttpGet("public-key")]
        public ContentResult GetPublicKey()
        {
            return Content(_notificationService.PublicKey, "text/plain");
        }

        // POST push-notifications-api/subscriptions
        [HttpPost("subscriptions")]
        public async Task<IActionResult> StoreSubscription([FromBody] PushSubscription subscription)
        {
            await _subscriptionStore.StoreSubscriptionAsync(subscription);

            return NoContent();
        }

        // DELETE push-notifications-api/subscriptions?endpoint={endpoint}
        [HttpDelete("subscriptions")]
        public async Task<IActionResult> DiscardSubscription(string endpoint)
        {
            await _subscriptionStore.DiscardSubscriptionAsync(endpoint);

            return NoContent();
        }

        // POST push-notifications-api/notifications
        [HttpPost("notifications")]
        public IActionResult SendNotification([FromBody] PushMessageViewModel message)
        {
            _pushNotificationsQueue.Enqueue(new Lib.Net.Http.WebPush.PushMessage(message.Notification)
            {
                Topic = message.Topic,
                Urgency = message.Urgency
            });

            return NoContent();
        }
    }
}
