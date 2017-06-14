using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class OrderViewModel : ViewModelBase
    {

        [Display(Name = "Order date")]
        public DateTime? OrderDate { get; set; }

        [Display(Name = "PO number")]
        public string PoNumber { get; set; }

        [Display(Name = "SAP number")]
        public string SapNumber { get; set; }

        [Display(Name = "REF")]
        public string ReferenceNumber { get; set; }

        [Display(Name = "Work description")]
        public string WorkDescription { get; set; }

        [Display(Name = "Network number")]
        public string NetworkNumber { get; set; }

        [Display(Name = "Contract start")]
        public DateTime? ContractStart { get; set; }

        [Display(Name = "Contract end")]
        public DateTime? ContractEnd { get; set; }

        [Display(Name = "Rate Selection Date")]
        public DateTime? RateSelectionDate { get; set; }

        [Required(ErrorMessage = "Contract Number is a required field")]
        [Display(Name = "Contract number")]
        public string ContractNumber { set; get; }

        public bool IsClosed { get; set; }
        public string OrderStatus => IsClosed ? Status.Closed.ToString() : Status.Open.ToString();
        public SelectList LocationsSelectList { get; set; }
        public string ContractDetailsId { get; set; }
        public SelectList DomainsSelectList { get; set; }
        public SelectList CustomerSelectList { get; set; }
        public SelectList WorkTypeSelectList { get; set; }
        public SelectList ReceiptMethodList { get; set; }
        [Required(ErrorMessage = "Domain Category is a required field")]
        public string DomainCategoryId { get; set; }
        public string DomainCategoryCategory { get; set; }
        public string BaseLocationId { get; set; }
        [Required(ErrorMessage = "Customer is a required field")]
        public string CustomerId { get; set; }
        public string CustomerRepId { get; set; }
        public Contact CustomerRep { get; set; }
        public string SeniorCustomerRepId { get; set; }
        public string ManagerId { get; set; }
        public string SeniorManagerId { get; set; }
        public string ExecutiveId { get; set; }

        public HeaderViewModel HeaderViewModel { get; set; }
        public LocationViewModel SiteLocationViewModel { get; set; }
        public string SiteLocationId { get; set; }
        [Display(Name = "Vendor code")]
        public string VendorCode { get; set; }
        public string ReceiptMethodId { get; set; }
        public string WorkTypeId { get; set; }
        public string WorkTypeDescription { get; set; }
        public FinNumberViewModel FinNumberViewModel { get; set; }
        public string Domain { get; set; }
        public List<JobViewModel> JobViewModels { get; set; }
        public List<InvoiceViewModel> InvoiceViewModels { get; set; }
        public List<ServiceItemViewModel> ServiceItemViewModels { get; set; }
        public List<InvoicePaymentReconViewModel> InvoicePaymentReconViewModels { get; set; }
        public CompanyViewModel CustomerViewModel { get; set; }
        public List<QualityAssuranceViewModel> AllQualityAssurances
        {
            get
            {
                var qualityAssurances = new List<QualityAssuranceViewModel>();
                if (JobViewModels == null) return qualityAssurances;
                foreach (var jobViewModel in JobViewModels)
                {
                    if (jobViewModel?.QualityAssuranceViewModels == null) continue;
                    foreach (var viewModel in jobViewModel.QualityAssuranceViewModels)
                    {
                        viewModel.OrderId = Id;
                        viewModel.JobId = jobViewModel.Id;
                        if (jobViewModel.FinNumberViewModel != null)
                            viewModel.JobFinNumber = jobViewModel.FinNumberViewModel.FormattedJobNumber;
                        viewModel.JobStatus = jobViewModel.Status;
                        viewModel.CanSubmitForRecon = ((viewModel.Status == Status.Passed) &&
                                                       (viewModel.JobStatus == Status.SubmittedForRecon));
                        viewModel.CanBypassQa = viewModel.Status == Status.Open;
                        qualityAssurances.Add(viewModel);
                    }
                }
                return qualityAssurances;
            }
        }

        public List<ReconViewModel> ReconViewModels { get; set; }
        public string LastTabSelected { get; set; }
        public bool DocumentExists { get; set; }
        public string Teams
        {
            get
            {
                if (JobViewModels == null) return "";
                var designation = Designation.TeamLeader;
                var contactViewModels = JobViewModels
                    .Where(jobViewModel => jobViewModel.ContactViewModels != null)
                    .SelectMany(viewModel => viewModel.ContactViewModels);

                var viewModels = contactViewModels
                    .Where(contactViewModel => contactViewModel?.Designation == designation)
                    .Select(contactViewModel => contactViewModel.Teams.FirstOrDefault()?.Description)
                    .Distinct()
                    .OrderBy(description => description);

                var teams = viewModels.Count() > 4 ? 
                    "Multiple Teams"
                    : 
                    string.Join(",", viewModels);
                return teams;
            }
        }
    }
}