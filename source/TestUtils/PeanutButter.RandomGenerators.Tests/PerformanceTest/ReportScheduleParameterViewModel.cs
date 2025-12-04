using System.Collections.Generic;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class ReportScheduleParameterViewModel
{
    public string ParameterDisplayName { get; set; }
    public string ParameterName { get; set; }
    public ReportParameterType ReportParameterType { get; set; }
    public IDictionary<string, object> Details { get; set; }
}