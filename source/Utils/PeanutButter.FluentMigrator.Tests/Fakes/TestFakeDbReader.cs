using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using PeanutButter.FluentMigrator.Fakes;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using NExpect.Interfaces;
using NExpect.MatcherLogic;
using static NExpect.Expectations;
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable PossibleNullReferenceException

namespace PeanutButter.FluentMigrator.Tests.Fakes
{
    [TestFixture]
    public class TestFakeDbReader
    {
        [Test]
        public void Type_ShouldImplement_IDataReader()
        {
            //--------------- Arrange -------------------
            var sut = typeof(FakeDbReader);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.ShouldImplement<IDataReader>();

            //--------------- Assert -----------------------
        }

        [Test]
        public void Dispose_ShouldNotChuck()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.Dispose()).Not.To.Throw();

            //--------------- Assert -----------------------
        }

        [Test]
        public void GetName_GivenColumnIndex_ShouldReturnEmptyString()
        {
            //--------------- Arrange -------------------
            var input = GetRandomInt();
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetName(input);

            //--------------- Assert -----------------------
            Expect(result).To.Be.Empty();
        }

        [Test]
        public void GetDataTypeName_GivenColumnIndex_ShouldReturnEmptyString()
        {
            //--------------- Arrange -------------------
            var input = GetRandomInt();
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetDataTypeName(input);

            //--------------- Assert -----------------------
            Expect(result).To.Be.Empty();
        }

        [Test]
        public void GetFieldType_GivenIndex_ShouldReturnObjectType()
        {
            //--------------- Arrange -------------------
            var input = GetRandomInt();
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetFieldType(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(typeof(object));
        }

        [Test]
        public void GetValue_GivenIndex_ShouldReturnDefaultObject()
        {
            //--------------- Arrange -------------------
            var input = GetRandomInt();
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetValue(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(default(object));
        }


        [Test]
        public void GetValues_GivenArrayOfValues_ShouldReturn0()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomCollection<object>().ToArray();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetValues(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(0);
        }

        [Test]
        public void GetOrdinal_GivenColumnName_ShouldReturnMinusOne()
        {
            //--------------- Arrange -------------------
            var input = GetRandomString();
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetOrdinal(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(-1);
        }

        [Test]
        public void GetBoolean_GivenIndex_ShouldReturnDefaultBoolean()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomInt();
            var expected = default(bool);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetBoolean(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }


        [Test]
        public void GetByte_GivenIndex_ShouldReturnDefaultByte()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomInt();
            var expected = default(byte);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetByte(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetChar_GivenIndex_ShouldReturnDefaultChar()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomInt();
            var expected = default(char);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetChar(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetGuid_GivenIndex_ShouldReturnDefaultGuid()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomInt();
            var expected = default(Guid);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetGuid(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetInt16_GivenIndex_ShouldReturnDefaultInt15()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomInt();
            var expected = default(Int16);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetInt16(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetInt32_GivenIndex_ShouldReturnDefaultInt15()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomInt();
            var expected = default(Int32);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetInt32(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetInt64_GivenIndex_ShouldReturnDefaultInt15()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomInt();
            var expected = default(Int64);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetInt64(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetFloat_GivenIndex_ShouldReturnDefaultInt15()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomInt();
            var expected = default(float);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetFloat(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetDouble_GivenIndex_ShouldReturnDefaultInt15()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomInt();
            var expected = default(double);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetDouble(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetString_GivenIndex_ShouldReturnDefaultInt15()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomInt();
            var expected = default(string);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetString(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetDecimal_GivenIndex_ShouldReturnDefaultInt15()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomInt();
            var expected = default(decimal);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetDecimal(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetDateTime_GivenIndex_ShouldReturnDefaultInt15()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var input = GetRandomInt();
            var expected = default(DateTime);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetDateTime(input);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetBytes_GivenIndexOffsetBufferBufferOffsetAndLength_ShouldReturnDefaultLong()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = default(long);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetBytes(GetRandomInt(), GetRandomInt(), new byte[0], GetRandomInt(), GetRandomInt());

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetChars_GivenIndexOffsetBufferBufferOffsetAndLength_ShouldReturnDefaultLong()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var expected = default(long);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetChars(GetRandomInt(), GetRandomInt(), new char[0], GetRandomInt(), GetRandomInt());

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void GetData_GivenIndex_ShouldReturnNewFakeDataReader()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var index = GetRandomInt();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = sut.GetData(index);
            var result2 = sut.GetData(index);

            //--------------- Assert -----------------------
            Expect(result1).Not.To.Be.Null();
            Expect(result2).Not.To.Be.Null();
            Expect(result1).To.Be.An.Instance.Of<FakeDbReader>();
            Expect(result2).To.Be.An.Instance.Of<FakeDbReader>();
            Expect(result1).Not.To.Equal(result2);
        }

        [Test]
        public void IsDbNull_GivenIndex_AlwaysReturnsTrue()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var index = GetRandomInt();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.IsDBNull(index);

            //--------------- Assert -----------------------
            Expect(result).To.Be.True();
        }

        [Test]
        public void FieldCount_IsZero()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.FieldCount;

            //--------------- Assert -----------------------
            Expect(result).To.Equal(0);
        }

        [Test]
        public void IndexedByInteger_ShouldReturnDefaultOfObject()
        {
            //--------------- Arrange -------------------
            var sut = Create() as IDataRecord;
            var index = GetRandomInt();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut[index];

            //--------------- Assert -----------------------
            Expect(result).To.Equal(default(object));
        }


        [Test]
        public void IndexedByString_ShouldReturnDefaultOfObject()
        {
            //--------------- Arrange -------------------
            var sut = Create() as IDataRecord;
            var index = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut[index];

            //--------------- Assert -----------------------
            Expect(result).To.Equal(default(object));
        }

        [Test]
        public void Close_ShouldSetIsClosed_True()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------
            Expect(sut.IsClosed).To.Be.False();

            //--------------- Act ----------------------
            sut.Close();

            //--------------- Assert -----------------------
            Expect(sut.IsClosed).To.Be.True();
        }

        [Test]
        public void GetSchemaTable_ShouldReturnNewEmptyDataTable()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = sut.GetSchemaTable();
            var result2 = sut.GetSchemaTable();

            //--------------- Assert -----------------------
            Expect(result1).Not.To.Be.Null();
            Expect(result1.Rows).To.Be.Empty();
            Expect(result2).Not.To.Be.Null();
            Expect(result2.Rows).To.Be.Empty();
            Expect(result1).Not.To.Equal(result2);
        }

        [Test]
        public void NextResult_AlwaysReturnsFalse()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.NextResult();

            //--------------- Assert -----------------------
            Expect(result).To.Be.False();
        }

        [Test]
        public void Read_AlwaysReturnsFalse()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.Read();

            //--------------- Assert -----------------------
            Expect(result).To.Be.False();
        }

        [Test]
        public void Depth_AlwaysReturnsZero()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.Depth;

            //--------------- Assert -----------------------
            Expect(result).To.Equal(0);
        }


        [Test]
        public void RecordsAffected_AlwaysReturnsZero()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.RecordsAffected;

            //--------------- Assert -----------------------
            Expect(result).To.Equal(0);
        }

        private FakeDbReader Create()
        {
            return new FakeDbReader();
        }
    }

    internal static class DataRowMatchers
    {
        public static void Empty(
            this IBe<DataRowCollection> be
        )
        {
            be.Compose(actual =>
            {
                Expect(actual.Count)
                    .To.Equal(
                        0,
                        $"Expected no rows, but found {actual.Count}"
                    );
            });
        }
    }
}