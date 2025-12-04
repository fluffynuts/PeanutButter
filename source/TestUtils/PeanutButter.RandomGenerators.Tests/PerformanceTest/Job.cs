using System.Collections.Generic;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class Job : EntityBase
{
    public List<Contact> Contacts { get; set; }
    public List<ServiceItem> ServiceItems { get; set; }
    public List<SiteDiary> SiteDiaries { get; set; }
    public FinNumber FinNumber { get; set; }
    public Status Status { get; set; }
    public List<QualityAssurance> QualityAssurances { get; set; }
    public List<ImajinRecord> ImajinSyncLog { get; set; }
    public Team Team { get; set; }
    public decimal AssignedPenaltyAmount { get; set; }
}