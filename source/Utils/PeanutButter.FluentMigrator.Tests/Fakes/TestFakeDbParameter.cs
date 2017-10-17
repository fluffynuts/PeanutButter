using System.Data;
using NUnit.Framework;
using PeanutButter.FluentMigrator.Fakes;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.FluentMigrator.Tests.Fakes
{
    [TestFixture]
    public class TestFakeDbParameter
    {
        [Test]
        public void Type_ShouldImplement_IDbDataParameter()
        {
            //--------------- Arrange -------------------
            var sut = typeof(FakeDbParameter);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.ShouldImplement<IDbDataParameter>();

            //--------------- Assert -----------------------
        }

        [Test]
        public void DbType_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandom<DbType>(); 

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.DbType = expected;

            //--------------- Assert -----------------------
            Expect(sut.DbType).To.Equal((expected));
        }

        [Test]
        public void Direction_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandom<ParameterDirection>(); 

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Direction = expected;

            //--------------- Assert -----------------------
            Expect(sut.Direction).To.Equal((expected));
        }

        [Test]
        public void ParameterName_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandomString(); 

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.ParameterName = expected;

            //--------------- Assert -----------------------
            Expect(sut.ParameterName).To.Equal((expected));
        }

        [Test]
        public void SourceColumn_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandomString(); 

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.SourceColumn = expected;

            //--------------- Assert -----------------------
            Expect(sut.SourceColumn).To.Equal((expected));
        }

        [Test]
        public void SourceVersionShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandom<DataRowVersion>(); 

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.SourceVersion= expected;

            //--------------- Assert -----------------------
            Expect(sut.SourceVersion).To.Equal((expected));
        }

        [Test]
        public void ValueShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Value = expected;

            //--------------- Assert -----------------------
            Expect(sut.Value).To.Equal((expected));
        }

        [Test]
        public void PrecisionShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandom<byte>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Precision = expected;

            //--------------- Assert -----------------------
            Expect(sut.Precision).To.Equal((expected));
        }

        [Test]
        public void ScaleShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandom<byte>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Scale = expected;

            //--------------- Assert -----------------------
            Expect(sut.Scale).To.Equal((expected));
        }

        [Test]
        public void SizeShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = GetRandomInt();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Size = expected;

            //--------------- Assert -----------------------
            Expect(sut.Size).To.Equal((expected));
        }


        private FakeDbParameter Create()
        {
            return new FakeDbParameter();
        }
    }
}