using System.Data;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.FluentMigrator.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.FluentMigrator.Tests.Fakes
{
    [TestFixture]
    public class TestFakeDbConnectionFactory
    {
        [Test]
        public void CreateConnection_GivenConnectionString_ShouldReturnNewFakeConnectionWithThatConnectionString()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected1 = GetRandomString();
            var expected2 = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = sut.CreateConnection(expected1);
            var result2 = sut.CreateConnection(expected2);

            //--------------- Assert -----------------------
            Expect(result1).Not.To.Be.Null();
            Expect(result1).To.Be.An.Instance.Of<FakeDbConnection>();
            Expect(result1.ConnectionString).To.Equal(expected1);
            Expect(result2).Not.To.Be.Null();
            Expect(result2).To.Be.An.Instance.Of<FakeDbConnection>();
            Expect(result2.ConnectionString).To.Equal(expected2);
            Expect(result1).Not.To.Equal(result2);
        }

        [Test]
        public void CreateCommand_GivenCommandTextAndConnectionAndTransaction_ShouldReturnNewFakeCommand()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var connection1 = Substitute.For<IDbConnection>();
            var transaction1 = Substitute.For<IDbTransaction>();
            var text1 = GetRandomString();
            var connection2 = Substitute.For<IDbConnection>();
            var transaction2 = Substitute.For<IDbTransaction>();
            var text2 = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = sut.CreateCommand(text1, connection1, transaction1);
            var result2 = sut.CreateCommand(text2, connection2, transaction2);

            //--------------- Assert -----------------------
            Expect(result1).Not.To.Be.Null();
            Expect(result1).To.Be.An.Instance.Of<FakeDbCommand>();
            Expect(result1.Connection).To.Equal(connection1);
            Expect(result1.CommandText).To.Equal(text1);
            Expect(result1.Transaction).To.Equal(transaction1);

            Expect(result2).Not.To.Be.Null();
            Expect(result2).To.Be.An.Instance.Of<FakeDbCommand>();
            Expect(result2.Connection).To.Equal(connection2);
            Expect(result2.CommandText).To.Equal(text2);
            Expect(result2.Transaction).To.Equal(transaction2);
        }

        [Test]
        public void CreateCommand_GivenCommandTextAndConnection_ShouldReturnNewFakeCommand()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var connection1 = Substitute.For<IDbConnection>();
            var text1 = GetRandomString();
            var connection2 = Substitute.For<IDbConnection>();
            var text2 = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = sut.CreateCommand(text1, connection1);
            var result2 = sut.CreateCommand(text2, connection2);

            //--------------- Assert -----------------------
            Expect(result1).Not.To.Be.Null();
            Expect(result1).To.Be.An.Instance.Of<FakeDbCommand>();
            Expect(result1.Connection).To.Equal(connection1);
            Expect(result1.CommandText).To.Equal(text1);

            Expect(result2).Not.To.Be.Null();
            Expect(result2).To.Be.An.Instance.Of<FakeDbCommand>();
            Expect(result2.Connection).To.Equal(connection2);
            Expect(result2.CommandText).To.Equal(text2);

            Expect(result1).Not.To.Equal(result2);
            Expect(result1.Transaction).Not.To.Be.Null();
            Expect(result2.Transaction).Not.To.Be.Null();
        }

        [Test]
        public void CreateDataAdapter_GivenCommand_ShouldReturnNewFakeDataAdapter()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var command1 = Substitute.For<IDbCommand>();
            var command2 = Substitute.For<IDbCommand>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = sut.CreateDataAdapter(command1);
            var result2 = sut.CreateDataAdapter(command2);

            //--------------- Assert -----------------------
            Expect(result1).Not.To.Be.Null();
            Expect(result1).To.Be.An.Instance.Of<FakeDbDataAdapter>();
            Expect(result1.DeleteCommand).To.Equal(command1);
            Expect(result1.InsertCommand).To.Equal(command1);
            Expect(result1.SelectCommand).To.Equal(command1);
            Expect(result1.UpdateCommand).To.Equal(command1);

            Expect(result2).Not.To.Be.Null();
            Expect(result2).To.Be.An.Instance.Of<FakeDbDataAdapter>();
            Expect(result2.DeleteCommand).To.Equal(command2);
            Expect(result2.InsertCommand).To.Equal(command2);
            Expect(result2.SelectCommand).To.Equal(command2);
            Expect(result2.UpdateCommand).To.Equal(command2);

            Expect(result1).Not.To.Equal(result2);
        }


        private FakeDbConnectionFactory Create()
        {
            return new FakeDbConnectionFactory();
        }
    }
}