using System;
#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif

/// <summary>
/// Performs work within the provided working
/// folder and then skips out of there.
/// Probably not a good idea to use whilst
/// performing parallel processing as it
/// changes the entire CWD for the current
/// process!
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class AutoWorkFolder : IDisposable
{
    private AutoTempFolder _tempFolder;
    private readonly string _startDir;

    /// <summary>
    /// Constructs the auto work dir with
    /// a temporary folder backed by AutoTempFolder
    /// </summary>
    public AutoWorkFolder() : this(new AutoTempFolder(), true)
    {
    }

    /// <summary>
    /// Constructs the auto work dir with
    /// the provided temp folder, but will
    /// not dispose of the temp folder!
    /// </summary>
    /// <param name="tempFolder"></param>
    public AutoWorkFolder(
        AutoTempFolder tempFolder
    ) : this(tempFolder, false)
    {
    }

    /// <summary>
    /// Constructs the auto work dir with
    /// the provided temp folder and will
    /// own disposal of that temp folder
    /// </summary>
    /// <param name="tempFolder"></param>
    /// <param name="disposeTempFolderWhenDone"></param>
    public AutoWorkFolder(
        AutoTempFolder tempFolder,
        bool disposeTempFolderWhenDone
    ) : this(tempFolder.Path)
    {
        if (disposeTempFolderWhenDone)
        {
            _tempFolder = tempFolder;
        }
    }

    /// <summary>
    /// Constructs the auto work dir with
    /// the provided path to a folder
    /// </summary>
    /// <param name="folder"></param>
    public AutoWorkFolder(string folder)
    {
        _startDir = Environment.CurrentDirectory;
        Environment.CurrentDirectory = folder;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _tempFolder?.Dispose();
        _tempFolder = null;
        Environment.CurrentDirectory = _startDir;
    }
}