using System;
using System.Collections.Generic;
using System.IO;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides a mechanism to autmatically delete one or more files
    /// using the IDisposable pattern.
    /// Use this when you'd like to clean up some temporary files after an
    /// operation completes without having to worry about exception handling, etc.
    /// Files which cannot be deleted (eg: locked for reading / writing) will be
    /// left behind. No exceptions are thrown. Files which have been removed in the
    /// interim do not cause any exceptions.
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class AutoDeleter : IDisposable
    {
        private readonly List<string> _toDelete;

        /// <summary>
        /// Constructs a new AutoDeleter with zero or more paths to delete upon disposal
        /// </summary>
        /// <param name="paths">Params array of paths to delete upon disposal</param>
        public AutoDeleter(params string[] paths)
        {
            _toDelete = new List<string>();
            Add(paths);
        }

        /// <summary>
        /// Adds zero or more paths to the list to delete upon disposal
        /// </summary>
        /// <param name="paths">Params array of paths to add to the list to delete upon disposal</param>
        public void Add(params string[] paths)
        {
            _toDelete.AddRange(paths);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (this)
            {
                foreach (var f in _toDelete)
                {
                    try
                    {
                        if (Directory.Exists(f))
                        {
                            Directory.Delete(f, true);
                        }
                        else
                        {
                            File.Delete(f);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(
                            $"Unable to delete temporary artifact '{f}': {ex}"
                        );
                    }
                }

                _toDelete.Clear();
            }
        }
    }
}