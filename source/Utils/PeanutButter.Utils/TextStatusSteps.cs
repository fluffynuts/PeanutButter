using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.Utils
{
    /// <summary>
    /// Describes the contract for textural feedback around
    /// running activities.
    /// </summary>
    public interface ITextStatusSteps
    {
        /// <summary>
        /// Run the provided activity with the given label
        /// </summary>
        /// <param name="label"></param>
        /// <param name="activity"></param>
        void Run(
            string label,
            Action activity
        );

        /// <summary>
        /// Run the provided async activity with the given label
        /// </summary>
        /// <param name="label"></param>
        /// <param name="activity"></param>
        Task RunAsync(
            string label,
            Func<Task> activity
        );
        
        /// <summary>
        /// Simply log within the context of the steps (ie with the
        /// same io &amp; prefixing)
        /// </summary>
        /// <param name="str"></param>
        void Log(string str);
        
        /// <summary>
        /// Simply log within the context of the steps (ie with the
        /// same io &amp; prefixing)
        /// </summary>
        /// <param name="str"></param>
        Task LogAsync(string str);
    }

    /// <summary>
    /// Provides an easy mechanism for steps of a process with
    /// status feedback, ie "starting X..." / {does X} / {"completed" / "failed"}
    /// </summary>
    public class TextStatusSteps
        : ITextStatusSteps
    {
        private readonly Func<string, Task> _asyncWriter;
        private readonly Func<Task> _asyncFlushAction;
        private readonly Func<Exception, Task<ErrorHandlerResult>> _asyncExceptionHandler;
        private readonly Func<string> _prefixAllStatusLines;
        private readonly string _startMarker;
        private readonly string _completedMarker;
        private readonly string _failedMarker;
        private readonly Action<string> _writer;
        private readonly Action _flushAction;
        private readonly Func<Exception, ErrorHandlerResult> _exceptionHandler;
        private int _lastWriteLength;
        private readonly Steps _steps;


        /// <summary>
        /// The default label applied to activities that complete successfully
        /// eg "[ OK ] Fetching the stuff"
        /// </summary>
        public const string DEFAULT_OK_LABEL = "[ OK ]";

        /// <summary>
        /// The default label applied to activities that fail
        /// eg "[FAIL] Fetching the stuff"
        /// </summary>
        public const string DEFAULT_FAIL_LABEL = "[FAIL]";

        /// <summary>
        /// The default label applied when starting your activity
        /// eg "[ -- ] Fetching the stuff
        /// </summary>
        public const string DEFAULT_START_LABEL = "[ -- ]";

        /// <summary>
        /// Create the default status steps with no per-activity
        /// prefix and the pass/fail indicators [ OK ] and [FAIL]
        /// </summary>
        public TextStatusSteps()
            : this("", DEFAULT_START_LABEL, DEFAULT_OK_LABEL, DEFAULT_FAIL_LABEL)
        {
        }

        /// <summary>
        /// Create the status steps with:
        /// - a default prefix (eg "Starting")
        /// - a completed marker (eg "[ OK ]" or "✔️")
        /// - a failed marker (eg "[FAIL]" or "❌")
        /// </summary>
        /// <param name="prefixAllStatusLines"></param>
        /// <param name="startMarker"></param>
        /// <param name="completedMarker"></param>
        /// <param name="failedMarker"></param>
        public TextStatusSteps(
            string prefixAllStatusLines,
            string startMarker,
            string completedMarker,
            string failedMarker
        ) : this(
            prefixAllStatusLines,
            startMarker,
            completedMarker,
            failedMarker,
            Console.Out.Write
        )
        {
        }

        /// <summary>
        /// Create the status steps with:
        /// - a default prefix (eg "Starting")
        /// - a completed marker (eg "[ OK ]" or "✔️")
        /// - a failed marker (eg "[FAIL]" or "❌")
        /// - a callback to write status information
        /// </summary>
        /// <param name="prefixAllStatusLines"></param>
        /// <param name="startMarker"></param>
        /// <param name="completedMarker"></param>
        /// <param name="failedMarker"></param>
        /// <param name="writer"></param>
        public TextStatusSteps(
            string prefixAllStatusLines,
            string startMarker,
            string completedMarker,
            string failedMarker,
            Action<string> writer
        ) : this(
            prefixAllStatusLines,
            startMarker,
            completedMarker,
            failedMarker,
            writer,
            null
        )
        {
        }

        /// <summary>
        /// Create the status steps with:
        /// - a default prefix (eg "Starting")
        /// - a completed marker (eg "[ OK ]" or "✔️")
        /// - a failed marker (eg "[FAIL]" or "❌")
        /// - a callback to write status information
        /// - a callback to flush status information when appropriate
        ///   - this is only necessary if your writer callback buffers
        /// </summary>
        /// <param name="prefixAllStatusLines"></param>
        /// <param name="startMarker"></param>
        /// <param name="completedMarker"></param>
        /// <param name="failedMarker"></param>
        /// <param name="writer"></param>
        /// <param name="flushAction"></param>
        public TextStatusSteps(
            string prefixAllStatusLines,
            string startMarker,
            string completedMarker,
            string failedMarker,
            Action<string> writer,
            Action flushAction
        ) : this(
            prefixAllStatusLines,
            startMarker,
            completedMarker,
            failedMarker,
            writer,
            flushAction,
            null
        )
        {
        }

        /// <summary>
        /// Create the status steps with:
        /// - a default prefix (eg "Starting")
        /// - a completed marker (eg "[ OK ]" or "✔️")
        /// - a failed marker (eg "[FAIL]" or "❌")
        /// - a callback to write status information
        /// - a callback to flush status information when appropriate
        ///   - this is only necessary if your writer callback buffers
        /// </summary>
        /// <param name="prefixAllStatusLines">Prefix all status lines with this (after the start/ok/fail marker)</param>
        /// <param name="startMarker">Marker/placeholder when activity starts</param>
        /// <param name="completedMarker">Marker for a completed activity</param>
        /// <param name="failedMarker">Marker for a failed activity</param>
        /// <param name="writer">Action to call to write a message</param>
        /// <param name="flushAction">Action to call to flush any buffered messages</param>
        /// <param name="exceptionHandler">
        /// (optional) - if provided, if an error is thrown by the activity,
        /// this is invoked. If this returns true, the exception will be rethrown,
        /// otherwise it will be suppressed</param>
        public TextStatusSteps(
            string prefixAllStatusLines,
            string startMarker,
            string completedMarker,
            string failedMarker,
            Action<string> writer,
            Action flushAction,
            Func<Exception, ErrorHandlerResult> exceptionHandler
        ) : this(
            () => prefixAllStatusLines,
            startMarker,
            completedMarker,
            failedMarker,
            writer,
            flushAction,
            exceptionHandler
        )
        {
        }

        /// <summary>
        /// Create the status steps with:
        /// - a default prefix (eg "Starting")
        /// - a completed marker (eg "[ OK ]" or "✔️")
        /// - a failed marker (eg "[FAIL]" or "❌")
        /// - a callback to write status information
        /// - a callback to flush status information when appropriate
        ///   - this is only necessary if your writer callback buffers
        /// </summary>
        /// <param name="prefixAllStatusLines">Prefix all status lines with this (after the start/ok/fail marker)</param>
        /// <param name="startMarker">Marker/placeholder when activity starts</param>
        /// <param name="completedMarker">Marker for a completed activity</param>
        /// <param name="failedMarker">Marker for a failed activity</param>
        /// <param name="writer">Action to call to write a message</param>
        /// <param name="flushAction">Action to call to flush any buffered messages</param>
        /// <param name="exceptionHandler">
        /// (optional) - if provided, if an error is thrown by the activity,
        /// this is invoked. If this returns true, the exception will be rethrown,
        /// otherwise it will be suppressed</param>
        public TextStatusSteps(
            Func<string> prefixAllStatusLines,
            string startMarker,
            string completedMarker,
            string failedMarker,
            Action<string> writer,
            Action flushAction,
            Func<Exception, ErrorHandlerResult> exceptionHandler
        ) : this(prefixAllStatusLines, startMarker, completedMarker, failedMarker)
        {
            _writer = writer;
            _flushAction = flushAction;
            _exceptionHandler = exceptionHandler;
            _steps = new Steps();
        }

        private TextStatusSteps(
            Func<string> prefixAllStatusLines,
            string startMarker,
            string completedMarker,
            string failedMarker
        )
        {
            _prefixAllStatusLines = prefixAllStatusLines;
            _completedMarker = completedMarker;
            _failedMarker = failedMarker;

            var longestMarkerLength = new[]
            {
                startMarker.Length,
                _completedMarker.Length,
                _failedMarker.Length
            }.Max();
            _startMarker = startMarker.Length == longestMarkerLength
                ? startMarker
                // ReSharper disable once BuiltInTypeReferenceStyle
                : $"{startMarker}{new String(' ', longestMarkerLength - startMarker.Length)}";
        }

        /// <summary>
        /// Create the status steps with:
        /// - a default prefix (eg "Starting")
        /// - a completed marker (eg "[ OK ]" or "✔️")
        /// - a failed marker (eg "[FAIL]" or "❌")
        /// - a callback to write status information
        /// - a callback to flush status information when appropriate
        ///   - this is only necessary if your writer callback buffers
        /// </summary>
        /// <param name="prefixAllStatusLines">Prefix all status lines with this (after the start/ok/fail marker)</param>
        /// <param name="startMarker">Marker/placeholder when activity starts</param>
        /// <param name="completedMarker">Marker for a completed activity</param>
        /// <param name="failedMarker">Marker for a failed activity</param>
        /// <param name="asyncWriter">Action to call to write a message</param>
        /// <param name="asyncFlushAction">Action to call to flush any buffered messages</param>
        /// <param name="asyncExceptionHandler">
        /// (optional) - if provided, if an error is thrown by the activity,
        /// this is invoked. If this returns true, the exception will be rethrown,
        /// otherwise it will be suppressed</param>
        public TextStatusSteps(
            Func<string> prefixAllStatusLines,
            string startMarker,
            string completedMarker,
            string failedMarker,
            Func<string, Task> asyncWriter,
            Func<Task> asyncFlushAction,
            Func<Exception, Task<ErrorHandlerResult>> asyncExceptionHandler
        ) : this(prefixAllStatusLines, startMarker, completedMarker, failedMarker)
        {
            _asyncWriter = asyncWriter;
            _asyncFlushAction = asyncFlushAction;
            _asyncExceptionHandler = asyncExceptionHandler;
        }

        private readonly SemaphoreSlim _ioLock = new(1, 1);

        /// <inheritdoc />
        public void Run(
            string label,
            Action activity
        )
        {
            _steps.Run(
                () => Start(label),
                activity,
                ex =>
                {
                    if (ex is null)
                    {
                        Ok(label);
                        return ErrorHandlerResult.NoError;
                    }

                    Fail(label);
                    return InvokeExceptionHandlerOrFail(ex);
                }
            );
        }

        /// <inheritdoc />
        public async Task RunAsync(
            string label,
            Func<Task> activity
        )
        {
            await _steps.RunAsync(
                () => StartAsync(label),
                activity,
                async ex =>
                {
                    if (ex is null)
                    {
                        await OkAsync(label);
                        return ErrorHandlerResult.NoError;
                    }

                    await FailAsync(label);
                    return await InvokeExceptionHandlerOrFailAsync(ex);
                }
            );
        }

        /// <inheritdoc />
        public void Log(string str)
        {
            using var _ = new AutoLocker(_ioLock);
            InvokeWriter($"{_prefixAllStatusLines()}{str}");
            InvokeFlush();
        }

        /// <inheritdoc />
        public async Task LogAsync(string str)
        {
            using var _ = new AutoLocker(_ioLock);
            await InvokeWriterAsync($"{_prefixAllStatusLines()}{str}");
            await InvokeFlushAsync();
        }

        private void Start(string label)
        {
            RunWithIoLocking(
                () =>
                {
                    var toWrite = $"{_startMarker} {_prefixAllStatusLines?.Invoke()}{label}";
                    _lastWriteLength = toWrite.Length;
                    InvokeWriter(toWrite);
                    InvokeFlush();
                }
            );
        }

        private async Task StartAsync(string label)
        {
            await RunWithIoLockingAsync(
                async () =>
                {
                    var toWrite = $"{_startMarker} {_prefixAllStatusLines?.Invoke()}{label}";
                    _lastWriteLength = toWrite.Length;
                    await InvokeWriterAsync(toWrite);
                    await InvokeFlushAsync();
                }
            );
        }

        private void Ok(string label)
        {
            RunWithIoLocking(
                () =>
                {
                    ClearLastLine();
                    InvokeWriter($"{_completedMarker} {_prefixAllStatusLines?.Invoke()}{label}\n");
                    InvokeFlush();
                }
            );
        }

        private async Task OkAsync(string label)
        {
            await RunWithIoLockingAsync(
                async () =>
                {
                    await ClearLastLineAsync();
                    await InvokeWriterAsync($"{_completedMarker} {_prefixAllStatusLines?.Invoke()}{label}\n");
                    await InvokeFlushAsync();
                }
            );
        }

        private void Fail(string label)
        {
            RunWithIoLocking(
                () =>
                {
                    ClearLastLine();
                    InvokeWriter($"{_failedMarker} {_prefixAllStatusLines?.Invoke()}{label}\n");
                    InvokeFlush();
                }
            );
        }

        private async Task FailAsync(string label)
        {
            await RunWithIoLockingAsync(
                async () =>
                {
                    await ClearLastLineAsync();
                    await InvokeWriterAsync($"{_failedMarker} {_prefixAllStatusLines?.Invoke()}{label}\n");
                    await InvokeFlushAsync();
                }
            );
        }

        private ErrorHandlerResult InvokeExceptionHandlerOrFail(Exception ex)
        {
            return RunWithIoLocking(
                () =>
                {
                    if (_exceptionHandler is not null)
                    {
                        return _exceptionHandler.Invoke(ex);
                    }

                    if (_asyncExceptionHandler is not null)
                    {
                        return Async.RunSync(() => _asyncExceptionHandler.Invoke(ex));
                    }

                    return ErrorHandlerResult.Rethrow;
                }
            );
        }

        private async Task<ErrorHandlerResult> InvokeExceptionHandlerOrFailAsync(Exception ex)
        {
            return await RunWithIoLockingAsync(
                async () =>
                {
                    if (_exceptionHandler is not null)
                    {
                        return _exceptionHandler.Invoke(ex);
                    }

                    if (_asyncExceptionHandler is not null)
                    {
                        return await _asyncExceptionHandler.Invoke(ex);
                    }

                    return ErrorHandlerResult.Rethrow;
                }
            );
        }

        private void InvokeWriter(string toWrite)
        {
            if (Wrote(toWrite))
            {
                return;
            }

            if (_asyncWriter is not null)
            {
                Async.RunSync(() => _asyncWriter.Invoke(toWrite));
            }
        }

        private async Task InvokeWriterAsync(string toWrite)
        {
            if (Wrote(toWrite))
            {
                return;
            }

            if (_asyncWriter is not null)
            {
                await _asyncWriter.Invoke(toWrite);
            }
        }

        private bool Wrote(string str)
        {
            if (_writer is null)
            {
                return false;
            }

            _writer.Invoke(str);
            return true;
        }

        private void InvokeFlush()
        {
            if (Flushed())
            {
                return;
            }

            if (_asyncFlushAction is not null)
            {
                Async.RunSync(() => _asyncFlushAction.Invoke());
            }
        }

        private async Task InvokeFlushAsync()
        {
            if (Flushed())
            {
                return;
            }

            if (_asyncFlushAction is not null)
            {
                await _asyncFlushAction.Invoke();
            }
        }

        private bool Flushed()
        {
            if (_flushAction is null)
            {
                return false;
            }

            _flushAction.Invoke();
            return true;
        }

        private void ClearLastLine()
        {
            if (_lastWriteLength == 0)
            {
                return;
            }

            InvokeWriter($"\r{new String(' ', _lastWriteLength)}\r");
            _lastWriteLength = 0;
        }

        private async Task ClearLastLineAsync()
        {
            if (_lastWriteLength == 0)
            {
                return;
            }

            await InvokeWriterAsync($"\r{new String(' ', _lastWriteLength)}\r");
            _lastWriteLength = 0;
        }

        private void RunWithIoLocking(Action activity)
        {
            using var _ = new AutoLocker(_ioLock);
            activity();
        }

        private async Task RunWithIoLockingAsync(Func<Task> activity)
        {
            using var _ = new AutoLocker(_ioLock);
            await activity();
        }

        private T RunWithIoLocking<T>(Func<T> activity)
        {
            using var _ = new AutoLocker(_ioLock);
            return activity();
        }

        private async Task<T> RunWithIoLockingAsync<T>(Func<Task<T>> activity)
        {
            using var _ = new AutoLocker(_ioLock);
            return await activity();
        }
    }
}