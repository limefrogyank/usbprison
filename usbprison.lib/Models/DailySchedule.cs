using System;
using System.Collections.Generic;
using System.Text;

namespace usbprison
{
    public class DailySchedule
    {
        public TimeSpan LockdownStart { get; set; } = new TimeOnly(22, 0, 0).ToTimeSpan();
        public TimeSpan LockdownEnd { get; set; } = new TimeOnly(7, 0, 0).ToTimeSpan();
        public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Sunday;


    }
}
