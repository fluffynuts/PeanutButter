using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class TeamViewModel : ViewModelBase
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Percentage { get; set; }

        [Required]
        public string Domain { get; set; }

        [DisplayName(@"Active")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = @"Please select company")]
        public string CompanyId { get; set; }

        public SelectList CompanySelectList { get; set; }

        [Display(Name = @"Company Name")]
        public string CompanyName { get; set; }
    }
}