using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace usbprison
{
    public class IPService : IIPService
    {
        public Task<IPAddress?> GetBroadcastAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress? address = null;
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    address = ip; 
                    break;
                }
            }
            var broadcast = address?.GetBroadcastAddress(address.GetSubnetMask());
            return Task.FromResult(broadcast);
        }
    }
}
