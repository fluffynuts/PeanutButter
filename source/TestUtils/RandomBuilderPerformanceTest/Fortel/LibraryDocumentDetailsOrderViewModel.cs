namespace RandomBuilderPerformanceTest.Fortel
{
    public class LibraryDocumentDetailsOrderViewModel : LibraryDocumentDetailsViewModel
    {
        public string FinNumber { get; set; }
        public string DocumentFinNumber { get; set; }

        public override string ToString()
        {
            return $"FIN Number: {FinNumber}\nDocument FIN Number: {DocumentFinNumber}";

        }
    }
}