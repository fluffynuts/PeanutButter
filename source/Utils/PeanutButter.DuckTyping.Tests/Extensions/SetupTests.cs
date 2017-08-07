using System;
using NExpect;
using NUnit.Framework;

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    [SetUpFixture]
    public class SetupTests
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            Assertions.RegisterAssertionsFactory(s =>
            {
                try
                {
                    return new AssertionException(s);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error constructing assertion: {ex.Message}");
                    throw;
                }
            });
        }
    }
}