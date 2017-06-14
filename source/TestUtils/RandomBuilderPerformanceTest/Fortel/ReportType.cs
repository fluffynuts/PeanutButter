using System.ComponentModel;

namespace RandomBuilderPerformanceTest.Fortel
{
    public enum ReportType
    {
        [Description("Job Card Summary")]
        JobCardSummary,
        [Description("Daily Update")]
        DailyUpdate,
        [Description("Order Summary")]
        OrderSummary,
        [Description("Invoice Penalties")]
        InvoicePenalties,
        [Description("Orders With No Jobs")]
        OrdersWithNoJobs,
        [Description("Customer Statement")]
        CustomerStatement
    }
}