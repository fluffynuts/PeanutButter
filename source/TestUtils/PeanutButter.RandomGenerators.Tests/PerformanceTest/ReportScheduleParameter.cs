using System.Collections.Generic;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class ReportScheduleParameter
    {
        public string ParameterName { get; set; }
        public string ParameterDisplayName { get; set; }
        public ReportParameterType ReportParameterType { get; set; }
        public Dictionary<string, object> Details { get; set; }
    }
}