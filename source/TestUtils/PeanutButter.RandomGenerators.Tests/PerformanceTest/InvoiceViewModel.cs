using System.Web.Mvc;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class InvoiceViewModel : ViewModelBase
    {
        public string GeneratedInvoiceNumber { get; set; }
        public string FortelInvoiceNumber { get; set; }
        public string OrderId { get; set; }
        public ReconViewModel ReconViewModel { get; set; }
        public SelectList ReconSelectList { get; set; }
        public string ReconId { get; set; }
        public string CancellationReason { get; set; }
        public FinNumberViewModel FinNumberViewModel { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string CustomerPaid { get; set; }
    }
}