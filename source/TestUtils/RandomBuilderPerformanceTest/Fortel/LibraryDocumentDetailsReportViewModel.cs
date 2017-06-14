using System.Collections.Generic;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class LibraryDocumentDetailsReportViewModel : LibraryDocumentDetailsViewModel
    {
        public string ReportScheduleId { get; set; }
        public string ReportName { get; set; }
        public Dictionary<string, string> ParameterValues { get; set; }
        public List<string> Recipients { get; set; }

        public override string ToString()
        {
            var details = $"Report: {ReportName}\nDelivered to {Recipients?.Count ?? 0} users(s)\nParameters\n";
            foreach (var param in ParameterValues)
            {
                details += $"{param.Key}: {param.Value}\n";
            }
            return details.Trim();
        }
    }
}