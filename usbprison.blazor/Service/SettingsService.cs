using DynamicData;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace usbprison.blazor
{
    public class SettingsService : SettingsServiceBase
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;

        private async Task SetItemAsync(string key, string value)
        {            
            await JS.InvokeVoidAsync("storageHelper.setItem", key, value);
        }

        private async Task<string?> GetItemAsync(string key, string defaultResult = "")
        {
            var result = await JS.InvokeAsync<string>("storageHelper.getItem", key);
            return result;
        }

        protected override async void LoadSettings()
        {
            try
            {

                var settingsJson = await GetItemAsync("SettingsService", "{}");
                var settings = System.Text.Json.JsonSerializer.Deserialize<SettingsService>(settingsJson);

                if (settings != null)
                {
                    if (settings.DailyScheduleList == null || settings.DailyScheduleList.Count == 0)
                    {
                        // not initialized yet, do it now
                        settings.DailyScheduleList = new List<DailySchedule>
                        {
                            new DailySchedule{DayOfWeek=DayOfWeek.Sunday},
                            new DailySchedule{DayOfWeek=DayOfWeek.Monday},
                            new DailySchedule{DayOfWeek=DayOfWeek.Tuesday},
                            new DailySchedule{DayOfWeek=DayOfWeek.Wednesday},
                            new DailySchedule{DayOfWeek=DayOfWeek.Thursday},
                            new DailySchedule{DayOfWeek=DayOfWeek.Friday},
                            new DailySchedule{DayOfWeek=DayOfWeek.Saturday}
                        };
                    }
                    this.DailyScheduleList = settings.DailyScheduleList;


                    this.TrackedDevicesList = settings.TrackedDevicesList;
                }
            }
            catch
            {
                this.TrackedDevicesList = new List<TrackedDeviceModel>();
            }
        }

        public override async Task SaveSettingsAsync()
        {
            var settingsJson = System.Text.Json.JsonSerializer.Serialize<SettingsService>(this, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
            await SetItemAsync("SettingsService", settingsJson);
            //return Task.CompletedTask;
            //return System.IO.File.WriteAllTextAsync("settings.json", settingsJson);
        }

        public SettingsService() : base() { }
        public SettingsService(bool uselessParameter) : base(uselessParameter) { }

    }
}
