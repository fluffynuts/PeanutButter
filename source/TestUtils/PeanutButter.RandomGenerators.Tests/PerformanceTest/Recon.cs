using System;
using System.Collections.Generic;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class Recon : EntityBase
{
    public string PaymentClaimNumber { get; set; }
    public int NumberOfShePenalties { get; set; }
    public int NumberOfTrips { get; set; }
    public decimal LateFee { get; set; }
    public decimal Discount { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public List<Job> Jobs { get; set; }
    public Status Status { get; set; }
    public Contact ServiceProvider { get; set; }
    public DateTime? DateLastPrinted { get; set; }
    public FinNumber FinNumber { get; set; }
    public List<Coordinate> Coordinates { get; set; }
    public string FailureReason { get; set; }
}