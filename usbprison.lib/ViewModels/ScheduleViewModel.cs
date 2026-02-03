using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;

namespace usbprison
{
    public  class ScheduleViewModel : ReactiveObject
    {
        //private readonly SourceCache<DailySchedule, DayOfWeek> dailyCache = new SourceCache<DailySchedule, DayOfWeek>(ds => ds.DayOfWeek);
        private readonly ISettingsService _settingsService;

        public ReadOnlyObservableCollection<DailyScheduleViewModel> DailySchedules { get; }
        public IObservableCache<DailyScheduleViewModel, DayOfWeek> TransformedCache { get; }

        public ScheduleViewModel()
        {
            var settingsService = Splat.Locator.Current.GetService(typeof(ISettingsService)) as ISettingsService;
            _settingsService = settingsService!;

            var list = _settingsService.DailySchedule.Connect()
                .Transform(x => new DailyScheduleViewModel(x))
                .Publish();

            TransformedCache = list.AsObservableCache();// _settingsService.DailySchedule.Connect()
                // .Transform(x => new DailyScheduleViewModel(x))
                // .AsObservableCache();

            list.SortAndBind(out var dailySchedules, SortExpressionComparer<DailyScheduleViewModel>.Ascending(x=>x.DayOfWeek))
                .Subscribe();
            DailySchedules = dailySchedules;

            list.WhenAnyPropertyChanged()
                .Subscribe(async x =>
                {
                    if (x != null)
                    {
                        // whenever the list contents change, save settings
                        x.DailySchedule.LockdownStart = x.StartTime;
                        x.DailySchedule.LockdownEnd = x.EndTime;
                        await _settingsService.SaveSettingsAsync().ConfigureAwait(false);
                    }
                });

            list.Connect();



        }

    }
}
