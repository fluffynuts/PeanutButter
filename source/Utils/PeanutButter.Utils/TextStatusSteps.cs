using System;
using System.Linq;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides an easy mechanism for steps of a process with
    /// status feedback, ie "starting X..." / {does X} / {"completed" / "failed"}
    /// </summary>
    public class TextStatusSteps
    {
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
        )
        {
            _prefixAllStatusLines = prefixAllStatusLines;
            _completedMarker = completedMarker;
            _failedMarker = failedMarker;
            _writer = writer;
            _flushAction = flushAction;
            _exceptionHandler = exceptionHandler;
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

            _steps = new Steps();
        }

        /// <summary>
        /// Run the provided activity with the given label
        /// </summary>
        /// <param name="label"></param>
        /// <param name="activity"></param>
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
                    return _exceptionHandler?.Invoke(ex) ?? ErrorHandlerResult.Rethrow;
                }
            );
        }

        private void Fail(string label)
        {
            ClearLastLine();
            _writer?.Invoke($"{_failedMarker} {_prefixAllStatusLines?.Invoke()}{label}\n");
            _flushAction?.Invoke();
        }

        private void Ok(string label)
        {
            ClearLastLine();
            _writer?.Invoke($"{_completedMarker} {_prefixAllStatusLines?.Invoke()}{label}\n");
            _flushAction?.Invoke();
        }

        private void ClearLastLine()
        {
            if (_lastWriteLength == 0)
            {
                return;
            }

            _writer?.Invoke($"\r{new String(' ', _lastWriteLength)}\r");
            _lastWriteLength = 0;
        }

        private void Start(string label)
        {
            var toWrite = $"{_startMarker} {_prefixAllStatusLines?.Invoke()}{label}";
            _lastWriteLength = toWrite.Length;
            _writer?.Invoke(toWrite);
            _flushAction?.Invoke();
        }
    }
}