using System.Net;

namespace usbprison
{
    public interface IIPService
    {
        Task<IPAddress?> GetBroadcastAddress();
    }
}