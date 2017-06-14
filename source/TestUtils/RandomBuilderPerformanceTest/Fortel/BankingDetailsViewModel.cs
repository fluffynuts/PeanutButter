using System.ComponentModel.DataAnnotations;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class BankingDetailsViewModel:ViewModelBase
    {
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }
        [Display(Name = "Bank Branch")]
        public string BankBranch { get; set; }
        [Display(Name = "Bank Branch Code")]
        public string BankBranchCode { get; set; }
       
    }
}