using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class UnpaidInvoicesViewModel:ViewModelBase
    {
        [DisplayName("Fin Number")]
        public string FinNumber { get; set; }
        [DisplayName("Invoice Date")]
      
        public DateTime InvoiceDate { get; set; }
        [DisplayName("Invoice Number")]
        public string InvoiceNumber { get; set; }
        [DisplayName("PO Number")]
        public string PoNumber { get; set; }
        [DisplayName("Reference Number")]
        public string RefNumber { get; set; }
        public decimal TotalInclVat { get; set; }
        [DisplayName("Payment Date")]

        public DateTime PaymentDate { get; set; }
        [DisplayName("Payment Amount")]
        [DisplayFormat(DataFormatString = "{0:0.00}")]
        public decimal PaymentAmount { get; set; }
        public decimal BalanceOutStanding { get; set; }
        [DisplayName("Payment Reference")]
        public string PaymentReference { get; set; }
        public int RowNumber { get; set; }
        public string OrderId { get; set; }
        public string InvoiceId { get; set; }
        public string GeneratedInvoiceNumber { get; set; }
        public string FortelInvoiceNumber { get; set; }
        public double NumberOfReconInvoices { get; set; }

        [DisplayName("Total Incl VAT")]
        public string FormattedTotalInclVat { get; set; }
        [DisplayName("Outstanding Balance")]
        public string FormattedBalanceOutStanding { get; set; }


    }
}