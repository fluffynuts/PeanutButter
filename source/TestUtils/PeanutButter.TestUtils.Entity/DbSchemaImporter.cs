using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using PeanutButter.FluentMigrator;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Entity
{
    public class DbSchemaImporter : IDBMigrationsRunner
    {
        private readonly string _connectionString;
        private readonly string _schemaSql;

        public DbSchemaImporter(string connectionString, string schemaSql)
        {
            _connectionString = connectionString;
            _schemaSql = schemaSql;
        }

        public void MigrateToLatest()
        {
            using (var cmd = CreateConnection().CreateCommand())
            {
                var cleaned = CleanCommentsFrom(_schemaSql);
                var parts = SplitPartsOutOf(cleaned);
                parts.ForEach(part =>
                {
                    if (string.IsNullOrEmpty(part))
                        return;
                    cmd.CommandText = part;
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

        public string[] SplitPartsOutOf(string schemaSql)
        {
            return schemaSql
                        .Split(new[] {"GO"}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToArray();
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