using System;
using System.Data;

namespace PeanutButter.FluentMigrator.Fakes
{
    internal class FakeDbReader: IDataReader
    {
        public void Dispose()
        {
            /* nothing to do */
        }

        public string GetName(int i)
        {
            return string.Empty;
        }

        public string GetDataTypeName(int i)
        {
            return string.Empty;
        }

        public Type GetFieldType(int i)
        {
            return typeof(object);
        }

        public object GetValue(int i)
        {
            return default(object);
        }

        public int GetValues(object[] values)
        {
            return 0;
        }

        public int GetOrdinal(string name)
        {
            return -1;
        }

        public bool GetBoolean(int i)
        {
            return default(bool);
        }

        public byte GetByte(int i)
        {
            return default(byte);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return default(long);
        }

        public char GetChar(int i)
        {
            return default(char);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return default(long);
        }

        public Guid GetGuid(int i)
        {
            return default(Guid);
        }

        public short GetInt16(int i)
        {
            return default(Int16);
        }

        public int GetInt32(int i)
        {
            return default(Int32);
        }

        public long GetInt64(int i)
        {
            return default(Int64);
        }

        public float GetFloat(int i)
        {
            return default(float);
        }

        public double GetDouble(int i)
        {
            return default(double);
        }

        public string GetString(int i)
        {
            return default(string);
        }

        public decimal GetDecimal(int i)
        {
            return default(decimal);
        }

        public DateTime GetDateTime(int i)
        {
            return default(DateTime);
        }

        public IDataReader GetData(int i)
        {
            return new FakeDbReader();
        }

        public bool IsDBNull(int i)
        {
            return true;
        }

        public int FieldCount => 0;

        object IDataRecord.this[int i] => default(object);
        object IDataRecord.this[string name] => default(object);

        public void Close()
        {
            IsClosed = true;
        }

        public DataTable GetSchemaTable()
        {
            return new DataTable();
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            return false;
        }

        public int Depth => 0;
        public bool IsClosed { get; private set; } = false;

        public int RecordsAffected => 0;
    }
}