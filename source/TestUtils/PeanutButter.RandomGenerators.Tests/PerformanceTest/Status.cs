using System.ComponentModel;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public enum Status
    {
        [Description("Open")] Open,
        [Description("Submitted For QA")] SubmittedForQa,
        [Description("Submitted For Recon")] SubmittedForRecon,
        [Description("Recon In Progress")] ReconInProgress,
        [Description("QA In Progress")] QaInProgress,
        [Description("Failed QA")] FailedQa,
        [Description("Failed")] Failed,
        [Description("Failed Recon")] FailedRecon,
        [Description("Passed")] Passed,
        [Description("Recon Quality Failure")] FailedReconQuality,
        [Description("Recon Quantity Failure")] FailedReconQuantity,
        [Description("Submitted For Invoicing")] SubmittedForInvoicing,
        [Description("Invoice In Progress")] InvoiceInProgress,
        [Description("Invoice Generated")] InvoiceGenerated,
        [Description("Cancelled Invoice")] CancelledInvoice,
        [Description("Closed")] Closed,
        [Description("Deleted")] Deleted,
        [Description("Invoice Paid")] InvoicePaid
    }
}