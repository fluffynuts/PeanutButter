using System;

namespace RandomBuilderPerformanceTest.Fortel
{
    public interface IFinNumberGenerator
    {
        FinNumber GetNextFinNumber(DateTime date);
        FinNumber GetNextJobNumber(Order order);
        FinNumber GetNextReconNumber(Order order);
        FinNumber GetNextInvoiceNumber();
        string GetNextQaNumber(Job job);
        FinNumber GetLatestInvoiceNumber();
    }
}