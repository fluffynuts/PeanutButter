using System;
using System.Collections.Generic;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class SiteDiary : EntityBase
    {
        public string Description { get; set; }
        public List<Attachment> Attachments { get; set; }
        public DateTime SiteDiaryDate { get; set; }
    }
}