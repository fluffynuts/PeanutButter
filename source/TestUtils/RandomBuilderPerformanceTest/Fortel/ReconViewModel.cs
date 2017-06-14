using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class ReconViewModel : ViewModelBase
    {
        public string PaymentClaimNumber { get; set; }
        [Range(0, int.MaxValue)]
        public int NumberOfShePenalties { get; set; }
        [Range(0, int.MaxValue)]
        public int NumberOfTrips { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal LatePenaltyAmount { get; set; }
        public decimal SafetyPenaltyAmount { get; set; }
        [DefaultValue(0)]
        public decimal Discount { get; set; }
        public List<JobViewModel> JobViewModels { get; set; }
        public List<CoordinateViewModel> CoordinateViewModels { get; set; }
        public string TotalDistance { get; set; }
        public string OrderId { get; set; }
        public FinNumberViewModel FinNumberViewModel { get; set; }
        public Contact ServiceProvider { get; set; }
        public string ServiceProviderId { get; set; }
        public SelectList ServiceProviderSelectList { get; set; }
        public SelectList LocationSelectList { get; set; }
        public DateTime? DateLastPrinted { get; set; }
        public SelectList PaymentClaimNumberSelectList { get; set; }
        public new bool CanEdit => Status == Status.Open || Status == Status.CancelledInvoice;
        public string LastTabSelected { get; set; }
        public string FailureReason { get; set; }

        public List<ServiceItemViewModel> ServiceItemViewModels
        {
            get
            {
                var serviceItems = new List<ServiceItemViewModel>();
                if (JobViewModels == null) return serviceItems;
                foreach (var jobViewModel in JobViewModels.Where(jobViewModel => jobViewModel?.ServiceItemViewModels != null))
                {
                    var serviceItemsToExclude = new List<ServiceItemViewModel>();
                    foreach (var viewModel in jobViewModel.ServiceItemViewModels)
                    {
                        viewModel.JobNumber = jobViewModel.FinNumberViewModel;
                        viewModel.CompositeActualQuantity = viewModel.ActualQuantity;//set model back to original state
                        SetCompositeActualQuantityForDuplicate(viewModel, serviceItems, serviceItemsToExclude);
                    }

                    serviceItems.AddRange(jobViewModel.ServiceItemViewModels.Except(serviceItemsToExclude));
                }
                return serviceItems;
            }
        }
        public OrderViewModel Order { get; set; }

        private static void SetCompositeActualQuantityForDuplicate(
            ServiceItemViewModel viewModel, 
            List<ServiceItemViewModel> serviceItems,
            List<ServiceItemViewModel> serviceItemsToExclude)
        {
            if (!serviceItems.Contains(viewModel, new ServiceItemViewModelComparer())) return;
            var existingViewModel = serviceItems.FirstOrDefault(x => x.ServiceNumber == viewModel.ServiceNumber);
            if (existingViewModel == null) return;
            {
                existingViewModel.CompositeActualQuantity += viewModel.CompositeActualQuantity;
                var serviceItemIndex =  serviceItems.FindIndex(x => x.ServiceNumber == viewModel.ServiceNumber);
                serviceItems[serviceItemIndex] = existingViewModel;
                serviceItemsToExclude.Add(viewModel);
            }
        }

        private class ServiceItemViewModelComparer : IEqualityComparer<ServiceItemViewModel>
        {
            public bool Equals(ServiceItemViewModel x, ServiceItemViewModel y)
            {
                if (x == null) return false;
                if (y == null) return false;
                return x.ServiceNumber == y.ServiceNumber;
            }

            public int GetHashCode(ServiceItemViewModel obj)
            {
                if (obj == null) return 0;
                return obj.Id.GetHashCode();
            }
        }
    }
}