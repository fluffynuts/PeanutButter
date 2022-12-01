using System;
using System.Linq;
using System.Reflection;
using MySqlConnector;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.Utils;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace PeanutButter.TempDb.MySql.Connector
{
    /// <summary>
    /// Provides the TempDB implementation for MySql, using
    /// MySql.Data as the connector library
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class TempDBMySql : TempDBMySqlBase<MySqlConnection>
    {
        /// <summary>
        /// Constructs a TempDbMySql
        /// </summary>
        public TempDBMySql()
        {
        }

        /// <summary>
        /// Construct a TempDbMySql with zero or more creation scripts and default options
        /// </summary>
        /// <param name="creationScripts"></param>
        public TempDBMySql(params string[] creationScripts)
            : base(new TempDbMySqlServerSettings(), creationScripts)
        {
        }

        /// <summary>
        /// Create a TempDbMySql instance with provided options and zero or more creation scripts
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="creationScripts"></param>
        public TempDBMySql(
            TempDbMySqlServerSettings settings,
            params string[] creationScripts
        )
            : base(
                settings,
                o =>
                {
                },
                creationScripts)
        {
        }

        /// <summary>
        /// Create a TempDbMySql instance with provided options, an action to run before initializing and
        /// zero or more creation scripts
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="beforeInit"></param>
        /// <param name="creationScripts"></param>
        public TempDBMySql(
            TempDbMySqlServerSettings settings,
            Action<object> beforeInit,
            params string[] creationScripts
        ) : base(
            settings,
            o => BeforeInit(o as TempDBMySqlBase<MySqlConnection>, beforeInit, settings),
            creationScripts
        )
        {
        }

        /// <summary>
        /// Generates the connection string to use for clients
        /// wishing to connect to this temp instance
        /// </summary>
        /// <returns></returns>
        protected override string GenerateConnectionString()
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Port = (uint) Port,
                UserID = "root",
                Password = RootPasswordSet
                    ? Settings.Options.RootUserPassword
                    : "",
                Server = "localhost",
                AllowUserVariables = true,
                SslMode = MySqlSslMode.None,
                Database = SchemaName,
                ConnectionTimeout = DefaultTimeout,
                DefaultCommandTimeout = DefaultTimeout,
                CharacterSet = Settings.CharacterSetServer
            };
            return builder.ToString();
        }

        protected override int FetchCurrentConnectionCount()
        {
            return MySqlPoolStatsFetcher.FetchSessionCountFor(
                ConnectionString
            );
        }
    }

    /// <summary>
    /// Fetches stats from the MySqlConnector pool using reflection
    /// </summary>
    public class MySqlPoolStatsFetcher
    {
        private static readonly Type[] MySqlConnectorTypes = typeof(MySqlConnection)
            .GetAssembly()
            .GetTypes();

        private static readonly Type ConnectionPoolType =
            MySqlConnectorTypes.Single(t => t.Name == "ConnectionPool");

        private static readonly BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        private static readonly FieldInfo LeasedSessionsField
            = ConnectionPoolType.GetFields(PrivateInstance)
                .Single(fi => fi.Name.ToLowerInvariant() == "m_leasedsessions");

        private static readonly BindingFlags PublicStatic =
            BindingFlags.Public | BindingFlags.Static;

        private static MethodInfo GetPoolMethod =
            ConnectionPoolType.GetMethods(PublicStatic)
                .Single(mi =>
                {
                    if (mi.Name.ToLowerInvariant() != "getpool")
                    {
                        return false;
                    }

                    var parameters = mi.GetParameters();
                    return parameters.Length == 1 &&
                        parameters[0].ParameterType == typeof(string);
                });

        public static int FetchSessionCountFor(string connectionString)
        {
            var pool = GetPoolMethod.Invoke(null, new object[] { connectionString });
            if (pool is null)
            {
                return 0;
            }

            var leasedSessionsValue = LeasedSessionsField.GetValue(pool);
            return (int) leasedSessionsValue.GetType()
                .GetProperty("Count")
                .GetValue(leasedSessionsValue);
        }
    }
}