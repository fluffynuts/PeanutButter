using PeanutButter.RandomGenerators;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class InvoiceViewModelBuilder : GenericBuilder<InvoiceViewModelBuilder, InvoiceViewModel>
    {
        public InvoiceViewModelBuilder WithReconViewModel(ReconViewModel viewModel)
        {
            return WithProp(x => x.ReconViewModel = viewModel);
        }

        public InvoiceViewModelBuilder WithReconId(string id)
        {
            return WithProp(x => x.ReconId = id);
        }
        public InvoiceViewModelBuilder WithStatus(Status status)
        {
            return WithProp(x => x.Status = status);
        }
        public InvoiceViewModelBuilder WithFinNumberViewModel(FinNumberViewModel finNumberViewModel)
        {
            return WithProp(x => x.FinNumberViewModel= finNumberViewModel);
        }
    }
}