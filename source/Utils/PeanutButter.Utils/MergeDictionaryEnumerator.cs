using System.Collections;
using System.Collections.Generic;

namespace PeanutButter.Utils
{
    public class MergeDictionaryEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly IDictionary<TKey, TValue>[] _layers;
        private int _currentIndex;
        private IEnumerator<KeyValuePair<TKey, TValue>> _currentEnumerator;
        private readonly HashSet<TKey> _seen = new HashSet<TKey>();

        public MergeDictionaryEnumerator(IDictionary<TKey, TValue>[] layers)
        {
            _layers = layers;
            Reset();
        }

        public bool MoveNext()
        {
            do
            {
                if (MoveCurrentNext())
                    return true;
            } while (SelectNext());
            return false;
        }

        private bool SelectNext()
        {
            if (_currentIndex == _layers.Length - 1)
            {
                return false;
            }
            Select(_currentIndex + 1);
            return true;
        }

        private bool MoveCurrentNext()
        {
            var moved = _currentEnumerator.MoveNext();
            while (moved &&
                   _seen.Contains(_currentEnumerator.Current.Key))
            {
                moved = _currentEnumerator.MoveNext();
            }
            if (moved)
            {
                _seen.Add(_currentEnumerator.Current.Key);
            }
            return moved;
        }

        public void Reset()
        {
            _seen.Clear();
            Select(0);
        }

        object IEnumerator.Current => Current;

        private void Select(int index)
        {
            _currentEnumerator?.Dispose();
            _currentIndex = index;
            _currentEnumerator = _layers[_currentIndex].GetEnumerator();
        }

        public KeyValuePair<TKey, TValue> Current => _currentEnumerator.Current;


        public void Dispose()
        {
            /* nothing to do */
        }
    }
}