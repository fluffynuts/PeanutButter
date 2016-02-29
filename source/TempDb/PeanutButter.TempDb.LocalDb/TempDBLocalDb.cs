using System;
using System.Data.SqlClient;
using System.IO;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.LocalDb
{
    //<summary>
    //  LocalDb implementation of TempDb. TempDb will destroy the temporary database on disposal. However,
    //      it is feasible that the Dispose method may not be called (eg aborting a test you're debugging),
    //      so you can end up with orphaned tempdb databases registered against your localdb instance. Unlike
    //      sqlite and SqlCe, where the file is the be-all and end-all of the database, localdb has a registration
    //      mechanism, so deleting the file (ie, clearing out temp files) is not enough. The procedure below can
    //      be run into your localdb master database and run at any time to purge orphaned tempdb_* databases
    //-- procedure to run if you have orphaned localdb temp databases
    //create procedure clear_temp_databases as
    //begin
    //    declare @tmp table (cmd nvarchar(1024));
    //    insert into @tmp (cmd) select 'drop database ' + name from sys.databases where name like 'tempdb_%';
    //    declare @cmd nvarchar(1024);
    //    while exists (select * from @tmp)
    //    begin
    //        select top 1 @cmd = cmd from @tmp;
    //        delete from @tmp where cmd = @cmd;
    //        exec(@cmd);
    //    end;
    //end;
    //</summary>

    public class TempDBLocalDb: TempDB<SqlConnection>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public string DatabaseName { get { return _databaseName; } set { _databaseName = value; } }
        private string _databaseName;
        // ReSharper disable once MemberCanBePrivate.Global
        private string _instanceName;
        public string InstanceName
        {
            get { return _instanceName ?? new LocalDbInstanceEnumerator().FindHighestDefaultInstance(); }
            set { _instanceName = value; }
        }
        private const string MasterConnectionString = @"Data Source=(localdb)\{0};Initial Catalog=master;Integrated Security=True";

        public TempDBLocalDb()
        {
        }

        public TempDBLocalDb(params string[] creationScripts): base(creationScripts)
        {
        }


        public TempDBLocalDb(string dbName, string instanceName = null, params string[] creationScripts)
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
            _databaseName = _databaseName ?? "tempdb_" + CreateGuidString();
            using (var connection = new SqlConnection(GenerateMasterConnectionString()))
            {
                connection.Open();
                using (SqlCommand cmd = connection.CreateCommand())
                {

                    cmd.CommandText = $"CREATE DATABASE [{_databaseName}] ON (NAME = N'[{_databaseName}]', FILENAME = '{DatabaseFile}')";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = $"ALTER DATABASE [{_databaseName}] SET TRUSTWORTHY ON";
                    //ConnectionString = $@"Data Source=(localdb)\{InstanceName};AttachDbFilename={DatabaseFile}; Initial Catalog={_databaseName};Integrated Security=True";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string GenerateMasterConnectionString()
        {
            return String.Format(MasterConnectionString, InstanceName);
        }

        private static string CreateGuidString()
        {
            return Guid.NewGuid()
                .ToString()
                .Replace("-", string.Empty)
                .Replace("{", string.Empty)
                .Replace("}", string.Empty);
        }

        protected override string GenerateConnectionString()
        {
            return $@"Data Source=(localdb)\{InstanceName};AttachDbFilename={DatabaseFile}; Initial Catalog={_databaseName};Integrated Security=True";
        }


        protected override void DeleteTemporaryDatabaseFile()
        {
            using (var connection = new SqlConnection(GenerateMasterConnectionString()))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = string.Format("alter database [{0}] set SINGLE_USER WITH ROLLBACK IMMEDIATE;", _databaseName);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = string.Format("drop database [{0}]", _databaseName);
                    cmd.ExecuteNonQuery();
                }
            }
            File.Delete(DatabaseFile);
        }
    }
}