using System.ComponentModel.DataAnnotations;

namespace RandomBuilderPerformanceTest.Fortel
{
    public enum LocationType
    {
        [Display(Name = "Telkom Yard")]
        TelkomYard,
        [Display(Name = "Site Location")]
        SiteLocation,
        [Display(Name = "Fortel Yard")]
        FortelYard
    }

}