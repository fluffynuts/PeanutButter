using System;
using System.Collections.Generic;
using System.Linq;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.TempDb.MySql.Connector;
using PeanutButter.TempDb.Sqlite;

namespace PeanutButter.TempDb.Runner
{
    public static class TempDbFactory
    {
        public static string[] AvailableEngines => Factories.Keys.ToArray();

        private static readonly Dictionary<string, Func<Options, ITempDB>> Factories
            = new Dictionary<string, Func<Options, ITempDB>>(StringComparer.OrdinalIgnoreCase)
            {
                ["MySql"] = GenerateTempDbMySql,
                ["LocalDb"] = Generate<TempDBLocalDb>,
                ["SQLite"] = Generate<TempDBSqlite>
            };

        private static ITempDB Generate<T>(Options opts)
            where T : ITempDB, new()
        {
            return new T();
        }

        private static ITempDB GenerateTempDbMySql(Options opts)
        {
            var result = new TempDBMySql(
                new TempDbMySqlServerSettings()
                {
                    Options =
                    {
                        LogAction = opts.Verbose 
                            ? Console.WriteLine 
                            : null as Action<string>
                    }
                }
            );
            return result;
        }

        public static ITempDB Create(Options opts)
        {
            if (Factories.TryGetValue(opts.Engine, out var factory))
            {
                return factory(opts);
            }

            throw new InvalidOperationException(
                $"TempDb factory not available for engine: {opts.Engine}"
            );
        }
    }
}