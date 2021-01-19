using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;

// ReSharper disable InconsistentNaming

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Wraps process IO (stdout, stderr) into an easy-to-access disposable source
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        interface IProcessIO : IDisposable
    {
        /// <summary>
        /// True if the process started properly
        /// </summary>
        bool Started { get; }

        /// <summary>
        /// Set if the process didn't start properly, to the exception thrown
        /// </summary>
        Exception StartException { get; }

        /// <summary>
        /// Read lines from stdout
        /// </summary>
        IEnumerable<string> StandardOutput { get; }

        /// <summary>
        /// Read lines from stderr
        /// </summary>
        IEnumerable<string> StandardError { get; }

        /// <summary>
        /// Access to the underlying Process
        /// </summary>
        Process Process { get; }

        /// <summary>
        /// Provides access to the exit code of the process,
        /// waiting for it to complete if necessary
        /// </summary>
        int ExitCode { get; }
    }

    /// <summary>
    /// Provides the contract for an unstarted ProcessIO, as would be
    /// obtained from `ProcessIO.In(workingDir)`
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        interface IUnstartedProcessIO : IProcessIO
    {
        /// <summary>
        /// Starts the process in the previously provided working directory
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        IProcessIO Start(string filename, params string[] arguments);
    }

    /// <inheritdoc />
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class ProcessIO : IProcessIO
    {
        /// <inheritdoc />
        public bool Started { get; private set; }

        /// <inheritdoc />
        public Exception StartException { get; private set; }

        /// <inheritdoc />
        public Process Process => _process;

        private Process _process;

        /// <summary>
        /// Run the provided command, pipe output as it streams
        /// </summary>
        /// <param name="filename">app to run</param>
        /// <param name="arguments">args for that app</param>
        [Obsolete(
            "Please use the static Process.Start or Process.In helpers; this constructor will be made internal in the future")]
        public ProcessIO(
            string filename,
            params string[] arguments
        )
        {
            StartInFolder(Environment.CurrentDirectory, filename, arguments, null);
        }

        private ProcessIO()
        {
        }

        /// <summary>
        /// Represents an unstarted process-io instance
        /// </summary>
        public class UnstartedProcessIO : ProcessIO, IUnstartedProcessIO
        {
            /// <summary>
            /// Working directory for the process, once started
            /// </summary>
            public string WorkingDirectory { get; }

            private readonly Dictionary<string, string> _environment = new Dictionary<string, string>();

            /// <inheritdoc />
            internal UnstartedProcessIO(string workingDirectory)
            {
                if (string.IsNullOrWhiteSpace(workingDirectory))
                {
                    throw new ArgumentException($"{nameof(workingDirectory)} must be provided");
                }

                if (!Directory.Exists(workingDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(workingDirectory);
                    }
                    catch
                    {
                        throw new ArgumentException(
                            $"Unable to find or create working directory '{workingDirectory}'"
                        );
                    }
                }

                WorkingDirectory = workingDirectory;
            }


            /// <inheritdoc />
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new IProcessIO Start(string filename, params string[] arguments)
            {
                return StartInFolder(WorkingDirectory, filename, arguments, _environment);
            }

            /// <summary>
            /// Sets up for the new process to use the provided environment variable
            /// </summary>
            /// <param name="name"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new IUnstartedProcessIO WithEnvironmentVariable(
                string name,
                string value
            )
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new InvalidOperationException($"environment variable name may not be null or blank");
                }

                _environment[name] = value;
                return this;
            }
        }

        /// <summary>
        /// Sets up ProcessIO to run within the provided folder. Usage:
        /// using var io = ProcessIO.In("/path/to/folder").Start("cmd", "arg1", "arg2")
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <returns></returns>
        public static UnstartedProcessIO In(string workingDirectory)
        {
            return new UnstartedProcessIO(workingDirectory);
        }

        /// <summary>
        /// Starts a ProcessIO instance for the given filename and args in the current
        /// working directory
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IProcessIO Start(string filename, params string[] args)
        {
#pragma warning disable 618
            return new ProcessIO(filename, args);
#pragma warning restore 618
        }

        private ProcessIO StartInFolder(
            string workingDirectory,
            string filename,
            string[] arguments,
            IDictionary<string, string> environment
        )
        {
            if (Started && !(_process?.HasExited ?? true))
            {
                throw new InvalidOperationException($"Process already started: {_process.Id}");
            }

            var processEnvironment = GenerateProcessEnvironmentFor(environment);

            try
            {
                _process = new Process()
                {
                    StartInfo =
                    {
                        FileName = filename,
                        Arguments = MakeArgsFrom(arguments),
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WorkingDirectory = workingDirectory,
#if NET452
                        EnvironmentVariables = { }
#else
                        Environment = { }
#endif
                    }
                };
                processEnvironment.ForEach(kvp =>
                {
#if NET452
                    _process.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
#else
                    _process.StartInfo.Environment[kvp.Key] = kvp.Value;
#endif
                });
                _process.Start();
                Started = true;
            }
            catch (Exception ex)
            {
                StartException = ex;
                _process = null;
            }

            return this;
        }

        private static IDictionary<string, string> GenerateProcessEnvironmentFor(
            IDictionary<string, string> environment)
        {
            var processEnvironment = Environment.GetEnvironmentVariables()
                .ToDictionary<string, string>();
            environment?.ForEach(kvp =>
            {
                if (kvp.Value is null)
                {
                    if (processEnvironment.ContainsKey(kvp.Key))
                    {
                        processEnvironment.Remove(kvp.Key);
                    }
                }
                else
                {
                    processEnvironment[kvp.Key] = kvp.Value;
                }
            });
            return processEnvironment;
        }

        /// <inheritdoc />
        public int ExitCode
        {
            get
            {
                if (Process is null)
                {
                    throw new InvalidOperationException($"Process has not started yet");
                }

                if (!Process.HasExited)
                {
                    Process.WaitForExit();
                }

                return Process.ExitCode;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> StandardOutput
        {
            get
            {
                if (!Started)
                {
                    yield break;
                }

                while (!_process.StandardOutput.EndOfStream)
                {
                    yield return _process.StandardOutput.ReadLine();
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> StandardError
        {
            get
            {
                if (!Started)
                {
                    yield break;
                }

                while (!_process.StandardError.EndOfStream)
                {
                    yield return _process.StandardError.ReadLine();
                }
            }
        }

        private string MakeArgsFrom(string[] parameters)
        {
            return string.Join(" ", parameters
                .Select(p => p.Contains(" ")
                    ? $"\"{p}\""
                    : p
                )
            );
        }

        /// <summary>
        /// Kills the process if it hasn't finished yet
        /// - you should always dispose, since you may decide not to read until the process is dead
        /// </summary>
        public void Dispose()
        {
            if (!_process?.HasExited ?? false)
            {
                try
                {
                    _process?.Kill();
                }
                catch
                {
                    /* intentionally suppressed */
                }

                _process?.Dispose();
            }

            _process = null;
        }

        /// <summary>
        /// Sets up for the new process to use the provided environment variable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static IUnstartedProcessIO WithEnvironmentVariable(
            string name,
            string value
        )
        {
            var result = new UnstartedProcessIO(Environment.CurrentDirectory);
            return result.WithEnvironmentVariable(name, value);
        }
    }
}