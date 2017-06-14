using System.Collections.Generic;
using System.Web.Mvc;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class QualityAssuranceViewModel : ViewModelBase
    {
        public string QualityAssuranceNumber { get; set; }
        public List<AttachmentViewModel> AttachmentViewModels { get; set; }
        public string JobFinNumber { get; set; }
        public string OrderId { get; set; }
        public string JobId { get; set; }
        public SelectList JobSelectList { get; set; }
        public bool ByPassedQa { get; set; }
        public Status JobStatus { get; set; }
        public bool CanBypassQa { get; set; }
        public bool CanSubmitForRecon { get; set; }
    }
}