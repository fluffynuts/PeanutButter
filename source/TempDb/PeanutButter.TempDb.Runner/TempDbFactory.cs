using System;
using System.Collections.Generic;
using System.Linq;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.TempDb.MySql.Data;
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
            var result = new T();
            result.SetupAutoDispose(
                TimeSpanFromSeconds(opts.AbsoluteTimeoutSeconds),
                TimeSpanFromSeconds(opts.IdleTimeoutSeconds)
            );
            return result;
        }

        private static ITempDB GenerateTempDbMySql(Options opts)
        {
            var result = new TempDBMySql(
                new TempDbMySqlServerSettings()
                {
                    Options =
                    {
                        LogAction = opts.Verbose
                            ? new Action<string>(s => Console.WriteLine($"debug: {s}"))
                            : null,
                        InactivityTimeout = TimeSpanFromSeconds(opts.IdleTimeoutSeconds),
                        AbsoluteLifespan = TimeSpanFromSeconds(opts.AbsoluteTimeoutSeconds)
                    }
                }
            );
            return result;
        }

        private static TimeSpan? TimeSpanFromSeconds(int? seconds)
        {
            return seconds.HasValue
                ? TimeSpan.FromSeconds(seconds.Value)
                : null as TimeSpan?;
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