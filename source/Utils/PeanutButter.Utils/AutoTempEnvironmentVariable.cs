using System;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides a scoped, temporary environment variable
    /// </summary>
    public class AutoTempEnvironmentVariable : IDisposable
    {
        /// <summary>
        /// The name of the temp env var
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The value this host will set
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// The original value of the env var
        /// </summary>
        public string OriginalValue { get; set; }

        /// <summary>
        /// sets up the auto-temp env var
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="NotImplementedException"></exception>
        public AutoTempEnvironmentVariable(
            string name,
            string value
        )
        {
            Name = name;
            Value = value;
            OriginalValue = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }

        private bool _disposed;
        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            Environment.SetEnvironmentVariable(Name, OriginalValue);
        }
    }
}