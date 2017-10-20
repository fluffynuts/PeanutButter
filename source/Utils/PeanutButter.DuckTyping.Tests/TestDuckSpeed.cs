using System;
using System.Diagnostics;
using ImpromptuInterface;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;
// ReSharper disable PossibleNullReferenceException
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.DuckTyping.Tests
{
    public class TestDuckSpeed
    {
        [OneTimeSetUp]
        public void WarmUpDlr()
        {
            dynamic i = 1;
            i.ToString();
        }

        private const int TIMES = 1000000;

        [Test]
        [Category("performance")]
        public void TestSetTimed()
        {
            // Compares PB duck-typing with plain reflection and ImpromptuInterfaces (InvokeSet and using an ActLike<> invocation)
            // So far, PB-duck-typing is about twice as slow (when profiled) as II. Perhaps this can get better... Though the penalty
            // may be worth it when considering the extra features (like fuzzy-ducking)
            var tPoco = new PropPoco();

            var tSetValue = "1";

            var tWatch = TimeIt.Go(() =>
            {
                Impromptu.InvokeSet(tPoco, "Prop1", tSetValue);
            }, 500000);
            var tPropertyInfo = tPoco.GetType().GetProperty("Prop1");
            var tWatch2 = TimeIt.Go(() => tPropertyInfo.SetValue(tPoco, tSetValue, new object[] {}), TIMES);
            var ducked = tPoco.DuckAs<IPropPoco>();
            Assert.IsNotNull(ducked);
            var tWatch3 = TimeIt.Go(() => ducked.Prop1 = tSetValue, TIMES);


            TestContext.WriteLine("InvokeSet: " + tWatch.Elapsed);
            TestContext.WriteLine("Reflection: " + tWatch2.Elapsed);
            TestContext.WriteLine("Ducked: " + tWatch3.Elapsed);
        }

        [Serializable]
        public class PropPoco
        {
            public string Prop1 { get; set; }
            public long Prop2 { get; set; }
            public Guid Prop3 { get; set; }
            public int Event { get; set; }
        }

        public interface IPropPoco
        {
            string Prop1 { get; set; }
            long Prop2 { get; set; }
            Guid Prop3 { get; set; }
            int Event { get; set; }

        }


        public static class TimeIt
        {
            public static Stopwatch Go(Action action, int interation = 1000000)
            {
                var tStopwatch = new Stopwatch();
                tStopwatch.Start();
                for (int i = 0; i < interation; i++)
                {
                    action();
                }
                tStopwatch.Stop();
                return tStopwatch;
            }
        }
    }
}