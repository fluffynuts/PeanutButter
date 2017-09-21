using System.ComponentModel.DataAnnotations;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public enum LocationType
    {
        [Display(Name = "Frobnozzle Yard")] FrobNozzleYard,
        [Display(Name = "Site Location")] SiteLocation,
        [Display(Name = "Wingbat Yard")] WingbatYard
    }
}