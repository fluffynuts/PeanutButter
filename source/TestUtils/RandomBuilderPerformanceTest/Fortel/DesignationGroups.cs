namespace RandomBuilderPerformanceTest.Fortel
{
    public static class DesignationGroups
    {
        public static readonly Designation[] AreaManagerDesignations =
        {
            Designation.AreaManager, Designation.DCPowerManager, Designation.OpsManager
        };

        public static readonly Designation[] TeamLeaderDesignations =
        {
            Designation.TeamLeader, Designation.SiteSupervisor, Designation.PlanningOfficer,
            Designation.Specialist, Designation.Technician, Designation.BuildOfficer, Designation.Foreman, Designation.HealthAndSafetyOfficer
        };

        public static readonly Designation[] AdminOfficerDesignations =
        {
            Designation.MidAdminOfficer, Designation.JuniorAdminOfficer, Designation.AdminSupervisor, Designation.AccountsSupervisor, Designation.AdminManager
        };

        public static readonly Designation[] DirectorDesignations =
        {
            Designation.Director
        };
        public static readonly Designation[] HealthAndSafetyDesignations =
        {
            Designation.HealthAndSafetyOfficer
        };
        public static readonly Designation[] SupervisorDesignations =
        {
            Designation.SiteSupervisor
        };
    }
}