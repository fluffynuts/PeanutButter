using System;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class WageViewModel:ViewModelBase
    {
        public DateTime StartDate { get; set; }
        public DateTime TerminationDate { get; set; }
        public string TerminationReason { get; set; }
        public decimal WageRate { get; set; }
        public WagePeriod WageEarningPeriod { get; set; }
        public WagePeriod WagePayoutPeriod { get; set; }
    }
}