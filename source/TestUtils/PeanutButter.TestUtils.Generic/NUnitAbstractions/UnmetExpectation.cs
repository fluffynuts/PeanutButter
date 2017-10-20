using System;

namespace PeanutButter.TestUtils.Generic.NUnitAbstractions
{
    internal class UnmetExpectation : Exception
    {
        public UnmetExpectation(string message) : base(message)
        {
        }
    }
}