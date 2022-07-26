using System.Data;
using NUnit.Framework;
using PeanutButter.FluentMigrator.Fakes;
using PeanutButter.TestUtils.Generic;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.FluentMigrator.Tests.Fakes
{
    [TestFixture]
    public class TestFakeDbConnection
    {
        [Test]
        public void Type_ShouldImplement_IDbConnection()
        {
            //--------------- Arrange -------------------
            var sut = typeof(FakeDbConnection);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.ShouldImplement<IDbConnection>();

            //--------------- Assert -----------------------
        }

        [Test]
        public void BeginTransaction_GivenIsolationLevel_ShouldReturnNewFakeTransaction()
        {
            //--------------- Arrange -------------------
            var isolationLevel = GetRandom<IsolationLevel>();
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.BeginTransaction(isolationLevel);

            //--------------- Assert -----------------------
            Expect(result).To.Be.An.Instance.Of<FakeDbTransaction>();
            Expect(result.IsolationLevel).To.Equal(isolationLevel);
        }

        [Test]
        public void BeginTransaction_GivenNoIsolationLevel_ShouldReturnNewFakeTransaction()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.BeginTransaction();

            //--------------- Assert -----------------------
            Expect(result).To.Be.An.Instance.Of<FakeDbTransaction>();
            Expect(result.IsolationLevel)
                .To.Equal(IsolationLevel.ReadUncommitted);
        }

        [Test]
        public void Construct_State_ShouldBeClosed()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.State;

            //--------------- Assert -----------------------
            Expect(result).To.Equal(ConnectionState.Closed);
        }


        [Test]
        public void Open_ShouldSetStateToOpen()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Open();

            //--------------- Assert -----------------------
            Expect(sut.State).To.Equal(ConnectionState.Open);
        }

        [Test]
        public void Close_ShouldSetStateToClosed()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            sut.Open();

            //--------------- Assume ----------------
            Expect(sut.State).To.Equal(ConnectionState.Open);

            //--------------- Act ----------------------
            sut.Close();

            //--------------- Assert -----------------------
            Expect(sut.State).To.Equal(ConnectionState.Closed);
        }

        [Test]
        public void CreadCommand_ShouldCreateNewCommand()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = sut.CreateCommand();
            var result2 = sut.CreateCommand();

            //--------------- Assert -----------------------
            Expect(result1).Not.To.Be.Null();
            Expect(result2).Not.To.Be.Null();
            Expect(result1).Not.To.Equal(result2);
            Expect(result1).To.Be.An.Instance.Of<FakeDbCommand>();
            Expect(result2).To.Be.An.Instance.Of<FakeDbCommand>();
            Expect(result1.Connection).To.Equal(sut);
            Expect(result2.Connection).To.Equal(sut);
        }


        [Test]
        public void ChangeDatabase_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.ChangeDatabase(GetRandomString()))
                .Not.To.Throw();

            //--------------- Assert -----------------------
        }

        [Test]
        public void Database_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Database = expected;

            //--------------- Assert -----------------------
            Expect(sut.Database).To.Equal(expected);
        }

        [Test]
        public void Construct_ShouldSetConnectionTimeoutTo_30()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.ConnectionTimeout;

            //--------------- Assert -----------------------
            Expect(result).To.Equal(30);
        }


        private FakeDbConnection Create()
        {
            return new FakeDbConnection();
        }
    }
}
