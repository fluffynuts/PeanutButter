using System;
using System.Data.SqlClient;
using System.IO;

namespace PeanutButter.TempDb.LocalDb
{
    public class TempDBLocalDb: TempDB<SqlConnection>
    {
        public string DatabaseName { get { return _dbName; } set { _dbName = value; } }
        private string _dbName;
        public string InstanceName = "v11.0";
        private const string _masterConnectionString = @"Data Source=(localdb)\{0};Initial Catalog=master;Integrated Security=True";

        public TempDBLocalDb()
        {
        }

        public TempDBLocalDb(params string[] creationScripts): base(creationScripts)
        {
        }


        public TempDBLocalDb(string dbName, string instanceName = "v11.0", params string[] creationScripts)
            : base(db => SetupInstance(db as TempDBLocalDb, dbName, instanceName), creationScripts)
        {
        }

        private static void SetupInstance(TempDBLocalDb db, string dbName, string instanceName)
        {
            db.InstanceName = instanceName;
            db.DatabaseName = dbName;
        }

        protected override void CreateDatabase()
        {
            _dbName = _dbName ?? "tempdb_" + Guid.NewGuid().ToString().Replace("-", "").Replace("{", "").Replace("}", "");
            using (var connection = new SqlConnection(GetMasterConnectionString()))
            {
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();
    
                cmd.CommandText = string.Format("CREATE DATABASE [{0}] ON (NAME = N'[{0}]', FILENAME = '{1}')", 
                    _dbName,
                    DatabaseFile);
                cmd.ExecuteNonQuery();
                cmd.CommandText = string.Format("ALTER DATABASE [{0}] SET TRUSTWORTHY ON", _dbName);
                ConnectionString = string.Format(@"Data Source=(localdb)\{0};AttachDbFilename={1}; Initial Catalog={2};Integrated Security=True", 
                    InstanceName, DatabaseFile, _dbName);
            }
        }

        private string GetMasterConnectionString()
        {
            return String.Format(_masterConnectionString, InstanceName);
        }


        protected override void DeleteTemporaryDatabaseFile()
        {
            using (var connection = new SqlConnection(GetMasterConnectionString()))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = String.Format("alter database [{0}] set SINGLE_USER WITH ROLLBACK IMMEDIATE;", _dbName);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = String.Format("drop database [{0}]", _dbName);
                    cmd.ExecuteNonQuery();
                }
            }
            File.Delete(DatabaseFile);
        }
    }
}