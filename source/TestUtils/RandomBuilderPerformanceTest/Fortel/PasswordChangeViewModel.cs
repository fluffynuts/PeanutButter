using System.ComponentModel.DataAnnotations;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class PasswordChangeViewModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Old password")]
        [Required(ErrorMessage = "This field is required.")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        [Required(ErrorMessage = "This field is required.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Required(ErrorMessage = "This field is required.")]
        [Compare("NewPassword", ErrorMessage = "Passwords don't match")]
        public string PasswordConfirm { get; set; }
    }
}