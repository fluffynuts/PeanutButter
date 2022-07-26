using System.Data;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.FluentMigrator.Fakes;
using PeanutButter.TestUtils.Generic;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.FluentMigrator.Tests.Fakes
{
    [TestFixture]
    public class TestFakeDbCommand
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
            Expect(() => sut.Dispose()).Not.To.Throw();

            //--------------- Assert -----------------------
        }
        [Test]
        public void Prepare_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.Prepare()).Not.To.Throw();

            //--------------- Assert -----------------------
        }
        [Test]
        public void Cancel_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.Cancel()).Not.To.Throw();

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
            Expect(result1).Not.To.Be.Null();
            Expect(result2).Not.To.Be.Null();
            Expect(result1).To.Be.An.Instance.Of<FakeDbParameter>();
            Expect(result2).To.Be.An.Instance.Of<FakeDbParameter>();
            Expect(result1).Not.To.Equal(result2);
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
            Expect(sut.Connection).To.Equal(expected);
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
            Expect(sut.Transaction).To.Equal(expected);
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
            Expect(sut.CommandText).To.Equal(expected);
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
            Expect(sut.CommandTimeout).To.Equal(expected);
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
            Expect(sut.CommandType).To.Equal(expected);
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
            Expect(sut.UpdatedRowSource).To.Equal(expected);
        }





        private FakeDbCommand Create()
        {
            return new FakeDbCommand();
        }
    }
}
