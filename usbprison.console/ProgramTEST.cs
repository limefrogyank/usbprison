// // See https://aka.ms/new-console-template for more information
// using LibUsbDotNet;
// using LibUsbDotNet.DeviceNotify;
// using LibUsbDotNet.Info;
// using LibUsbDotNet.Main;
// using System;
// using System.Collections.ObjectModel;
// namespace TEST {

// Console.WriteLine("Hello, World!");
// // Hook the device notifier event
// var notifier = new LibUsbDotNet.DeviceNotify.Linux.LinuxDeviceNotifier();
// notifier.OnDeviceNotify += OnDeviceNotifyEvent;

// void OnDeviceNotifyEvent(object? sender, DeviceNotifyEventArgs e)
// {
//     Console.SetCursorPosition(0, Console.CursorTop);

//     Console.WriteLine(e.ToString()); // Dump the event info to output.

//     Console.WriteLine();
//     Console.Write("[Press any key to exit]");
// }

// // load registered Devices
// var registered = Registration.LoadRegisteredDevices();

// using var cts = new CancellationTokenSource();
// Console.CancelKeyPress += (sender, e) =>
// {
//     e.Cancel = true; // Prevent immediate termination
//     cts.Cancel();
//     Console.WriteLine("\nStopping loop...");
// };

// UsbRegDeviceList allDevices = UsbDevice.AllDevices;

// UsbDevice MyUsbDevice;
// foreach (UsbRegistry usbRegistry in allDevices)
// {
//     if (usbRegistry.Open(out MyUsbDevice))
//     {
//         Console.WriteLine(MyUsbDevice.Info.ToString());
//         for (int iConfig = 0; iConfig < MyUsbDevice.Configs.Count; iConfig++)
//         {
//             UsbConfigInfo configInfo = MyUsbDevice.Configs[iConfig];
//             Console.WriteLine(configInfo.ToString());

//             ReadOnlyCollection<UsbInterfaceInfo> interfaceList = configInfo.InterfaceInfoList;
//             for (int iInterface = 0; iInterface < interfaceList.Count; iInterface++)
//             {
//                 UsbInterfaceInfo interfaceInfo = interfaceList[iInterface];
//                 Console.WriteLine(interfaceInfo.ToString());

//                 ReadOnlyCollection<UsbEndpointInfo> endpointList = interfaceInfo.EndpointInfoList;
//                 for (int iEndpoint = 0; iEndpoint < endpointList.Count; iEndpoint++)
//                 {
//                     Console.WriteLine(endpointList[iEndpoint].ToString());
//                 }
//             }
//         }
//     }
// }

// try
// {
//     while (!cts.Token.IsCancellationRequested)
//     {
//         await Task.Delay(TimeSpan.FromSeconds(1), cts.Token);
//     }
// }
// catch (TaskCanceledException)
// {
//     //Expected when Ctrl+C is pressed
// }


// notifier.Enabled = false;  // Disable the device notifier

// // Unhook the device notifier event
// notifier.OnDeviceNotify -= OnDeviceNotifyEvent;

// // Free usb resources.
// // This is necessary for libusb-1.0 and Linux compatibility.
// UsbDevice.Exit();

// // Wait for user input..
// //Console.Read();

// }