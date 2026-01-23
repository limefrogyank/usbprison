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
        private readonly IPEndPoint _mainEndpoint;
        private readonly IPEndPoint _backgroundEndpoint;
        private CancellationTokenSource? _cancelTokenForListen;

        private Subject<string> _rawMessageSubject = new Subject<string>();
        public IObservable<string> RawMessageReceived => _rawMessageSubject.AsObservable();

        private Subject<string> _notificationSubject = new Subject<string>();
        public IObservable<string> NotificationReceived => _notificationSubject.AsObservable();

        private Subject<List<TrackedDeviceModel>> _lastestDevicesSubject = new Subject<List<TrackedDeviceModel>>();
        public IObservable<List<TrackedDeviceModel>> LastestDevicesReceived => _lastestDevicesSubject.AsObservable();


        public UDPService()
        {
            //_udpClient = new System.Net.Sockets.UdpClient(UDPService.PORT);
            _mainEndpoint = new IPEndPoint(IPAddress.Broadcast, UDPService.MainPort);
            _backgroundEndpoint = new IPEndPoint(IPAddress.Broadcast, UDPService.BackgroundPort);
        }

        public async Task StartListening()
        {
            _cancelTokenForListen = new CancellationTokenSource();
            await Task.Run(() => ReceiveLoopAsync(_cancelTokenForListen.Token));
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            using var udpClient = new UdpClient(UDPService.MainPort) { EnableBroadcast = true };
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
                                _lastestDevicesSubject.OnNext(udpmessage.PluggedDevices ?? new List<TrackedDeviceModel>());
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
            using var udpClient = new UdpClient(UDPService.BackgroundPort) { EnableBroadcast = true };
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
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                using var udpClient = new UdpClient() { EnableBroadcast = true };
                switch (message.MessageType)
                {
                    case UDPMessageType.List:
                    case UDPMessageType.Notify:
                        await udpClient.SendAsync(data, data.Length, _mainEndpoint);
                        break;
                    case UDPMessageType.OK:
                    case UDPMessageType.Alert:
                        await udpClient.SendAsync(data, data.Length, _backgroundEndpoint);
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
