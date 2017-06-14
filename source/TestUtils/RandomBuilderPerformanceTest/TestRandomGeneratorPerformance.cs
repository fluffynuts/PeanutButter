using System;
using NUnit.Framework;
using RandomBuilderPerformanceTest.Fortel;

namespace RandomBuilderPerformanceTest
{
    [TestFixture]
    public class TestRandomGeneratorPerformance
    {
        [Test]
        public void TestPerformance()
        {
            Console.WriteLine($"Building random object");

            var startDate = DateTime.Now;
            var testObject = InvoiceViewModelBuilder.BuildRandom();
            var elapsedTime = DateTime.Now - startDate;

            Console.WriteLine($"Time taken to create random object - {elapsedTime}");
            Console.ReadLine();
        }
    }
}