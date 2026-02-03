using System.Text.Json.Serialization;

namespace usbprison
{
    public class TrackedDeviceModel : DeviceModel
    {
        public TrackedDeviceModel() { }
        public TrackedDeviceModel(DeviceModel device) 
        {
            WindowsId = device.WindowsId;
            Name = device.Name;
            Pid = device.Pid;
            Vid = device.Vid;
            SerialNumber = device.SerialNumber;
        }
        public string? CustomText { get; set; }
    }

    public class DeviceModel
    {
        public string? WindowsId { get; set; }
        public string Name { get; set; }
        public ushort Pid { get; set; }
        [JsonIgnore] public string PidHex => Pid.ToString("X4");
        public ushort Vid { get; set; }
        [JsonIgnore] public string VidHex => Vid.ToString("X4");
        public ushort Mi { get; set; }        
        public string? SerialNumber { get; set; }
        public string Guid { get;set; }
        public string Id
        {
            get
            {
                if (WindowsId != null)
                {
                    return WindowsId;
                }
                else if (Pid != 0 && Vid != 0 && SerialNumber != null)
                {
                    // windows style ID so that we can potentially use multiple base stations that can recognize the same device 
                    // Why windows style? Why not!? Combines all the necessary elements.
                    return $"USB\\VID_{VidHex}&PID_{PidHex}\\{SerialNumber}";
                }
                else if (SerialNumber != null)
                {
                    return SerialNumber;
                }
                else if (Pid != 0 && Vid != 0)
                {
                    return $"{Vid}:{Pid}";
                }
                else
                {
                    //Guid = System.Guid.NewGuid().ToString();
                    return Guid;
                }
            }
        }

        public DeviceModel()
        {
            Name = "Unknown Device";
            Pid = 0;
            Vid = 0;
            SerialNumber = null;
            Guid = System.Guid.NewGuid().ToString();
        }

        public DeviceModel(string name, ushort pid, ushort vid, string? serialNumber)
        {
            Name = name;
            Pid = pid;
            Vid = vid;
            SerialNumber = serialNumber;
            Guid = System.Guid.NewGuid().ToString();
        }
    }
}