using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeanutButter.Utils
{
    public class AutoDisposer: IDisposable
    {
        private List<IDisposable> _toDispose;

        public AutoDisposer(params IDisposable[] toDispose)
        {
            _toDispose = new List<IDisposable>();
            Add(toDispose);
        }

        public void Add(params IDisposable[] toDispose)
        {
            _toDispose.AddRange(toDispose);
        }

        public T Add<T>(T toDispose) where T : IDisposable
        {
            _toDispose.Add(toDispose);
            return toDispose;
        }

        public void Dispose()
        {
            lock (this)
            {
                _toDispose.Reverse();
                foreach (var disposable in _toDispose)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch { }
                }
                _toDispose.Clear();
            }
        }
    }
}
