using System;

namespace RandomBuilderPerformanceTest.Fortel
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
        DateTime Today { get; }
    }
}
