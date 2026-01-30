using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Serilog;

namespace usbprison
{
    public class IPService : IIPService
    {
        public Task<IPAddress?> GetBroadcastAddress()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530); // Connect to a public IP
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            IPAddress? address = null;
            var success = IPAddress.TryParse(localIP, out address);
            // var host = Dns.GetHostEntry(Dns.GetHostName());
            // IPAddress? address = null;
            // foreach (var ip in host.AddressList)
            // {
            //     if (ip.AddressFamily == AddressFamily.InterNetwork)
            //     {
            //         address = ip;
            //         break;
            //     }
            // }

            IPAddress? broadcast = null;
            // if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            // {
            //     Log.Information($"Local IP Address: {address}");
            //     broadcast = IPAddress.Broadcast;
            //     Log.Information($"Broadcast IP Address: {broadcast}");
            // }
            // else
            {
                Log.Information($"Local IP Address: {address}");
                broadcast = address?.GetBroadcastAddress(address.GetSubnetMask());
                Log.Information($"Broadcast IP Address: {broadcast}");
            }

            return Task.FromResult(broadcast);
        }
    }
}
