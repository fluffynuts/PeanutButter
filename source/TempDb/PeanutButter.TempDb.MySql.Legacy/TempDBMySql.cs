using System;
using MySql.Data.MySqlClient;
using PeanutButter.TempDb.MySql.Base;

namespace PeanutButter.TempDb.MySql.Legacy
{
    /// <summary>
    /// Legacy redirection to newer PeanutButter.TempDb.MySql.Data.TempDbMySql
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class TempDBMySql: PeanutButter.TempDb.MySql.Data.TempDBMySql
    {
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
                Port = (uint) _port,
                UserID = "root",
                Password = "",
                Server = "localhost",
                AllowUserVariables = true,
                SslMode = MySqlSslMode.None,
                Database = _schema,
                ConnectionTimeout = DefaultTimeout,
                DefaultCommandTimeout = DefaultTimeout
            };
            return builder.ToString();
        }
    }
}