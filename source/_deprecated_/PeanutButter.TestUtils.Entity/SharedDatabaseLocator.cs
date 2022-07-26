using System;
using System.Collections.Generic;
using System.Threading;
using PeanutButter.TempDb;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Entity
{
    internal static class SharedDatabaseLocator
    {
        private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1);

        private static readonly Dictionary<string, ITempDB> SharedDatabases
            = new Dictionary<string, ITempDB>();

        public static void Register(string name, ITempDB database)
        {
            if (SharedDatabases.ContainsKey(name))
            {
                throw new SharedDatabaseAlreadyRegisteredException(name);
            }
            SharedDatabases[name] = database;
        }

        public static ITempDB Find(string name)
        {
            using (new AutoLocker(Lock))
            {
                ITempDB result;
                SharedDatabases.TryGetValue(name, out result);
                return result;
            }
        }

        public static void Cleanup()
        {
            using (new AutoLocker(Lock))
            {
                foreach (var kvp in SharedDatabases)
                {
                    try
                    {
                        kvp.Value.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Unable to dispose of shared database \"{kvp.Key}\": {e.Message}");
                    }
                }
            }
        }
    }
}