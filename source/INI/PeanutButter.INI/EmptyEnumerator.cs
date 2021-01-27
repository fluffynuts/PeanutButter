using System.Collections;
using System.Collections.Generic;

namespace PeanutButter.INI
{
    internal class EmptyEnumerator<T>
        : IEnumerator<T>
    {
        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
        }

        public T Current => default(T);

        object IEnumerator.Current => Current;
    }
}