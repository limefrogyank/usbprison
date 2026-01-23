using System.Reactive.Linq;
using System.Text.Json.Serialization;
using DynamicData;
using ReactiveUI;

namespace usbprison
{
    public partial class SettingsService : ReactiveObject, ISettingsService
    {
        //public bool StartMinimized { get; set; } = false;
        //public bool CloseToTray { get; set; } = true;
        //public bool MinimizeToTray { get; set; } = true;
        //public bool ShowNotifications { get; set; } = true;

        [JsonIgnore] public SourceCache<TrackedDeviceModel, string> TrackedDevices { get; } = new SourceCache<TrackedDeviceModel, string>(dev => dev.Id ?? System.Guid.NewGuid().ToString());
        public List<TrackedDeviceModel> TrackedDevicesList
        {
            get => TrackedDevices.Items.ToList();
            set
            {
                TrackedDevices.Clear();
                foreach (var device in value)
                {
                    TrackedDevices.AddOrUpdate(device);
                }
            }
        }

        private void LoadSettings()
        {
            try
            {
                var settingsJson = Preferences.Default.Get<string>("SettingsService", "{}");
                var settings = System.Text.Json.JsonSerializer.Deserialize<SettingsService>(settingsJson);
                //if (!System.IO.File.Exists("settings.json"))
                //{
                //    var writer = System.IO.File.CreateText("settings.json");
                //    writer.Write(@"{
                //    ""StartMinimized"": false,
                //    ""CloseToTray"": true,
                //    ""MinimizeToTray"": true,
                //    ""ShowNotifications"": true,
                //    ""TrackedDevicesList"": []
                //    }");
                //    writer.Dispose();

                //}
                //var settingsJson = System.IO.File.ReadAllText("settings.json");
                //var settings = System.Text.Json.JsonSerializer.Deserialize<SettingsService>(settingsJson);
                if (settings != null)
                {
                    //this.StartMinimized = settings.StartMinimized;
                    //this.CloseToTray = settings.CloseToTray;
                    //this.MinimizeToTray = settings.MinimizeToTray;
                    //this.ShowNotifications = settings.ShowNotifications;

                    this.TrackedDevicesList = settings.TrackedDevicesList;
                }
            }
            catch
            {
                this.TrackedDevicesList = new List<TrackedDeviceModel>();
            }
        }

        public Task SaveSettingsAsync()
        {

            var settingsJson = System.Text.Json.JsonSerializer.Serialize<SettingsService>(this, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
            Preferences.Default.Set<string>("SettingsService", settingsJson);
            return Task.CompletedTask;
            //return System.IO.File.WriteAllTextAsync("settings.json", settingsJson);
        }

        public SettingsService(bool uselessParameter)
        {
            LoadSettings();

            TrackedDevices.Connect()
                .ToCollection()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(
                    async x =>
                    {
                        //Terminal.Gui.Views.MessageBox.Query(Globals.App,"Saving Settings","Saving settings...", "Ok");
                        await SaveSettingsAsync();
                    }
                );
        }

        public SettingsService() { }


        
    }
}