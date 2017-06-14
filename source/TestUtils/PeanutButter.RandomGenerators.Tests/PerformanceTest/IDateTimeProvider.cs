using System;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
        DateTime Today { get; }
    }
}