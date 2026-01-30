using MauiWifiManager;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace usbprison
{
    public class IPService : IIPService
    {
        public IPService() { }

        public async Task<IPAddress?> GetBroadcastAddress()
        {
            var response = await CrossWifiManager.Current.GetNetworkInfo();
            if (response.Data == null || response.Data.IpAddress == 0)
            {
                return null;
                // try using 255.255.255.255
            }

            var ipAddress = new IPAddress(BitConverter.GetBytes(response.Data.IpAddress));
            var subnetmask = ipAddress.GetSubnetMask();
            if (subnetmask == null)
            {
                Log.Warning("Could not determine subnet mask.  Using default broadcast address.");
                return IPAddress.Broadcast;
            }
            var broadcastAddress = ipAddress.GetBroadcastAddress(subnetmask);
            return broadcastAddress;
        }


    }
}
