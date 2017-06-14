using System.ComponentModel.DataAnnotations;

namespace RandomBuilderPerformanceTest.Fortel
{
    public enum WagePeriod
    {
        [Display(Name = "Unknown")]
        Unknown,

        [Display(Name = "Hourly")]
        Hourly,

        [Display(Name = "Daily")]
        Daily,

        [Display(Name = "Weekly")]
        Weekly,
        
        [Display(Name = "Forthnightly")]
        Forthnightly,

        [Display(Name = "Monthly")]
        Monthly,
    }
}