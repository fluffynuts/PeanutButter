using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.FluentMigrator.Fakes;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.FluentMigrator.Tests.Fakes
{
    [TestFixture]
    public class TestFakeDbConnection: AssertionHelper
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
            Expect(result, Is.InstanceOf<FakeDbTransaction>());
            Expect(result.IsolationLevel, Is.EqualTo(isolationLevel));
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
            Expect(result, Is.InstanceOf<FakeDbTransaction>());
            Expect(result.IsolationLevel, 
                    Is.EqualTo(IsolationLevel.ReadUncommitted));
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
            Expect(result, Is.EqualTo(ConnectionState.Closed));
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
            Expect(sut.State, Is.EqualTo(ConnectionState.Open));
        }

        [Test]
        public void Close_ShouldSetStateToClosed()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            sut.Open();

            //--------------- Assume ----------------
            Expect(sut.State, Is.EqualTo(ConnectionState.Open));

            //--------------- Act ----------------------
            sut.Close();

            //--------------- Assert -----------------------
            Expect(sut.State, Is.EqualTo(ConnectionState.Closed));
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
            Expect(result1, Is.Not.Null);
            Expect(result2, Is.Not.Null);
            Expect(result1, Is.Not.EqualTo(result2));
            Expect(result1, Is.InstanceOf<FakeDbCommand>());
            Expect(result2, Is.InstanceOf<FakeDbCommand>());
            Expect(result1.Connection, Is.EqualTo(sut));
            Expect(result2.Connection, Is.EqualTo(sut));
        }


        [Test]
        public void ChangeDatabase_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.ChangeDatabase(GetRandomString()),
                Throws.Nothing);

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
            Expect(sut.Database, Is.EqualTo(expected));
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
            Expect(result, Is.EqualTo(30));
        }


        private FakeDbConnection Create()
        {
            return new FakeDbConnection();
        }
    }
}
