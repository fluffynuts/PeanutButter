using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public enum Designation
    {
        [Display(Name = "Customer Representative")] CustomerRepresentative,
        [Display(Name = "Senior Customer Representative")] SeniorCustomerRepresentative,
        Manager,
        [Display(Name = "Senior Manager")] SeniorManager,
        Executive,
        [Display(Name = "F5 Director")] Director,
        [Display(Name = "D5 Area Manager")] AreaManager,
        [Display(Name = "C2 Team Leader")] TeamLeader,
        [Display(Name = "D1 Admin Officer")] MidAdminOfficer,
        [Display(Name = "F4 CEO")] CEO,
        [Display(Name = "F3 CFO")] CFO,
        [Display(Name = "F3 COO")] COO,
        [Display(Name = "E5 Financial Manager")] FinancialManager,
        [Display(Name = "D5 DC Power Manager")] DCPowerManager,
        [Display(Name = "D5 HR Manager")] HRManager,
        [Display(Name = "D3 Admin Supervisor")] AdminSupervisor,
        [Display(Name = "D4 Admin Manager")] AdminManager,
        [Display(Name = "D3 Accounts Supervisor")] AccountsSupervisor,
        [Display(Name = "D3 Ops Manager")] OpsManager,
        [Display(Name = "D3 Quality Manager")] QualityManager,
        [Display(Name = "D3 Safety Manager")] SafetyManager,
        [Display(Name = "D3 Site Supervisor")] SiteSupervisor,
        [Display(Name = "D2 Planning Officer")] PlanningOfficer,
        [Display(Name = "D2 Specialist")] Specialist,
        [Display(Name = "C4 Co-Ordinator")] CoOrdinator,
        [Display(Name = "C3 Technician")] Technician,
        [Display(Name = "C3 Build Officer")] BuildOfficer,
        [Display(Name = "C3 Admin Officer")] JuniorAdminOfficer,
        [Display(Name = "C3 Health and Safety Officer")] HealthAndSafetyOfficer,
        [Display(Name = "C3 Quality Officer")] QualityOfficer,
        [Display(Name = "C2 Storeman")] Storeman,
        [Display(Name = "C2 Heavy Duty Driver")] HeavyDutyDriver,
        [Display(Name = "C1 Foreman")] Foreman,
        [Display(Name = "C1 General Maintenance Officer")] GeneralMaintenanceOfficer,
        [Display(Name = "C1 Assistant Build Officer")] AssistantBuildOfficer,
        [Display(Name = "C1 Facilities Officer")] FacilitiesOfficer,
        [Display(Name = "C1 Contact Person")] ContactPerson,
        [Display(Name = "B2 Technical Assistant")] TechnicalAssistant,
        [Display(Name = "B2 Learner Technician")] LearnerTechnician,
        [Display(Name = "B1 General Assistant")] GeneralAssistant,
        [Display(Name = "A1 Trainee")] Trainee
    }
}