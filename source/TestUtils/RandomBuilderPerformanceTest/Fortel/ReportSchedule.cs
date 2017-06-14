using System.Collections.Generic;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class ReportSchedule : EntityBase
    {
        public ReportType  ReportType { get; set; }
        public string ReportMethodName { get; set; }
        public List<Contact> Contacts { get; set; }
        public List<ReportScheduleParameter> ReportScheduleParameters { get; set; }
        public Schedule Schedule { get; set; }
    }
}