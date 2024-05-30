using System.Reflection;
using PeanutButter.RandomGenerators;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestAssemblyExtensions
    {
        [TestFixture]
        public class FindTypeByName
        {
            [TestFixture]
            public class GivenUnknownTypeName
            {
                [Test]
                public void ShouldReturnNull()
                {
                    //---------------Set up test pack-------------------
                    var asm = GetType().Assembly;
                    var search = RandomValueGen.GetRandomString(20, 30);
                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = asm.FindTypeByName(search);

                    //---------------Test Result -----------------------
                    Expect(result)
                        .To.Be.Null();
                }
            }

            [TestFixture]
            public class GivenKnownTypeName
            {
                [Test]
                public void ShouldReturnTheType()
                {
                    //---------------Set up test pack-------------------
                    var myType = GetType();
                    var asm = myType.Assembly;
                    var search = myType.Name;
                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = asm.FindTypeByName(search);

                    //---------------Test Result -----------------------
                    Expect(result)
                        .To.Equal(myType);
                }
            }
        }

        [TestFixture]
        public class WalkDependencies
        {
            [Test]
            public void ShouldReturnAssemblyDependenciesPerLevel()
            {
                // Arrange
                // Act
                var collected = new List<Assembly>();
                foreach (var assemblies in typeof(TestAssemblyExtensions).Assembly.WalkDependencies())
                {
                    collected.AddRange(assemblies);
                }

                // Assert
                var asmNames = collected.Select(o => o.FullName).ToArray();
                Expect(asmNames)
                    .To.Contain.At.Least(80).Items(
                        "At time of writing, this should be around 87..."
                    );
                Expect(asmNames)
                    .To.Be.Distinct();
            }
        }
    }
}