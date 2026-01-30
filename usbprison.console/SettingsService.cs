using System.Reactive.Linq;
using System.Text.Json.Serialization;
using DynamicData;
using ReactiveUI;

namespace usbprison
{
    public partial class SettingsService : SettingsServiceBase
    {
       
        protected override void LoadSettings()
        {
            if (!System.IO.File.Exists("settings.json"))
            {
                var writer = System.IO.File.CreateText("settings.json");
                writer.Write(@"{
                ""StartMinimized"": false,
                ""CloseToTray"": true,
                ""MinimizeToTray"": true,
                ""ShowNotifications"": true,
                ""TrackedDevicesList"": []
                }");
                writer.Dispose();

            }
            var settingsJson = System.IO.File.ReadAllText("settings.json");
            var settings = System.Text.Json.JsonSerializer.Deserialize<SettingsService>(settingsJson);
            if (settings != null)
            {                
                this.TrackedDevicesList = settings.TrackedDevicesList;
            }
        }

        public override Task SaveSettingsAsync()
        {
            var settingsJson = System.Text.Json.JsonSerializer.Serialize<SettingsService>(this, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
            return System.IO.File.WriteAllTextAsync("settings.json", settingsJson);
        }

        public SettingsService() : base() { }
        public SettingsService(bool uselessParameter) : base(uselessParameter) { }


    }
}