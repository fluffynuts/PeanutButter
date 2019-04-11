using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
// ReSharper disable InconsistentNaming

namespace PeanutButter.Utils
{
    /// <summary>
    /// Wraps process IO (stdout, stderr) into an easy-to-access disposable source
    /// </summary>
    public interface IProcessIO: IDisposable
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
    }

    /// <inheritdoc />
    public class ProcessIO : IProcessIO
    {
        /// <inheritdoc />
        public bool Started { get; }

        /// <inheritdoc />
        public Exception StartException { get; }

        private Process _process;

        /// <inheritdoc />
        public ProcessIO(string filename, params string[] arguments)
        {
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
                        UseShellExecute = false
                    }
                };
                _process.Start();
                Started = true;
            }
            catch (Exception ex)
            {
                StartException = ex;
                _process = null;
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
            return parameters
                   .Select(p => p.Contains(" ") ? $"\"{p}\"" : p)
                   .JoinWith(" ");
        }

        /// <summary>
        /// Kills the process if it hasn't finished yet
        /// - you should always dispose, since you may decide not to read until the process is dead
        /// </summary>
        public void Dispose()
        {
            if (!_process?.HasExited ?? false)
            {
                _process?.Kill();
                _process?.Dispose();
            }

            _process = null;
        }
    }
}