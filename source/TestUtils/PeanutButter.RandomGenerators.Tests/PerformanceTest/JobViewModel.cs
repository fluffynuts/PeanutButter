using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class JobViewModel : ViewModelBase
{
    public string OrderId { get; set; }

    [Required(ErrorMessage = @"Director is a required field")]
    public string DirectorId { get; set; }

    [Required(ErrorMessage = @"Area Manager is a required field")]
    public string AreaManagerId { get; set; }

    [Required(ErrorMessage = @"Team Leader is a required field")]
    public string TeamLeaderId { get; set; }

    [Required(ErrorMessage = @"Admin Officer is a required field")]
    public string AdminOfficerId { get; set; }

    [Required(ErrorMessage = @"Quality/Safety Officer is a required field")]
    public string QualitySafetyOfficerId { get; set; }

    [Required(ErrorMessage = @"Supervisor is a required field")]
    public string SupervisorId { get; set; }

    [Required(ErrorMessage = "Team is required")]
    public string TeamId { get; set; }

    public SelectList TeamSelectList { get; set; }

    public SelectList DirectorSelectList { get; set; }
    public SelectList AreaManagerSelectList { get; set; }
    public SelectList TeamLeaderSelectList { get; set; }
    public SelectList AdminOfficerSelectList { get; set; }
    public SelectList QualitySafetyOfficerSelectList { get; set; }
    public SelectList SupervisorSelectList { get; set; }
    public SelectList JobSelectList { get; set; }
    public List<ServiceItemViewModel> ServiceItemViewModels { get; set; }
    public TeamViewModel Team { get; set; }
    public List<SiteDiaryViewModel> SiteDiaryViewModels { get; set; }
    public FinNumberViewModel FinNumberViewModel { get; set; }
    public List<QualityAssuranceViewModel> QualityAssuranceViewModels { get; set; }
    public List<ContactViewModel> ContactViewModels { get; set; }

    public new bool CanEdit => Status == Status.Open || Status == Status.FailedQa ||
        Status == Status.FailedReconQuality ||
        Status == Status.FailedReconQuantity;

    public ContactViewModel Director => ContactViewModels?.FirstOrDefault(
        contactViewModel => contactViewModel != null &&
            DesignationGroups.DirectorDesignations.Contains(contactViewModel.Designation));

    public ContactViewModel AreaManager => ContactViewModels?.FirstOrDefault(
        contactViewModel => contactViewModel != null &&
            DesignationGroups.AreaManagerDesignations.Contains(contactViewModel.Designation));

    public ContactViewModel TeamLeader => ContactViewModels?.FirstOrDefault(
        contactViewModel => contactViewModel != null &&
            DesignationGroups.TeamLeaderDesignations.Contains(contactViewModel.Designation));

    public ContactViewModel AdminOfficer => ContactViewModels?.FirstOrDefault(
        contactViewModel => contactViewModel != null &&
            DesignationGroups.AdminOfficerDesignations.Contains(contactViewModel.Designation));

    public ContactViewModel CustomerRepresentative => ContactViewModels?.FirstOrDefault(
        contactViewModel => contactViewModel != null &&
            contactViewModel.Designation == Designation.CustomerRepresentative);

    public string LastTabSelected { get; set; }

    public decimal TotalActualValue { get; set; }
    public decimal AssignedPenaltyAmount { get; set; }

    public bool CanSubmitForQa { get; set; }
}