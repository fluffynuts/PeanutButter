using System.Collections.Generic;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class LibraryDocumentDetailsReport : LibraryDocumentDetails
{
    public string ReportScheduleId { get; set; }
    public string ReportName { get; set; }
    public Dictionary<string, string> ParameterValues { get; set; }
    public List<string> Recipients { get; set; }
}