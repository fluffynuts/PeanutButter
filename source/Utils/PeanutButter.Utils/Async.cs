using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides methods to run async code synchronously and safely
    /// http://stackoverflow.com/questions/5095183/how-would-i-run-an-async-taskt-method-synchronously
    /// </summary>
    public static class Async
    {
        /// <summary>
        /// Executes an async Action synchronously
        /// - use when you really cannot use async/await
        /// - NEVER use Task.Wait() or Task.Result if the underlying code uses async/await
        /// </summary>
        /// <param name="task">Action to execute</param>
        public static void RunSync(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var context = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                context.Post(async _ =>
                {
                    try
                    {
                        await task();
                    }
                    catch (Exception e)
                    {
                        context.InnerException = e;
                        throw;
                    }
                    finally
                    {
                        context.EndMessageLoop();
                    }
                }, null);
                context.BeginMessageLoop();

                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is null)
                {
                    throw;
                }
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                // shouldn't actually get here
                throw;
            }
        }

        /// <summary>
        /// Executes an async Func&lt;T&gt; synchronously and returns the result
        /// - use when you really cannot use async/await
        /// - NEVER use Task.Wait() or Task.Result if the underlying code uses async/await
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task&lt;T&gt; method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var context = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);
            var ret = default(T);
            try
            {
                context.Post(async _ =>
                {
                    try
                    {
                        ret = await task();
                    }
                    catch (Exception e)
                    {
                        context.InnerException = e;
                        throw;
                    }
                    finally
                    {
                        context.EndMessageLoop();
                    }
                }, null);
                context.BeginMessageLoop();
                SynchronizationContext.SetSynchronizationContext(oldContext);
                return ret;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is null)
                {
                    throw;
                }
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                // shouldn't actually get here
                throw;
            }
        }
    }

    internal class ExclusiveSynchronizationContext : SynchronizationContext
    {
        private bool _done;
        public Exception InnerException { get; set; }
        private readonly AutoResetEvent _workItemsWaiting = new AutoResetEvent(false);

        private readonly Queue<Tuple<SendOrPostCallback, object>> _items =
            new Queue<Tuple<SendOrPostCallback, object>>();

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotSupportedException("We cannot send to our same thread");
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            lock (_items)
            {
                _items.Enqueue(Tuple.Create(d, state));
            }

            _workItemsWaiting.Set();
        }

        public void EndMessageLoop()
        {
            Post(_ => _done = true, null);
        }

        public void BeginMessageLoop()
        {
            while (!_done)
            {
                Tuple<SendOrPostCallback, object> task = null;
                lock (_items)
                {
                    if (_items.Count > 0)
                    {
                        task = _items.Dequeue();
                    }
                }

                if (task is null)
                {
                    _workItemsWaiting.WaitOne();
                    continue;
                }

                task.Item1(task.Item2);
                if (InnerException is not null)
                {
                    throw new AggregateException(
                        $"{nameof(Async)}.{nameof(Async.RunSync)}: provided async function threw an exception.",
                        InnerException
                    );
                }
            }
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}