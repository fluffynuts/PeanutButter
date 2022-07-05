using System;

namespace PeanutButter.TestUtils.AspNetCore
{
    public class FeatureIsObsoleteException
        : Exception
    {
        public FeatureIsObsoleteException(
            string property
        ) : base($"Feature is obsolete: {property}")
        {
        }
    }
}