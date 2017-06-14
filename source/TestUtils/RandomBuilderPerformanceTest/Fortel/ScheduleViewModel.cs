using System;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class ScheduleViewModel
    {
        public ScheduleType ScheduleType { get; set; }
        public DateTime ExecutionTime { get; set; }
        public int ExecutionHours { get; set; }
        public int ExecutionMinutes { get; set; }
        public int MinuteOffSet { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public int? DateOfMonth { get; set; }
    }
}