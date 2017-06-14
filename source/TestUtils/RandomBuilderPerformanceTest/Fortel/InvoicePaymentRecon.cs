using System;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class InvoicePaymentRecon : EntityBase
    {
        public int RowNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentReference { get; set; }
        public decimal PaymentAmount { get; set; }
        public string  InvoiceId { get; set; }
    }
}