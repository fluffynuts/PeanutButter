using System;
using System.Collections.Generic;
using System.Threading;
using PeanutButter.TempDb;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Entity
{
    internal static class SharedDatabaseLocator
    {
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        private static readonly Dictionary<string, ITempDB> _sharedDatabases
            = new Dictionary<string, ITempDB>();

        public static void Register(string name, ITempDB database)
        {
            if (_sharedDatabases.ContainsKey(name))
            {
                throw new SharedDatabaseAlreadyRegisteredException(name);
            }
            _sharedDatabases[name] = database;
        }

        public static ITempDB Find(string name)
        {
            using (new AutoLocker(_lock))
            {
                ITempDB result;
                _sharedDatabases.TryGetValue(name, out result);
                return result;
            }
        }

        public static void Cleanup()
        {
            using (new AutoLocker(_lock))
            {
                foreach (var kvp in _sharedDatabases)
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