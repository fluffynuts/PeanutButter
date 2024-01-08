using PeanutButter.RandomGenerators;

namespace PeanutButter.Utils.NetCore.Tests
{
    [TestFixture]
    public class TestAssemblyExtensions
    {
        [Test]
        public void FindTypeByName_WhenGivenUnknownTypeName_ShouldReturnNull()
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

        [Test]
        public void FindTypeByName_WhenGivenKnownTypeName_ShouldReturnTheType()
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
