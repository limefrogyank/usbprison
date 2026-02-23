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
        
        public abstract Task SaveSettingsAsync();
        
        public SettingsServiceBase(bool uselessParameter)
        {
            LoadSettings();

            TrackedDevices.Connect()
                .ToCollection()
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(
                    async x =>
                    {
                        await SaveSettingsAsync();
                    }
                );
            DailySchedule.Connect()
                .ToCollection()
                .ObserveOn(RxSchedulers.MainThreadScheduler)
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
