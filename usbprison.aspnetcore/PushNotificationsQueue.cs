using Lib.Net.Http.WebPush;
using System.Collections.Concurrent;

namespace usbprison.aspnetcore
{
    public class PushNotificationsQueue
    {
        private readonly ConcurrentQueue<PushMessage> _messages = new ConcurrentQueue<PushMessage>();
        private readonly SemaphoreSlim _messageEnqueuedSignal = new SemaphoreSlim(0);

        public void Enqueue(PushMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _messages.Enqueue(message);

            _messageEnqueuedSignal.Release();
        }

        public async Task<PushMessage> DequeueAsync(CancellationToken cancellationToken)
        {
            await _messageEnqueuedSignal.WaitAsync(cancellationToken);

            _messages.TryDequeue(out PushMessage message);

            return message;
        }
    }
}
