using System;
using System.Collections.Generic;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class SiteDiaryViewModel : ViewModelBase
    {
        public string Fin { get; set; }
        public string PoNumber { get; set; }
        public string Ref { get; set; }
        public string Description { get; set; }
        public List<AttachmentViewModel> Attachments { get; set; }
        public DateTime? SiteDiaryDate { get; set; }
        public string Source { get; set; }
        public string Username { get; set; }
    }
}