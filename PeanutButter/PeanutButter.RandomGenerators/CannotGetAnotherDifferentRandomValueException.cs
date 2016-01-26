using System;

namespace PeanutButter.RandomGenerators
{
    public class CannotGetAnotherDifferentRandomValueException<T>: Exception
    {
        public CannotGetAnotherDifferentRandomValueException(T unwantedValue):
            base($"Unable to get a value different from ${unwantedValue} after ${RandomValueGen.MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS} attempts )':")
        {
        }
    }
}