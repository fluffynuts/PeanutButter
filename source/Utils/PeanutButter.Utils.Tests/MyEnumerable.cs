namespace PeanutButter.Utils.Tests
{
    public class MyEnumerable<T>
    {
        private readonly T[] _data;

        public MyEnumerable(T[] data)
        {
            _data = data;
        }

        public MyEnumerator<T> GetEnumerator()
        {
            return new MyEnumerator<T>(_data);
        }
    }

    public class MyEnumerator<T>
    {
        public T Current => _data[_index];

        private readonly T[] _data;
        private int _index;

        public MyEnumerator(T[] data)
        {
            _data = data;
            Reset();
        }

        public bool MoveNext()
        {
            _index++;
            return (_index < _data.Length);
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}