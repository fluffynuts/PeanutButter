using System;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class Schedule
    {
        public ScheduleType ScheduleType { get; set; }
        public DateTime ExecutionTime { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public int? DateOfMonth { get; set; }
    }
}