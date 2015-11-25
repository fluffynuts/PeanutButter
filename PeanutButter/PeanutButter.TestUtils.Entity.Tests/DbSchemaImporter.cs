using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using PeanutButter.FluentMigrator;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Entity.Tests
{
    public class DbSchemaImporter : IDBMigrationsRunner
    {
        private string _connectionString;

        public DbSchemaImporter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void MigrateToLatest()
        {
            using (var cmd = CreateConnection().CreateCommand())
            {
                var parts = TestResources.dbscript.Split(new[] {"GO"}, StringSplitOptions.RemoveEmptyEntries);
                parts.ForEach(part =>
                {
                    var sql = CleanCommentsFrom(part);
                    if (string.IsNullOrEmpty(sql))
                        return;
                    cmd.CommandText = sql;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to run part of the schema import:\n" + part + "\n" + ex.Message);
                    }
                });
            }
        }

        public string CleanCommentsFrom(string input)
        {
            var lines = input.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
            var inComment = 0;
            var cleaned = lines.Select(l => l.Trim())
                .Where(l => !l.StartsWith("--"))
                .Aggregate(new List<string>(), (acc, cur) =>
                {
                    if (cur.StartsWith("/*"))
                        inComment++;
                    if (inComment == 0)
                        acc.Add(cur);
                    if (cur.EndsWith("*/"))
                        inComment--;
                    return acc;
                });
            return string.Join("\r\n", cleaned);
        }

        private DbConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}