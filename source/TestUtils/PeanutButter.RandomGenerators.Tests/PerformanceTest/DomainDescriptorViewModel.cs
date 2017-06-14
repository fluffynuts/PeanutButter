using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class DomainDescriptorViewModel : ViewModelBase
    {
        [Display(Name = "Domain")]
        public string Domain { get; set; }

        [Display(Name = "Work Type")]
        public string WorkType { get; set; }

        [Display(Name = "Contract Number")]
        public string ContractNumber { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Descriptor")]
        public string Descriptor { get; set; }


        [Display(Name = "No End Date")]
        public bool NoEndDate { get; set; }

        public SelectList DomainSelectList { get; set; }
        public SelectList WorkTypeSelectList { get; set; }
    }
}