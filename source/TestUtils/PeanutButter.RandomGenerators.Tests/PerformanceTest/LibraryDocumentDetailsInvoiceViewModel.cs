namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class LibraryDocumentDetailsInvoiceViewModel : LibraryDocumentDetailsViewModel
{
    public string FinNumber { get; set; }
    public string DocumentFinNumber { get; set; }
    public string GeneratedInvoiceNumber { get; set; }

    public override string ToString()
    {
        return
            $"FIN Number: {FinNumber}\nDocument FIN Number: {DocumentFinNumber}\nGenerated Invoice Number: {GeneratedInvoiceNumber}";
    }
}