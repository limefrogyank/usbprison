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
            if (CrossWifiManager.Current == null)
            {
                Log.Error("CrossWifiManager.Current is null.  Cannot determine broadcast address.");
                await Task.Delay(2000);
            }
            var response = await CrossWifiManager.Current!.GetNetworkInfo();
            if (response.Data == null || response.Data.IpAddress == 0)
            {
                return IPAddress.Broadcast;
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
