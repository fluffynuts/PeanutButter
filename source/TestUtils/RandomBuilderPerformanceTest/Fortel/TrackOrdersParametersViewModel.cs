using System;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class TrackOrdersParametersViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FinNumber { get; set; }
        public string PoNumber { get; set; }
        public string RefNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public string NetworkNumber { get; set; }
    }
}