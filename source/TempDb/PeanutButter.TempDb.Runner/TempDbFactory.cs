using System;
using System.Collections.Generic;
using System.Linq;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.TempDb.MySql.Connector;
using PeanutButter.TempDb.Sqlite;

namespace PeanutButter.TempDb.Runner
{
    public static class TempDbFactory
    {
        public static string[] AvailableEngines => Factories.Keys.ToArray();

        private static readonly Dictionary<string, Func<ITempDB>> Factories
            = new Dictionary<string, Func<ITempDB>>(StringComparer.OrdinalIgnoreCase)
            {
                ["MySql"] = Generate<TempDBMySql>,
                ["LocalDb"] = Generate<TempDBLocalDb>,
                ["SQLite"] = Generate<TempDBSqlite>
            };

        private static ITempDB Generate<T>()
            where T: ITempDB, new()
        {
            return new T();
        }


        public static ITempDB Create(Options opts)
        {
            if (Factories.TryGetValue(opts.Engine, out var factory))
            {
                return factory();
            }

            throw new InvalidOperationException(
                $"TempDb factory not available for engine: {opts.Engine}"
            );
        }
    }
}