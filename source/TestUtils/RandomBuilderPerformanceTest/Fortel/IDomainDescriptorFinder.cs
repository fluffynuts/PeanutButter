namespace RandomBuilderPerformanceTest.Fortel
{
    public interface IDomainDescriptorFinder
    {
        DomainDescriptor FindDescriptorForOrder(Order order);
    }
}