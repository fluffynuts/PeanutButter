using System;
using System.Data.SqlClient;
using System.IO;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TempDb.LocalDb
{
    //<summary>
    //  LocalDb implementation of TempDB. TempDB will destroy the temporary database on disposal. However,
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

    public class TempDBLocalDb : TempDB<SqlConnection>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public string DatabaseName { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        private string _instanceName;

        public string InstanceName
        {
            get => _instanceName ?? new LocalDbInstanceEnumerator().FindFirstAvailableInstance();
            set => _instanceName = value;
        }

        private const string MasterConnectionString =
            @"Data Source=(localdb)\{0};Initial Catalog=master;Integrated Security=True";

        public TempDBLocalDb()
        {
        }

        public TempDBLocalDb(params string[] creationScripts) : base(creationScripts)
        {
        }


        public TempDBLocalDb(string dbName, string instanceName = null, params string[] creationScripts)
            : base(db => SetupInstance(db as TempDBLocalDb, dbName, instanceName), creationScripts)
        {
        }

        private static void SetupInstance(TempDBLocalDb db, string dbName, string instanceName)
        {
            db.InstanceName = instanceName ?? db.InstanceName;
            db.DatabaseName = dbName ?? db.DatabaseName;
        }

        protected override void CreateDatabase()
        {
            DatabaseName = DatabaseName ?? "tempdb_" + CreateGuidString();
            using (var connection = new SqlConnection(GenerateMasterConnectionString()))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error opening connection to {connection.ConnectionString}: {ex.Message}");
                    throw;
                }

                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        $"CREATE DATABASE [{DatabaseName}] ON (NAME = N'[{DatabaseName}]', FILENAME = '{DatabasePath}')";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = $"ALTER DATABASE [{DatabaseName}] SET TRUSTWORTHY ON";
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

        public override string DumpSchema()
        {
            throw new NotImplementedException(
                "DumpSchema not yet implemented for LocalDb. Open a Pull Request!"
            );
        }

        protected override int FetchCurrentConnectionCount()
        {
            throw new NotImplementedException(
                "Inactivity monitoring for LocalDb not yet implemented"
            );
        }

        protected override string GenerateConnectionString()
        {
            var builder = new SqlConnectionStringBuilder()
            {
                DataSource = $"(localdb)\\{InstanceName}",
                AttachDBFilename = DatabasePath,
                InitialCatalog = DatabaseName,
                ConnectTimeout = (int) DefaultTimeout,
            };
            return builder.ToString();
        }


        protected override void DeleteTemporaryDataArtifacts()
        {
            using (var connection = new SqlConnection(GenerateMasterConnectionString()))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"alter database [{DatabaseName}] set SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = $"drop database [{DatabaseName}]";
                    cmd.ExecuteNonQuery();
                }
            }

            File.Delete(DatabasePath);
        }
    }
}