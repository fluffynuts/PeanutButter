using System;
using System.Threading.Tasks;

namespace PeanutButter.Utils;

/// <summary>
/// Provides a mechanism for rate-limiting calls to blocks
/// of code. Calls made within limits are executed, those
/// which exceed limits are dropped
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// The configured max-calls-per-minute, which can be modified at runtime
    /// </summary>
    public int MaxCallsPerPeriod { get; set; }

    /// <summary>
    /// The configured period to rate limit over (read-only, for information only)
    /// </summary>
    public TimeSpan Period { get; }

    /// <summary>
    /// Reports the number of calls which have been successfully run
    /// during the last sliding window period
    /// </summary>
    public int CallsRunWithinLastPeriod { get; }

    /// <summary>
    /// Runs the synchronous code, rate-limited
    /// - calls which come in within the sliding minute
    ///   are performed until the configured rate per
    ///   minute is reached, and extra calls are discarded
    /// </summary>
    /// <param name="action"></param>
    void Run(Action action);

    /// <summary>
    /// Runs the asynchronous code, rate-limited
    /// - calls which come in within the sliding minute
    ///   are performed until the configured rate per
    ///   minute is reached, and extra calls are discarded
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    Task RunAsync(Func<Task> action);

    /// <summary>
    /// Resets the sliding window of known calls
    /// </summary>
    void Reset();
}

/// <inheritdoc />
public class RateLimiter : IRateLimiter
{
    private readonly SlidingWindow<bool> _record;

    /// <inheritdoc />
    public int MaxCallsPerPeriod { get; set; }

    /// <inheritdoc />
    public TimeSpan Period
    {
        get
        {
            lock (_record)
            {
                return _record.TimeToLive;
            }
        }
    }

    /// <inheritdoc />
    public int CallsRunWithinLastPeriod
    {
        get
        {
            lock (_record)
            {
                return _record.Count;
            }
        }
    }

    /// <summary>
    /// Constructs the rate limiter with the provided maxCallsPerMinute
    /// </summary>
    /// <param name="maxCallsPerMinute"></param>
    public RateLimiter(int maxCallsPerMinute)
        : this(maxCallsPerMinute, TimeSpan.FromMinutes(1))
    {
    }

    /// <summary>
    /// Configures the rate limiter with the provided max calls
    /// per provided period
    /// </summary>
    /// <param name="maxCallsPerPeriod"></param>
    /// <param name="period"></param>
    public RateLimiter(
        int maxCallsPerPeriod,
        TimeSpan period
    )
    {
        MaxCallsPerPeriod = maxCallsPerPeriod;
        _record = new SlidingWindow<bool>(period);
    }

    /// <inheritdoc />
    public void Run(
        Action action
    )
    {
        if (CanRunNow())
        {
            action();
        }
    }

    private bool CanRunNow()
    {
        lock (_record)
        {
            if (_record.Count >= MaxCallsPerPeriod)
            {
                return false;
            }

            // the actual value doesn't really matter
            // -> we're just leaning on the functionality
            //    of the sliding window
            _record.Add(true);
            return true;
        }
    }

    /// <inheritdoc />
    public async Task RunAsync(
        Func<Task> action
    )
    {
        if (CanRunNow())
        {
            await action();
        }
    }

    /// <inheritdoc />
    public void Reset()
    {
        lock (_record)
        {
            _record.Clear();
        }
    }
}