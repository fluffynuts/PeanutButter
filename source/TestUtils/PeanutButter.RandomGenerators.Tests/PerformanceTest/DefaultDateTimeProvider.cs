using System;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class DefaultDateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;

        public DateTime Today => DateTime.Today;
    }
}