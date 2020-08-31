using System;
using System.Linq;
using System.Reflection;
using MySql.Data.MySqlClient;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.MySql.Data
{
    /// <summary>
    /// fetches MySql connection pool stats via reflection from
    /// non-public types and fields as the MySql driver doesn't
    /// report any actually useful stats to WMI, making this document
    /// not all that useful for MySql (even when adjusting counter names):
    /// https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/performance-counters
    /// </summary>
    public class MySqlPoolStatsFetcher
    {
        private static readonly Type[] MySqlDataTypes = typeof(MySqlConnection)
            .GetAssembly()
            .GetTypes();

        private static readonly Type PoolManagerType =
            MySqlDataTypes.Single(t => t.Name == "MySqlPoolManager");

        private static readonly Type PoolType =
            MySqlDataTypes.Single(t => t.Name == "MySqlPool");

        private static readonly BindingFlags PublicStatic =
            BindingFlags.Public | BindingFlags.Static;

        private static readonly MethodInfo GetPoolMethod
            = PoolManagerType.GetMethods(PublicStatic)
                .Single(mi => mi.Name == "GetPool");

        private static readonly BindingFlags PrivateInstance =
            BindingFlags.NonPublic | BindingFlags.Instance;

        private static FieldInfo[] PoolFields = PoolType.GetFields(PrivateInstance);

        private static readonly FieldInfo MaxSizeField =
            PoolFields.Single(fi => fi.Name.ToLower().Contains("maxsize"));

        private static readonly FieldInfo AvailableField =
            PoolFields.Single(fi => fi.Name.ToLower().Contains("available"));

        private static readonly FieldInfo InUseField =
            PoolFields.Single(fi => fi.Name.ToLower().Contains("inusepool"));

        private static readonly FieldInfo IdleField =
            PoolFields.Single(fi => fi.Name.ToLower().Contains("idlepool"));

        /// <summary>
        /// Provides pool stats for MySql.Data Connection Pools via reflection
        /// </summary>
        public class PoolStats
        {
            /// <summary>
            /// Max connections
            /// </summary>
            public int MaxConnections { get; }
            /// <summary>
            /// Total available connections
            /// </summary>
            public int TotalAvailable { get; }
            /// <summary>
            /// Total connections in use
            /// </summary>
            public int TotalInUse { get; }
            /// <summary>
            /// Total idle connections
            /// </summary>
            public int TotalIdle { get; }

            /// <summary>
            /// Constructs an instance of the stats
            /// </summary>
            /// <param name="maxConnections"></param>
            /// <param name="totalAvailable"></param>
            /// <param name="totalInUse"></param>
            /// <param name="totalIdle"></param>
            public PoolStats(
                int maxConnections,
                int totalAvailable,
                int totalInUse,
                int totalIdle)
            {
                MaxConnections = maxConnections;
                TotalAvailable = totalAvailable;
                TotalInUse = totalInUse;
                TotalIdle = totalIdle;
            }
        }

        /// <summary>
        /// Retrieves connection pool stats via reflection
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public PoolStats GetPoolStatsViaReflection(string connectionString)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString);
            var pool = GetPoolMethod.Invoke(null, new object[] { builder });

            var maxConnections = (uint) MaxSizeField.GetValue(pool);
            var totalAvailable = (int) AvailableField.GetValue(pool);
            var inUsePool = (System.Collections.ICollection) InUseField.GetValue(pool);
            var idlePool = (System.Collections.ICollection) IdleField.GetValue(pool);
            return new PoolStats((int)maxConnections, totalAvailable, inUsePool.Count, idlePool.Count);
        }
    }
}