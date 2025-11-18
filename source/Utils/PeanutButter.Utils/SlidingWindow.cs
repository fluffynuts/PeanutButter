using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils;
#else
    PeanutButter.Utils;
#endif

/// <summary>
/// Provides an easy mechanism for a sliding window of data
/// This collection is not thread-safe - you should add your
/// own thread-safety as appropriate.
/// </summary>
/// <typeparam name="T"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    interface ISlidingWindow<T> : IList<T>
{
    /// <summary>
    /// Returns the raw item at the provided index - only really
    /// useful for diagnostics
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    ISlidingWindowItem<T> ItemAt(int index);

    /// <summary>
    /// Provides the first sliding window item in the collection,
    /// if available, or null
    /// </summary>
    ISlidingWindowItem<T> First { get; }

    /// <summary>
    /// Provides the last sliding window item in the collection,
    /// if available, or null
    /// </summary>
    ISlidingWindowItem<T> Last { get; }

    /// <summary>
    /// The age of the oldest record in the collection
    /// </summary>
    TimeSpan MaxLifeSpan { get; }

    /// <summary>
    /// Trims out stale or over-allocated items. Most useful
    /// when the collection is created with a max time to live
    /// and you'd like to get the current snapshot.
    /// </summary>
    void Trim();
}

/// <inheritdoc />
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class SlidingWindow<T> : ISlidingWindow<T>
{
    /// <summary>
    /// The maximum amount of time that items are kept for
    /// </summary>
    public TimeSpan TimeToLive
    {
        get => _timeToLive;
        set
        {
            _timeToLive = value;
            _livesForEver = value == TimeSpan.MaxValue;
        }
    }

    private TimeSpan _timeToLive;

    private readonly List<SlidingWindowItem<T>> _items = new();
    private bool _livesForEver;

    /// <summary>
    /// The maximum number of items allowed in the window
    /// </summary>
    public int MaxItems { get; set; }

    /// <summary>
    /// Constructs the sliding window with a cap on maximum items
    /// </summary>
    /// <param name="maxItems"></param>
    public SlidingWindow(int maxItems) : this(maxItems, TimeSpan.MaxValue)
    {
    }

    /// <summary>
    /// Constructs the sliding window with a cap on lifespan for items
    /// </summary>
    /// <param name="timeToLive"></param>
    public SlidingWindow(TimeSpan timeToLive) : this(int.MaxValue, timeToLive)
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="maxItems"></param>
    /// <param name="timeToLive"></param>
    public SlidingWindow(int maxItems, TimeSpan timeToLive)
    {
        TimeToLive = timeToLive;
        MaxItems = maxItems;
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        lock (_items)
        {
            return TrimThenRunLocked(
                () => _items
                    .Select(o => o.Value)
                    .ToList()
                    // ReSharper disable once NotDisposedResourceIsReturned
                    .GetEnumerator()
            );
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public void Add(T item)
    {
        lock (_items)
        {
            _items.Add(new SlidingWindowItem<T>(item));
        }

        Trim();
    }

    /// <inheritdoc />
    public TimeSpan MaxLifeSpan => CalculateCurrentTimeSpan();

    private TimeSpan CalculateCurrentTimeSpan()
    {
        var now = DateTime.Now;
        var first = First?.Created ?? now;
        return now - first;
    }

    /// <inheritdoc />
    public void Trim()
    {
        lock (_items)
        {
            if (_items.Count > MaxItems)
            {
                _items.Shift();
            }

            if (_livesForEver)
            {
                return;
            }

            var staleTime = DateTime.Now - TimeToLive;
            while (_items.Count > 0 && _items[0].Created < staleTime)
            {
                _items.Shift();
            }
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        lock (_items)
        {
            _items.Clear();
        }
    }

    /// <inheritdoc />
    public bool Contains(T item)
    {
        return TrimThenRunLocked(() => _items.Any(v => v.HasValue(item)));
    }

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex)
    {
        T[] items;
        lock (_items)
        {
            items = _items.Select(o => o.Value)
                .ToArray();
        }

        items.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public bool Remove(T item)
    {
        lock (_items)
        {
            var first = _items.FirstOrDefault(o => o.Value.Equals(item));
            return first is not null &&
                _items.Remove(first);
        }
    }

    /// <inheritdoc />
    public int Count
    {
        get
        {
            lock (_items)
            {
                Trim();
                return _items.Count;
            }
        }
    }

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public int IndexOf(T item)
    {
        var result = 0;
        SlidingWindowItem<T>[] snapshot;
        lock (_items)
        {
            snapshot = _items.ToArray();
        }

        foreach (var o in snapshot)
        {
            if (o.HasValue(item))
            {
                return result;
            }

            result++;
        }

        return -1;
    }

    /// <inheritdoc />
    public void Insert(int index, T item)
    {
        var slidingWindowItem = new SlidingWindowItem<T>(item);

        lock (_items)
        {
            if (index == 0)
            {
                slidingWindowItem.Created = _items[0].Created;
            }
            else if (index < _items.Count)
            {
                var before = _items[index - 1].Created.Ticks;
                var after = _items[index].Created.Ticks;
                var average = (before + after) / 2;
                slidingWindowItem.Created = new DateTime(average);
            }

            _items.Insert(index, slidingWindowItem);
        }

        Trim();
    }

    /// <inheritdoc />
    public ISlidingWindowItem<T> ItemAt(int index)
    {
        lock (_items)
        {
            return _items[index];
        }
    }

    /// <inheritdoc />
    public ISlidingWindowItem<T> First => TrimThenRunLocked(() => _items.FirstOrDefault());

    private ISlidingWindowItem<T> TrimThenRunLocked(
        Func<SlidingWindowItem<T>> func
    )
    {
        Trim();
        lock (_items)
        {
            return func();
        }
    }

    private TOut TrimThenRunLocked<TOut>(
        Func<TOut> func
    )
    {
        Trim();
        lock (_items)
        {
            return func();
        }
    }

    /// <inheritdoc />
    public ISlidingWindowItem<T> Last => TrimThenRunLocked(() => _items.LastOrDefault());

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        lock (_items)
        {
            _items.RemoveAt(index);
        }
    }

    /// <inheritdoc />
    public T this[int index]
    {
        get
        {
            lock (_items)
            {
                return _items[index].Value;
            }
        }

        set
        {
            SlidingWindowItem<T> item;
            lock (_items)
            {
                item = _items[index];
            }

            lock (item)
            {
                item.Value = value;
            }
        }
    }
}