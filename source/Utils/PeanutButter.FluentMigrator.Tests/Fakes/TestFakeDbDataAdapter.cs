using System.Data;
using NUnit.Framework;
using PeanutButter.FluentMigrator.Fakes;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.FluentMigrator.Tests.Fakes
{
    [TestFixture]
    public class TestFakeDbDataAdapter: AssertionHelper
    {
        [Test]
        public void Type_ShouldImplement_IDbDataAdapter()
        {
            //--------------- Arrange -------------------
            var sut = typeof(FakeDbDataAdapter);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.ShouldImplement<IDbDataAdapter>();

            //--------------- Assert -----------------------
        }

        [Test]
        public void MissingMappingAction_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandom<MissingMappingAction>();
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.MissingMappingAction = expected;

            //--------------- Assert -----------------------
            Expect(sut.MissingMappingAction, Is.EqualTo(expected));
        }

        [Test]
        public void MissingSchemaAction_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandom<MissingSchemaAction>();
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.MissingSchemaAction = expected;

            //--------------- Assert -----------------------
            Expect(sut.MissingSchemaAction, Is.EqualTo(expected));
        }

        [Test]
        public void SelectCommand_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandom<IDbCommand>();
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.SelectCommand = expected;

            //--------------- Assert -----------------------
            Expect(sut.SelectCommand, Is.EqualTo(expected));
        }

        [Test]
        public void InsertCommand_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandom<IDbCommand>();
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.InsertCommand = expected;

            //--------------- Assert -----------------------
            Expect(sut.InsertCommand, Is.EqualTo(expected));
        }

        [Test]
        public void UpdateCommand_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandom<IDbCommand>();
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.UpdateCommand = expected;

            //--------------- Assert -----------------------
            Expect(sut.UpdateCommand, Is.EqualTo(expected));
        }

        [Test]
        public void DeleteCommand_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandom<IDbCommand>();
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.DeleteCommand = expected;

            //--------------- Assert -----------------------
            Expect(sut.DeleteCommand, Is.EqualTo(expected));
        }


        [Test]
        public void FillSchema_GivenDataSetAndSchemaType_ShouldReturnEmptyDataTableArray()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var dataSet = GetRandom<DataSet>();
            var schemaType = GetRandom<SchemaType>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.FillSchema(dataSet, schemaType);

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result, Is.Empty);
        }

        [Test]
        public void Fill_GivenDataSet_ShouldReturn0()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var dataset = GetRandom<DataSet>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.Fill(dataset);

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(0));
        }

        [Test]
        public void GetFillParameters_ShouldReturnEmptyCOllection()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetFillParameters();

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(result, Is.Empty);
        }

        [Test]
        public void Update_GivenDataSet_ShouldReturn0()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var dataset = GetRandom<DataSet>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.Update(dataset);

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(0));
        }

        private FakeDbDataAdapter Create()
        {
            return new FakeDbDataAdapter();
        }
    }
}