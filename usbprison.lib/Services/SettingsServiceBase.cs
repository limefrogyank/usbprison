using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace usbprison
{
    public abstract partial class SettingsServiceBase : ReactiveObject, ISettingsService
    {
        //public bool StartMinimized { get; set; } = false;
        //public bool CloseToTray { get; set; } = true;
        //public bool MinimizeToTray { get; set; } = true;
        //public bool ShowNotifications { get; set; } = true;
        [JsonIgnore] public SourceCache<DailySchedule, DayOfWeek> DailySchedule { get; } = new SourceCache<DailySchedule, DayOfWeek>(x => x.DayOfWeek);
        public List<DailySchedule> DailyScheduleList
        {
            get => DailySchedule.Items.ToList();
            set
            {
                DailySchedule.Clear();
                foreach (var item in value)
                {
                    DailySchedule.AddOrUpdate(item);
                }
            }
        }

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

        protected abstract void LoadSettings();
        //{
        //    try
        //    {
        //        var settingsJson = Preferences.Default.Get<string>("SettingsService", "{}");
        //        var settings = System.Text.Json.JsonSerializer.Deserialize<SettingsService>(settingsJson);

        //        if (settings != null)
        //        {
        //            if (settings.DailyScheduleList == null || settings.DailyScheduleList.Count == 0)
        //            {
        //                // not initialized yet, do it now
        //                settings.DailyScheduleList = new List<DailySchedule>
        //                {
        //                    new DailySchedule{DayOfWeek=DayOfWeek.Sunday},
        //                    new DailySchedule{DayOfWeek=DayOfWeek.Monday},
        //                    new DailySchedule{DayOfWeek=DayOfWeek.Tuesday},
        //                    new DailySchedule{DayOfWeek=DayOfWeek.Wednesday},
        //                    new DailySchedule{DayOfWeek=DayOfWeek.Thursday},
        //                    new DailySchedule{DayOfWeek=DayOfWeek.Friday},
        //                    new DailySchedule{DayOfWeek=DayOfWeek.Saturday}
        //                };
        //            }
        //            this.DailyScheduleList = settings.DailyScheduleList;


        //            this.TrackedDevicesList = settings.TrackedDevicesList;
        //        }
        //    }
        //    catch
        //    {
        //        this.TrackedDevicesList = new List<TrackedDeviceModel>();
        //    }
        //}

        public abstract Task SaveSettingsAsync();
        //{

        //    var settingsJson = System.Text.Json.JsonSerializer.Serialize<SettingsService>(this, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
        //    Preferences.Default.Set<string>("SettingsService", settingsJson);
        //    return Task.CompletedTask;
        //    //return System.IO.File.WriteAllTextAsync("settings.json", settingsJson);
        //}

        public SettingsServiceBase(bool uselessParameter)
        {
            LoadSettings();

            TrackedDevices.Connect()
                .ToCollection()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(
                    async x =>
                    {
                        await SaveSettingsAsync();
                    }
                );
            DailySchedule.Connect()
                .ToCollection()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(
                    async x =>
                    {
                        await SaveSettingsAsync();
                    }
                );
        }

        public SettingsServiceBase() { }
    }
}
