using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Serilog;
using usbprison;
using usbprison.lib.Models;
using usbprison.lib.Services;



 public class UDPServiceEx : UDPService
{
    private IJSObjectReference? module;
    private IJSRuntime JS { get; set; } = default!;

    public UDPServiceEx(GenericDeviceInfo info, IIPService iPService) : base(info, iPService)
    {

    }

    public override void InitializeJS(object jsRuntime)
    {
        JS = (IJSRuntime)jsRuntime;
    }

    public override async Task StartListening()
    {
        Log.Information("Starting UDPServiceEx listening...");
        try
        {
            if (module == null)
            {
                Log.Information("getting module...");
                Log.Information($"JS is null: {JS == null}");
                module = await JS.InvokeAsync<IJSObjectReference>("import", "./udp.js");
                Log.Information("got module");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading JavaScript module");
            throw;
        }

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
            Log.Information("javascript interop...");
        try
        {
            await module.InvokeVoidAsync("initializeUDP","localhost", UDPService.MainPort);
            //await JS.InvokeVoidAsync("udp.Initialize", UDPService.MainPort);
            Log.Information("UDPServiceEx is now listening for messages...");
        }
            catch (Exception ex)
        {
            Log.Error(ex.Message);
            Log.Error( "Error initializing UDP listening via JavaScript");
            throw;
        }
    }
}