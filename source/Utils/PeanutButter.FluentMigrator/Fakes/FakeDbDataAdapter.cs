using System.Data;

namespace PeanutButter.FluentMigrator.Fakes
{
    public class FakeDbDataAdapter: IDbDataAdapter
    {
        public DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType)
        {
            return new DataTable[0];
        }

        public int Fill(DataSet dataSet)
        {
            return 0;
        }

        public IDataParameter[] GetFillParameters()
        {
            return new IDataParameter[0];
        }

        public int Update(DataSet dataSet)
        {
            return 0;
        }

        public MissingMappingAction MissingMappingAction { get; set; }
        public MissingSchemaAction MissingSchemaAction { get; set; }
        public ITableMappingCollection TableMappings { get; }
        public IDbCommand SelectCommand { get; set; }
        public IDbCommand InsertCommand { get; set; }
        public IDbCommand UpdateCommand { get; set; }
        public IDbCommand DeleteCommand { get; set; }
    }
}