using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

// ReSharper disable MemberCanBePrivate.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Provides a fake form file collection
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FakeFormFileCollection : IFormFileCollection, IFake
{
    private readonly List<IFormFile> _store = new();

    /// <inheritdoc />
    public IEnumerator<IFormFile> GetEnumerator()
    {
        return _store.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public int Count => _store.Count;

    /// <inheritdoc />
    public IFormFile this[int index] => _store[index];

    /// <inheritdoc />
    public IFormFile GetFile(string name)
    {
        return _store.FirstOrDefault(o => o.Name == name);
    }

    /// <inheritdoc />
    public IReadOnlyList<IFormFile> GetFiles(string name)
    {
        return _store
            .Where(o => o.Name == name)
            .ToList();
    }

    /// <inheritdoc />
    public IFormFile this[string name] => GetFile(name);

    /// <summary>
    /// Adds a form file
    /// </summary>
    /// <param name="formFile"></param>
    public void Add(IFormFile formFile)
    {
        _store.Add(formFile);
    }

    /// <summary>
    /// Removes the file
    /// </summary>
    /// <param name="formFile"></param>
    public void Remove(IFormFile formFile)
    {
        _store.Remove(formFile);
    }

    /// <summary>
    /// Removes all files with this field name
    /// </summary>
    /// <param name="name"></param>
    public void Remove(string name)
    {
        var existing = _store.Where(f => f.Name == name).ToArray();
        foreach (var e in existing)
        {
            Remove(e);
        }
    }

    /// <summary>
    /// Clears the file collection
    /// </summary>
    public void Clear()
    {
        _store.Clear();
    }
}