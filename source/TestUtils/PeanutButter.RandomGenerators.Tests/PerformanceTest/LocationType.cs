using System.ComponentModel.DataAnnotations;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public enum LocationType
    {
        [Display(Name = "Telkom Yard")] TelkomYard,
        [Display(Name = "Site Location")] SiteLocation,
        [Display(Name = "Fortel Yard")] FortelYard
    }
}