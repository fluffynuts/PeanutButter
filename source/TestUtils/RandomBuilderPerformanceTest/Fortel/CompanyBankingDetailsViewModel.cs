using System.ComponentModel.DataAnnotations;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class CompanyBankingDetailsViewModel :BankingDetailsViewModel
    {
        [Display(Name = "Use on Invoice")]
        public bool UseOnInvoice { get; set; }
       

    }
}