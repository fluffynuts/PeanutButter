using System;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class DomainDescriptor : EntityBase
    {
        public string Domain { get; set; }
        public string WorkType { get; set; }
        public string ContractNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Descriptor { get; set; }
    }
}