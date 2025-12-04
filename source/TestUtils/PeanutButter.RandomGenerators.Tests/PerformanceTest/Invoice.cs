namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class Invoice : EntityBase
{
    public Recon Recon { get; set; }
    public Status Status { get; set; }
    public FinNumber FinNumber { get; set; }
    public string GeneratedInvoiceNumber { get; set; }
    public string WingbatInvoiceNumber { get; set; }
    public string CancellationReason { get; set; }
}