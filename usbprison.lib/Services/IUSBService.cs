using DynamicData;
using DynamicData.Binding;

namespace usbprison
{
    public interface IUSBService
    {
        ISourceCache<DeviceModel, string> DeviceCache { get; }
        IObservable<string> DeviceEvents { get; }
        ObservableCollectionExtended<DeviceModel> Devices { get; }

        //void GetInfoForDevice(DeviceModel device);
        //void Test();
    }
}