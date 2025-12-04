namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class Team : EntityBase
{
    public string Code { get; set; }
    public string Description { get; set; }
    public decimal Percentage { get; set; }
    public string Domain { get; set; }
    public bool IsActive { get; set; }
    public string CompanyId { get; set; }
}