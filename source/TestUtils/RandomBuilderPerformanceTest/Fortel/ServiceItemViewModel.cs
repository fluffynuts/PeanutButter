using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class ServiceItemViewModel : ViewModelBase
    {
        [Required]
        [Display(Name = "Service Number")]
        public string ServiceNumber { get; set; }
        [Required]
        [Display(Name = "Contract Item Number")]
        public string ContractItemNumber { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Unit { get; set; }
        [Display(Name = "BOQ Definition")]
        public string BoqDefinition { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public decimal EstimatedQuantity { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public decimal ActualQuantity { get; set; }
        public SelectList DomainsSelectList { get; set; }
        [Required]
        public string Domain { get; set; }
        public string WorkType { get; set; }
        public List<RateViewModel> RateViewModels { get; set; }
        public RateViewModel RateViewModel { get; set; }
        public string CompanyId { get; set; }

        [Required]
        [Display(Name = "Contract Number")]
        public string ContractNumber { get; set; }
        public FinNumberViewModel JobNumber { get; set; }

        public RateViewModel LatestRateViewModel { get; set; }

        public decimal EstimatedAmount => EstimatedQuantity * LatestRateViewModel?.Value ?? 0;

        public decimal ActualAmount => CalculateAmount(ActualQuantity);

        private decimal CalculateAmount(decimal quantity)
        {
            return Math.Round(quantity * LatestRateViewModel?.Value ?? 0, 2);
        }



        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public decimal CompositeActualQuantity { get; set; }
        public decimal CompositeActualAmount => CalculateAmount(CompositeActualQuantity);

        public string ModificationInfo
        {
            get
            {
                var info = "Not Available";
                if (!string.IsNullOrEmpty(CreatedUsername))
                {
                    info = $"Created by {CreatedUsername} on {DateCreated.ToString(DateTimeExtensions.DateTimeFormat)}";
                    if (!string.IsNullOrEmpty(LastModifiedUsername))
                    {
                        info += $"\nLast Modified by {LastModifiedUsername} on {DateLastModified.ToString(DateTimeExtensions.DateTimeFormat)}";
                    }
                }
                return info;
            }
        }
    }
}