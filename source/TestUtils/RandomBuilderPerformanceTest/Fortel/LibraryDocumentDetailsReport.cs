using System.Collections.Generic;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class LibraryDocumentDetailsReport : LibraryDocumentDetails
    {
        public string ReportScheduleId { get; set; }
        public string ReportName { get; set; }
        public Dictionary<string, string> ParameterValues { get; set; }
        public List<string> Recipients { get; set; }
    }
}