using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace PeanutButter.TestUtils.Entity.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class AllowSharedTempDbInstancesAttribute : TestActionAttribute
    {
        public override ActionTargets Targets => ActionTargets.Suite;

        public override void AfterTest(ITest test)
        {
            SharedDatabaseLocator.Cleanup();
        }
    }
}