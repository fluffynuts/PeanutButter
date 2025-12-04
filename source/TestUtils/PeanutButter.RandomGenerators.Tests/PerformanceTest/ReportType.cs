using System.ComponentModel;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public enum ReportType
{
    [Description("Job Card Summary")] JobCardSummary,
    [Description("Daily Update")] DailyUpdate,
    [Description("Order Summary")] OrderSummary,
    [Description("Invoice Penalties")] InvoicePenalties,
    [Description("Orders With No Jobs")] OrdersWithNoJobs,
    [Description("Customer Statement")] CustomerStatement
}