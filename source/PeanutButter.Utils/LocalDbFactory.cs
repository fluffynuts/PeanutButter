using System.Data.SqlClient;

namespace PeanutButter.Utils
{
    public class LocalDbFactory: ILocalDbFactory
    {
        public string InstanceName { get; set; }
        private const string MasterConnectionString = @"Data Source=(localdb)\{0};Initial Catalog=master;Integrated Security=True";

        public LocalDbFactory()
        {
        }

        public LocalDbFactory(string instanceName)
        {
            InstanceName = instanceName;
        }

        public void CreateDatabase(string databaseName, string databaseFile)
        {
            using (var connection = new SqlConnection(GetMasterConnectionString()))
            {
                connection.Open();
                using (SqlCommand cmd = connection.CreateCommand())
                {

                    cmd.CommandText = $"CREATE DATABASE [{databaseName}] ON (NAME = N'[{databaseName}]', FILENAME = '{databaseFile}')";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = $"ALTER DATABASE [{databaseName}] SET TRUSTWORTHY ON";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public string GetMasterConnectionString()
        {
            return string.Format(MasterConnectionString, 
                InstanceName ?? new LocalDbInstanceEnumerator().FindHighestDefaultInstance());
        }

    }
}
