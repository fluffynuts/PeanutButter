using System;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable PublicConstructorInAbstractClass

namespace PeanutButter.TempDb
{
    public class FatalTempDbInitializationException : Exception
    {
        public FatalTempDbInitializationException(string message)
            : base($"Fatal error whilst attempting to initialize TempDb instance: {message}")
        {
        }
    }
}