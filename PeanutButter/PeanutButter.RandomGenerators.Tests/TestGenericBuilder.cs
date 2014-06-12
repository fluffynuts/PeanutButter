using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestGenericBuilder: TestBase
    {
        private class SimpleClass
        {
        }

        private class SimpleBuilder : GenericBuilder<SimpleBuilder, SimpleClass>
        {
        }

        [Test]
        public void Create_ReturnsANewInstanceOfTheBuilder()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var b1 = SimpleBuilder.Create();
            var b2 = SimpleBuilder.Create();

            //---------------Test Result -----------------------
            Assert.IsNotNull(b1);
            Assert.IsNotNull(b2);
            Assert.IsInstanceOf<SimpleBuilder>(b1);
            Assert.IsInstanceOf<SimpleBuilder>(b2);
            Assert.AreNotEqual(b1, b2);
        }

        [Test]
        public void Build_ReturnsANewInstanceOfTheTargetClass()
        {
            //---------------Set up test pack-------------------
            var builder = SimpleBuilder.Create();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var simple1 = builder.Build();
            var simple2 = builder.Build();

            //---------------Test Result -----------------------
            Assert.IsNotNull(simple1);
            Assert.IsNotNull(simple2);
            Assert.IsInstanceOf<SimpleClass>(simple1);
            Assert.IsInstanceOf<SimpleClass>(simple2);
            Assert.AreNotEqual(simple1, simple2);
        }

        private class NotAsSimpleClass
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public bool Flag { get; set; }
            public DateTime Created { get; set; }
            public decimal Cost { get; set; }
        }

        private class NotAsSimpleBuilder : GenericBuilder<NotAsSimpleBuilder, NotAsSimpleClass>
        {
        }

        [Test]
        public void BuildDefault_ReturnsBlankObject()
        {
            //---------------Set up test pack-------------------
            var blank = new NotAsSimpleClass();
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var obj = NotAsSimpleBuilder.BuildDefault();

            //---------------Test Result -----------------------
            Assert.IsNotNull(obj);
            Assert.IsInstanceOf<NotAsSimpleClass>(obj);
            Assert.AreEqual(blank.Name, obj.Name);
            Assert.AreEqual(blank.Value, obj.Value);
            Assert.AreEqual(blank.Flag, obj.Flag);
            Assert.AreEqual(blank.Created, obj.Created);
            Assert.AreEqual(blank.Cost, obj.Cost);
        }

        public class VarianceAssert
        {
            public static void IsVariant<TObject, TProperty>(IEnumerable<TObject> collection, string propertyName)
            {
                var type = typeof(TObject);
                var propInfo = type.GetProperty(propertyName);
                if (propInfo == null)
                    throw new Exception(String.Join("", new[] { "Unable to find property '", propertyName, "' on type '", type.Name }));

                IEnumerable<TProperty> values = null;
                try
                {
                    values = collection.Select(obj => (TProperty)propInfo.GetValue(obj, null)).ToArray();
                }
                catch (Exception ex)
                {
                    Assert.Fail(String.Join("", new[] { "Unable to get list of property values for '", propertyName, "' of type '", typeof(TProperty).Name
                        , "' from object of type '", type.Name, "': ", ex.Message }));
                }
                var totalCount = values.Count();
                foreach (var value in values)
                {
                    if (values.Count(v => v.Equals(value)) == totalCount)
                    {
                        Assert.Fail(String.Join("", new[] { "No variance for property '", propertyName, "' across ", totalCount.ToString(), " samples" }));
                    }
                }
            }
        }

        [Test]
        public void WithRandomProps_SetsRandomValuesForAllProperties()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var randomItems = new List<NotAsSimpleClass>();
            for (var i = 0; i < RANDOM_TEST_CYCLES; i++)
            {
                randomItems.Add(NotAsSimpleBuilder.Create().WithRandomProps().Build());
            }

            //---------------Test Result -----------------------
            // look for variance
            VarianceAssert.IsVariant<NotAsSimpleClass, string>(randomItems, "Name");
            VarianceAssert.IsVariant<NotAsSimpleClass, int>(randomItems, "Value");
            VarianceAssert.IsVariant<NotAsSimpleClass, bool>(randomItems, "Flag");
            VarianceAssert.IsVariant<NotAsSimpleClass, DateTime>(randomItems, "Created");
            VarianceAssert.IsVariant<NotAsSimpleClass, decimal>(randomItems, "Cost");
        }

        private class BuilderInspector : GenericBuilder<BuilderInspector, SimpleClass>
        {
            public static string[] Calls
            {
                get
                {
                    return _calls.ToArray();
                }
            }
            private static List<string> _calls = new List<string>();
            public override BuilderInspector WithRandomProps()
            {
                _calls.Add("WithRandomProps");
                return base.WithRandomProps();
            }

            public override SimpleClass Build()
            {
                _calls.Add("Build");
                return base.Build();
            }
        }
        [Test]
        public void BuildRandom_CallsWithRandomPropsThenBuildAndReturnsResult()
        {
            //---------------Set up test pack-------------------
            Assert.AreEqual(0, BuilderInspector.Calls.Length);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            BuilderInspector.BuildRandom();

            //---------------Test Result -----------------------
            Assert.AreEqual(2, BuilderInspector.Calls.Length);
            Assert.AreEqual("WithRandomProps", BuilderInspector.Calls[0]);
            Assert.AreEqual("Build", BuilderInspector.Calls[1]);
        }

        public class ComplexMember1
        {
            public string Name { get; set; }
        }

        public class ComplexMember2
        {
            public int Value { get; set; }
        }

        public class ClassWithComplexMembers
        {
            public ComplexMember1 ComplexMember1 { get; set; }
            public ComplexMember2 ComplexMember2 { get; set; }
        }

        private class ClassWithComplexMembersBuilder : GenericBuilder<ClassWithComplexMembersBuilder, ClassWithComplexMembers>
        {
        }

        [Test]
        public void BuildDefault_SetsComplexMembersToNullValue()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var obj = ClassWithComplexMembersBuilder.BuildDefault();

            //---------------Test Result -----------------------
            Assert.IsNull(obj.ComplexMember1);
            Assert.IsNull(obj.ComplexMember2);
        }

        [Test]
        public void WithRandomProps_SetsRandomPropertiesForComplexMembersAndTheirProps()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var randomItems = new List<ClassWithComplexMembers>();
            for (var i = 0; i < RANDOM_TEST_CYCLES; i++)
            {
                var randomItem = ClassWithComplexMembersBuilder.BuildRandom();
                Assert.IsNotNull(randomItem.ComplexMember1);
                Assert.IsNotNull(randomItem.ComplexMember2);
                randomItems.Add(randomItem);
            }

            //---------------Test Result -----------------------
            VarianceAssert.IsVariant<ClassWithComplexMembers, ComplexMember1>(randomItems, "ComplexMember1");
            VarianceAssert.IsVariant<ClassWithComplexMembers, ComplexMember2>(randomItems, "ComplexMember2");
            var complexMembers1 = randomItems.Select(i => i.ComplexMember1);
            VarianceAssert.IsVariant<ComplexMember1, string>(complexMembers1, "Name");
            var complexMembers2 = randomItems.Select(i => i.ComplexMember2);
            VarianceAssert.IsVariant<ComplexMember2, int>(complexMembers2, "Value");
        }
    }
}
