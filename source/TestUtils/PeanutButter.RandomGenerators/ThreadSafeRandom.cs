using System;

internal class ThreadSafeRandom
{
    private readonly Random _random = new(DateTime.Now.Millisecond);

    public double NextDouble()
    {
        lock (_random)
        {
            return _random.NextDouble();
        }
    }

    public int Next(int min, int max)
    {
        lock (_random)
        {
            return _random.Next(min, max);
        }
    }

    public void NextBytes(byte[] bytes)
    {
        lock (_random)
        {
            _random.NextBytes(bytes);
        }
    }
}