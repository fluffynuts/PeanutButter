using System.ComponentModel.DataAnnotations;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class CompanyBankingDetailsViewModel : BankingDetailsViewModel
{
    [Display(Name = "Use on Invoice")]
    public bool UseOnInvoice { get; set; }
}