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
        protected CancellationTokenSource? _cancelTokenForListen;

        protected Subject<string> _rawMessageSubject = new();
        public IObservable<string> RawMessageReceived => _rawMessageSubject.AsObservable();

        protected Subject<string> _notificationSubject = new();
        public IObservable<string> NotificationReceived => _notificationSubject.AsObservable();

        protected Subject<List<MultiTrackedDeviceViewModel>> _lastestDevicesSubject = new();
        public IObservable<List<MultiTrackedDeviceViewModel>> LastestDevicesReceived => _lastestDevicesSubject.AsObservable();

        protected GenericDeviceInfo _deviceInfo;
        protected readonly IIPService _ipService;

        protected IPAddress? _broadcastAddress = null;

        public UDPService(GenericDeviceInfo info, IIPService iPService)
        {
            _deviceInfo = info;
            _ipService = iPService;
            //_udpClient = new System.Net.Sockets.UdpClient(UDPService.PORT);
            //_mainEndpoint = new IPEndPoint(IPAddress.Broadcast, UDPService.MainPort);
            //_backgroundEndpoint = new IPEndPoint(IPAddress.Broadcast, UDPService.BackgroundPort);
        }

        public virtual void InitializeJS(object jsRuntime)
        {

        }

        public async virtual Task StartListening()
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

        protected async virtual Task ReceiveLoopAsync(CancellationToken token)
        {
            Log.Information("Starting UDP receive loop...");
            Log.Information($"Starting UDP receive loop on address {_broadcastAddress} port {MainPort}");
            using var udpClient = new UdpClient(new IPEndPoint(_broadcastAddress!, MainPort)) { EnableBroadcast = true };
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var result = await udpClient.ReceiveAsync(token);
                    Log.Information($"Received UDP message from {result.RemoteEndPoint}");
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
                                var plugged = udpmessage.PluggedDevices?.Select(x => new MultiTrackedDeviceViewModel(x, true, udpmessage.IsLockdown, udpmessage.SenderId)).ToList() ?? new List<MultiTrackedDeviceViewModel>();
                                var unplugged = udpmessage.MissingDevices?.Select(x => new MultiTrackedDeviceViewModel(x, false, udpmessage.IsLockdown, udpmessage.SenderId)).ToList() ?? new List<MultiTrackedDeviceViewModel>();
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
                // Socket has been closed
                Log.Information("UDP receive loop stopped.");
            }
            catch (Exception ex)
            {
                Log.Error($"Receive error.  {ex.Message}");
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
                Log.Information("UDP receive loop stopped.");
            }
            catch (Exception ex)
            {
                Log.Error($"Receive error.  {ex.Message}");
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
                message.SenderId = _deviceInfo.SenderId;
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
