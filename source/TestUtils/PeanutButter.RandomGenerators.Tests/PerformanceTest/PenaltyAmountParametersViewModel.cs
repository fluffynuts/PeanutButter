using System;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class PenaltyAmountParametersViewModel
{
    public string OrderId { get; set; }
    public string[] JobIds { get; set; }
    public int NumberOfShePenalties { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
}