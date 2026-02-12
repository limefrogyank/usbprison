
using DynamicData;
using System.Text.Json.Serialization;

namespace usbprison
{
    public interface ISettingsService
    {
        //List<TrackedDeviceModel> TrackedDevicesList { get; set; }
        //[JsonIgnore] public SourceCache<TrackedDeviceModel, string> TrackedDevices { get; }

        List<DailySchedule> DailyScheduleList { get; set; }
        [JsonIgnore] public SourceCache<DailySchedule, DayOfWeek> DailySchedule { get; }
        Task SaveSettingsAsync();
    }
}