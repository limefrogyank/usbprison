using Serilog;
using System.Net;

namespace usbprison.blazor
{
    public class IPService : IIPService
    {
        public Task<IPAddress?> GetBroadcastAddress()
        {
            // seems like it's really hard to get a local IP from the browser.
            return Task.FromResult(IPAddress.Broadcast)!;
        }
    }
}
