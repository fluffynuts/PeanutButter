using System;
using System.ComponentModel.DataAnnotations;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class RateViewModel : ViewModelBase
    {
        [Required]
        [Display(Name = "Rate Value")]
        public decimal Value { get; set; }

        [Required]
        [Display(Name = "Rate Effective Date")]
        public DateTime EffectiveDate { get; set; }
    }
}