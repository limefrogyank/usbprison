using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using usbprison.lib.Models;

namespace usbprison.lib.Services
{
    public class UDPService
    {
        public static int MainPort = 49111;
        public static int BackgroundPort = 49112;
        //private readonly UdpClient _udpClient;
        //private readonly IPEndPoint _mainEndpoint;
        //private readonly IPEndPoint _backgroundEndpoint;
        private CancellationTokenSource? _cancelTokenForListen;

        private Subject<string> _rawMessageSubject = new Subject<string>();
        public IObservable<string> RawMessageReceived => _rawMessageSubject.AsObservable();

        private Subject<string> _notificationSubject = new Subject<string>();
        public IObservable<string> NotificationReceived => _notificationSubject.AsObservable();

        private Subject<List<MultiTrackedDeviceViewModel>> _lastestDevicesSubject = new Subject<List<MultiTrackedDeviceViewModel>>();
        public IObservable<List<MultiTrackedDeviceViewModel>> LastestDevicesReceived => _lastestDevicesSubject.AsObservable();

        private GenericDeviceInfo _deviceInfo;
        private readonly IIPService _ipService;

        private IPAddress? _broadcastAddress = null;

        public UDPService(GenericDeviceInfo info, IIPService iPService)
        {
            _deviceInfo = info;
            _ipService = iPService;
            //_udpClient = new System.Net.Sockets.UdpClient(UDPService.PORT);
            //_mainEndpoint = new IPEndPoint(IPAddress.Broadcast, UDPService.MainPort);
            //_backgroundEndpoint = new IPEndPoint(IPAddress.Broadcast, UDPService.BackgroundPort);
        }

        public async Task StartListening()
        {
            if (_broadcastAddress == null)
            {
                _broadcastAddress = await _ipService.GetBroadcastAddress();
                if (_broadcastAddress == null)
                {
                    Log.Warning("Could not determine broadcast address, using default");
                    _broadcastAddress = IPAddress.Broadcast;
                }
            }
            _cancelTokenForListen = new CancellationTokenSource();
            await Task.Run(() => ReceiveLoopAsync(_cancelTokenForListen.Token));
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            using var udpClient = new UdpClient(new IPEndPoint(_broadcastAddress!, MainPort)) { EnableBroadcast = true };
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var result = await udpClient.ReceiveAsync(token);
                    var message = Encoding.UTF8.GetString(result.Buffer, 0, result.Buffer.Length);
                    _rawMessageSubject.OnNext(message);
                    var udpmessage = JsonSerializer.Deserialize<UDPMessage>(message);
                    if (udpmessage != null)
                    {
                        Log.Information($"Received message: {udpmessage.MessageType} {(udpmessage.PluggedDevices != null ? udpmessage.PluggedDevices.Count : "no devices")}");
                        switch (udpmessage.MessageType)
                        {
                            case UDPMessageType.Notify:
                                _notificationSubject.OnNext(udpmessage.Message ?? "");
                                break;
                            case UDPMessageType.List:
                                var plugged = udpmessage.PluggedDevices?.Select(x => new MultiTrackedDeviceViewModel(x, true, udpmessage.SenderId)).ToList() ?? new List<MultiTrackedDeviceViewModel>();
                                var unplugged = udpmessage.MissingDevices?.Select(x => new MultiTrackedDeviceViewModel(x, false, udpmessage.SenderId)).ToList() ?? new List<MultiTrackedDeviceViewModel>();
                                _lastestDevicesSubject.OnNext(plugged.Concat(unplugged).ToList());
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (ObjectDisposedException) when (token.IsCancellationRequested)
            {
                // Socket has been closed, exit the loop
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Receive error: {ex.Message}");
            }
        }

        public async Task<UDPMessage?> ListenOnceAsync(CancellationToken token)
        {
            if (_broadcastAddress == null)
            {
                _broadcastAddress = await _ipService.GetBroadcastAddress();
                if (_broadcastAddress == null)
                {
                    Log.Warning("Could not determine broadcast address, using default");
                    _broadcastAddress = IPAddress.Broadcast;
                }
            }
            using var udpClient = new UdpClient(new IPEndPoint(_broadcastAddress, BackgroundPort)) { EnableBroadcast = true };
            try
            {

                var result = await udpClient.ReceiveAsync(token);
                var message = Encoding.UTF8.GetString(result.Buffer, 0, result.Buffer.Length);
                _rawMessageSubject.OnNext(message);
                var udpmessage = JsonSerializer.Deserialize<UDPMessage>(message);
                if (udpmessage != null)
                {
                    //switch (udpmessage.MessageType)
                    //{
                    //    case UDPMessageType.Notify:
                    //        _notificationSubject.OnNext(udpmessage.Message);
                    //        break;
                    //    case UDPMessageType.List:
                    //        _lastestDevicesSubject.OnNext(udpmessage.Devices);
                    //        break;
                    //    default:
                    //        break;
                    //}

                    return udpmessage;
                }

            }
            catch (ObjectDisposedException) when (token.IsCancellationRequested)
            {
                // Socket has been closed
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Receive error: {ex.Message}");
            }
            return null;
        }

        public void StopListening()
        {
            if (_cancelTokenForListen != null)
                _cancelTokenForListen.Cancel();
        }

        public async Task BroadcastMessageAsync(UDPMessage message, bool standard = true)
        {
            if (_broadcastAddress == null)
            {
                _broadcastAddress = await _ipService.GetBroadcastAddress();
                if (_broadcastAddress == null)
                {
                    Log.Warning("Could not determine broadcast address, using default");
                    _broadcastAddress = IPAddress.Broadcast;
                }
            }
            try
            {
                //add on deviceinfo before sending message
                message.SenderId = string.Join(',', _deviceInfo.Name, _deviceInfo.Version);
                byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                using var udpClient = new UdpClient() { EnableBroadcast = true, ExclusiveAddressUse=false, MulticastLoopback=false };
                switch (message.MessageType)
                {
                    case UDPMessageType.List:
                    case UDPMessageType.Notify:
                        //using (var udpClient = new UdpClient(new IPEndPoint(_broadcastAddress, MainPort)) { EnableBroadcast = true })
                        //{
                            var sent = await udpClient.SendAsync(data, data.Length, new IPEndPoint(_broadcastAddress, MainPort));// _mainEndpoint);
                        //}2
                        break;
                    case UDPMessageType.OK:
                    case UDPMessageType.Alert:
                        //using (var udpClient = new UdpClient(new IPEndPoint(_broadcastAddress, BackgroundPort)) { EnableBroadcast = true })
                        //{
                            var sent2 = await udpClient.SendAsync(data, data.Length, new IPEndPoint(_broadcastAddress, BackgroundPort));// _mainEndpoint);
                        //}
                        break;
                }

                
            }
            catch (Exception ex)
            {
                Log.Error($"Broadcast error: {ex.Message}");
                Log.Error($"Tried seding: {message}");
            }
        }


    }
}
