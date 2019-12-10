using System;
using Microsoft.Win32;

namespace PeanutButter.WindowsServiceManagement
{
    public class RegKey : IDisposable
    {
        public RegistryKey Key { get; private set; }
        public bool Exists => Key != null;

        public RegKey(string path)
        {
            try
            {
                Key = Registry.LocalMachine.OpenSubKey(
                    path,
                    RegistryKeyPermissionCheck.ReadWriteSubTree
                );
            }
            catch
            {
                /* intentionally left blank */
            }
        }

        public void Dispose()
        {
            Key?.Dispose();
            Key = null;
        }

        public void SetValue(
            string start,
            ServiceStartupTypes value,
            RegistryValueKind dWord)
        {
            Key?.SetValue(start, value, dWord);
        }

        public object GetValue(string start)
        {
            return Key?.GetValue(start);
        }
    }
}