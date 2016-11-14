using System.Data;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.FluentMigrator.Fakes;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.FluentMigrator.Tests.Fakes
{
    [TestFixture]
    public class TestFakeDbCommand: AssertionHelper
    {
        [Test]
        public void Type_ShouldImplement_IDbCommand()
        {
            //--------------- Arrange -------------------
            var sut = typeof(FakeDbCommand);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.ShouldImplement<IDbCommand>();

            //--------------- Assert -----------------------
        }

        [Test]
        public void Dispose_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.Dispose(), Throws.Nothing);

            //--------------- Assert -----------------------
        }
        [Test]
        public void Prepare_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.Prepare(), Throws.Nothing);

            //--------------- Assert -----------------------
        }
        [Test]
        public void Cancel_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.Cancel(), Throws.Nothing);

            //--------------- Assert -----------------------
        }

        [Test]
        public void CreateParameter_ShouldReturnNewFakeDbParameter()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = sut.CreateParameter();
            var result2 = sut.CreateParameter();

            //--------------- Assert -----------------------
            Expect(result1, Is.Not.Null);
            Expect(result2, Is.Not.Null);
            Expect(result1, Is.InstanceOf<FakeDbParameter>());
            Expect(result2, Is.InstanceOf<FakeDbParameter>());
            Expect(result1, Is.Not.EqualTo(result2));
        }

        [Test]
        public void Connection_CanSetAndGet()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = Substitute.For<IDbConnection>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Connection = expected;

            //--------------- Assert -----------------------
            Expect(sut.Connection, Is.EqualTo(expected));
        }

        [Test]
        public void Transaction_CanSetAndGet()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = Substitute.For<IDbTransaction>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Transaction = expected;

            //--------------- Assert -----------------------
            Expect(sut.Transaction, Is.EqualTo(expected));
        }

        [Test]
        public void CommandText_CanSetAndGet()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.CommandText = expected;

            //--------------- Assert -----------------------
            Expect(sut.CommandText, Is.EqualTo(expected));
        }

        [Test]
        public void CommandTimeout_CanSetAndGet()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandomInt();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.CommandTimeout = expected;

            //--------------- Assert -----------------------
            Expect(sut.CommandTimeout, Is.EqualTo(expected));
        }

        [Test]
        public void CommandType_CanSetAndGet()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandomEnum<CommandType>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.CommandType = expected;

            //--------------- Assert -----------------------
            Expect(sut.CommandType, Is.EqualTo(expected));
        }

        [Test]
        public void UpdateRowSource_CanSetAndGet()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandomEnum<UpdateRowSource>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.UpdatedRowSource = expected;

            //--------------- Assert -----------------------
            Expect(sut.UpdatedRowSource, Is.EqualTo(expected));
        }





        private FakeDbCommand Create()
        {
            return new FakeDbCommand();
        }
    }
}
