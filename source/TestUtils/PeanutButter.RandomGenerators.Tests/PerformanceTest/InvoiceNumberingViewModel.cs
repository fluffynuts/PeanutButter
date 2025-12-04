namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class InvoiceNumberingViewModel
{
    public string Id { get; set; }
    public int SequenceNumber { get; set; }
    public string LatestInvoiceNumber { get; set; }
    public string NextInvoiceNumber { get; set; }
    public bool OtherInvoiceTypes { get; set; }
    public InvoiceNumberingType InvoiceNumberingType { get; set; }
}