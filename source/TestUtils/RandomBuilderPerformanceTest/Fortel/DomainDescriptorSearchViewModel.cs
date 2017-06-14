using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class DomainDescriptorSearchViewModel
    {
        public SelectList DomainSelectList { get; set; }
        [Display(Name = "Domain")]
        public string Domain { get; set; }
        [Display(Name = "Only display active descriptors")]
        public bool OnlyDisplayActiveDescriptors { get; set; }
    }
}