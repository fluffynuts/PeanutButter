using System.Data;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.FluentMigrator.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.FluentMigrator.Tests.Fakes
{
    [TestFixture]
    public class TestFakeDbConnectionFactory: AssertionHelper
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
            Expect(result1, Is.Not.Null);
            Expect(result1, Is.InstanceOf<FakeDbConnection>());
            Expect(result1.ConnectionString, Is.EqualTo(expected1));
            Expect(result2, Is.Not.Null);
            Expect(result2, Is.InstanceOf<FakeDbConnection>());
            Expect(result2.ConnectionString, Is.EqualTo(expected2));
            Expect(result1, Is.Not.EqualTo(result2));
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
            Expect(result1, Is.Not.Null);
            Expect(result1, Is.InstanceOf<FakeDbCommand>());
            Expect(result1.Connection, Is.EqualTo(connection1));
            Expect(result1.CommandText, Is.EqualTo(text1));
            Expect(result1.Transaction, Is.EqualTo(transaction1));
            Expect(result2, Is.Not.Null);
            Expect(result2, Is.InstanceOf<FakeDbCommand>());
            Expect(result2.Connection, Is.EqualTo(connection2));
            Expect(result2.CommandText, Is.EqualTo(text2));
            Expect(result2.Transaction, Is.EqualTo(transaction2));
            Expect(result1, Is.Not.EqualTo(result2));
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
            Expect(result1, Is.Not.Null);
            Expect(result1, Is.InstanceOf<FakeDbCommand>());
            Expect(result1.Connection, Is.EqualTo(connection1));
            Expect(result1.CommandText, Is.EqualTo(text1));
            Expect(result2, Is.Not.Null);
            Expect(result2, Is.InstanceOf<FakeDbCommand>());
            Expect(result2.Connection, Is.EqualTo(connection2));
            Expect(result2.CommandText, Is.EqualTo(text2));
            Expect(result1, Is.Not.EqualTo(result2));
            Expect(result1.Transaction, Is.Not.Null);
            Expect(result2.Transaction, Is.Not.Null);
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
            Expect(result1, Is.Not.Null);
            Expect(result1, Is.InstanceOf<FakeDbDataAdapter>());
            Expect(result1.DeleteCommand, Is.EqualTo(command1));
            Expect(result1.InsertCommand, Is.EqualTo(command1));
            Expect(result1.SelectCommand, Is.EqualTo(command1));
            Expect(result1.UpdateCommand, Is.EqualTo(command1));
            Expect(result2, Is.Not.Null);
            Expect(result2, Is.InstanceOf<FakeDbDataAdapter>());
            Expect(result2.DeleteCommand, Is.EqualTo(command2));
            Expect(result2.InsertCommand, Is.EqualTo(command2));
            Expect(result2.SelectCommand, Is.EqualTo(command2));
            Expect(result2.UpdateCommand, Is.EqualTo(command2));
            Expect(result1, Is.Not.EqualTo(result2));
        }


        private FakeDbConnectionFactory Create()
        {
            return new FakeDbConnectionFactory();
        }
    }
}