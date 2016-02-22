using System.Data.SqlClient;

namespace PeanutButter.Utils
{
    public class LocalDbFactory: ILocalDbFactory
    {
        public string InstanceName { get; set; } = "v11.0";
        private const string MasterConnectionString = @"Data Source=(localdb)\{0};Initial Catalog=master;Integrated Security=True";

        public LocalDbFactory()
        {
        }

        public LocalDbFactory(string instanceName)
        {
            InstanceName = instanceName;
        }

        public void CreateDatabase(string dbName, string dbFile)
        {
            using (var connection = new SqlConnection(GetMasterConnectionString()))
            {
                connection.Open();
                using (SqlCommand cmd = connection.CreateCommand())
                {

                    cmd.CommandText = $"CREATE DATABASE [{dbName}] ON (NAME = N'[{dbName}]', FILENAME = '{dbFile}')";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = $"ALTER DATABASE [{dbName}] SET TRUSTWORTHY ON";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string GetMasterConnectionString()
        {
            return string.Format(MasterConnectionString, InstanceName);
        }

    }
}
