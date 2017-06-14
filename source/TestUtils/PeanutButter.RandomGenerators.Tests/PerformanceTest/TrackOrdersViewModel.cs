namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class TrackOrdersViewModel : OrderViewModel
    {
        public string OrderDescription => $"PO: {PoNumber}\n" +
                                          $"{FinNumberViewModel?.FormattedNumber}\n" +
                                          $"REF: {ReferenceNumber}\n" +
                                          $"Created: {DateCreated.AsFormattedDate()}\n" +
                                          $"Start: {ContractStart.AsFormattedDate()} End: {ContractEnd.AsFormattedDate()}";

        public string CustomerDescription => $"{CustomerViewModel?.TradingName}\n" +
                                             $"{CustomerRep.AsFullName()}\n\n\n" +
                                             $"{SiteLocationViewModel?.AreaLocation}";

        public string SubDomainDescription => $"{DomainCategoryCategory}\n" +
                                              $"{WorkTypeDescription}";

        public int JobCount => JobViewModels?.Count ?? 0;
        public int QaCount => AllQualityAssurances?.Count ?? 0;
        public int ReconCount => ReconViewModels?.Count ?? 0;
        public int InvoiceCount => InvoiceViewModels?.Count ?? 0;
    }
}