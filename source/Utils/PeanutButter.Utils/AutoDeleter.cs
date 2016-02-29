using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PeanutButter.Utils
{
    public class AutoDeleter: IDisposable
    {
        private List<string> _toDelete;

        public AutoDeleter(params string[] paths)
        {
            _toDelete = new List<string>();
            Add(paths);
        }

        public void Add(params string[] paths)
        {
            _toDelete.AddRange(paths);
        }

        public void Dispose()
        {
            lock (this)
            {
                foreach (var f in _toDelete)
                {
                    try
                    {
                        if (Directory.Exists(f))
                            Directory.Delete(f, true);
                        else
                            File.Delete(f);
                    }
                    catch { }
                }
                _toDelete.Clear();
            }
        }
    }
}
