using System;
using System.Web.Mvc;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class InvoicePaymentReconViewModel : ViewModelBase
    {
        public string OrderId { get; set; }
        public string InvoiceNumber { get; set; }
        public int RowNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentReference { get; set; }
        public SelectList InvoiceSelectList { get; set; }
        public string InvoiceId { get; set; }
    }
}