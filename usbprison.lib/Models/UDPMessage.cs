using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace usbprison.lib.Models
{
    public enum UDPMessageType
    {
        // PORT 49111
        List,  // List of current devices, used for displaying in app
        Notify, // Simple notification message, e.g. "Device XYZ plugged in"

        // PORT 49112
        OK,     // All registered devices are present, used for background service checks
        Alert   // One or more registered devices are missing (Device list shows which ones), used for background service checks
    }

    public class UDPMessage
    {
        public string SenderId { get; set; } = string.Empty;
        public UDPMessageType MessageType { get; set; }
        public string? Message { get; set; }
        public bool IsLockdown { get; set; }
        public List<TrackedDeviceModel>? MissingDevices { get; set; }
        public List<TrackedDeviceModel>? PluggedDevices { get; set; }

    }

}
