namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public interface IDomainDescriptorFinder
    {
        DomainDescriptor FindDescriptorForOrder(Order order);
    }
}