using LibUsbDotNet.DeviceNotify;
using System;
using System.Collections.Generic;
using System.Text;

namespace usbprison.lib.Models
{
    public class GenericDeviceInfo
    {
       public string Model { get; set; }

        public string Manufacturer { get; set; }
        public string Name { get; set; }

        public string Version { get; set; }

        public string Platform { get; set; }

        public string Idiom { get; set; }

        public string DeviceType { get; set; }
    }
}
