using System.ComponentModel.DataAnnotations;

namespace RandomBuilderPerformanceTest.Fortel
{
    public enum UserRole
    {
        [Display(Name = "Manager")]
        Manager,

        [Display(Name = "Non Admin")]
        NonAdmin,

        [Display(Name = "Administrator")]
        Administrator,

        [Display(Name = "Mobile User")]
        MobileUser,

        [Display(Name = "Third Party")]
        ThirdParty
    }
}