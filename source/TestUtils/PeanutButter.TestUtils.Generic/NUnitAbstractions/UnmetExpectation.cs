using System;

namespace PeanutButter.TestUtils.Generic
{
    internal class UnmetExpectation : Exception
    {
        public UnmetExpectation(string message) : base(message)
        {
        }
    }
}