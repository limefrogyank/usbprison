using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace usbprison
{
    public partial class DailyScheduleViewModel : ReactiveObject
    {
        private readonly DailySchedule _dailySchedule;
        public DailySchedule DailySchedule => _dailySchedule;

        [Reactive] private TimeSpan _startTime;
        [Reactive] private TimeSpan _endTime;

        public DayOfWeek DayOfWeek => _dailySchedule.DayOfWeek;

        public DailyScheduleViewModel(DailySchedule dailySchedule) 
        {
            _dailySchedule = dailySchedule;
            _startTime = dailySchedule.LockdownStart;
            _endTime = dailySchedule.LockdownEnd;

            this.WhenAnyValue(x=>x.StartTime,x=>x.EndTime).Subscribe(x=>
            {

            });

        }
    }
}
