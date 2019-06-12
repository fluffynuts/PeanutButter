using System;
using System.Linq;

namespace PeanutButter.TempDb.Tests
{
    public class MySqlConnectionStringUtil
    {
        public string Database { get; }

        public MySqlConnectionStringUtil(
            string connectionString)
        {
            Database = connectionString
                .Split(';')
                .Select(p => p.Trim())
                .FirstOrDefault(p => p.StartsWith("DATABASE", StringComparison.OrdinalIgnoreCase))
                ?.Split('=')
                ?.Last();
        }
    }
}