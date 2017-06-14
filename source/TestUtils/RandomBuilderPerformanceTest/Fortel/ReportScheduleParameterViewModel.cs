using System.Collections.Generic;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class ReportScheduleParameterViewModel
    {
        public string ParameterDisplayName { get; set; }
        public string ParameterName { get; set; }
        public ReportParameterType ReportParameterType { get; set; }
        public IDictionary<string, object> Details { get; set; }

    }
}