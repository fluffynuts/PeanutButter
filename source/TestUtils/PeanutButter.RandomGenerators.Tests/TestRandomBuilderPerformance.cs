using System;
using NUnit.Framework;
using PeanutButter.RandomGenerators.Tests.PerformanceTest;
// ReSharper disable UnusedVariable

namespace PeanutButter.RandomGenerators.Tests;

[TestFixture]
public class TestRandomBuilderPerformance
{
    [Test]
    public void TestPerformance()
    {
        Console.WriteLine($"Building random object");

        var startDate = DateTime.Now;
        var testObject = InvoiceViewModelBuilder.BuildRandom();
        var elapsedTime = DateTime.Now - startDate;

        Console.WriteLine($"Time taken to create random object - {elapsedTime}");
    }
}