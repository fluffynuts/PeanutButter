using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using PeanutButter.TestUtils.AspNetCore.Builders;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Implements a fake form collection
/// </summary>
public class FakeFormCollection : StringValueMap, IFormCollection, IFake
{
    /// <inheritdoc />
    public FakeFormCollection(
        IDictionary<string, StringValues> store
    ) : base(store)
    {
    }

    /// <inheritdoc />
    public FakeFormCollection()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    /// <summary>
    /// Exposes the form field values
    /// </summary>
    public IDictionary<string, StringValues> FormValues
    {
        get => Store;
        set => Store = value;
    }

    /// <summary>
    /// Exposes the file collection
    /// </summary>
    public IFormFileCollection Files
    {
        get => _files ??= new FakeFormFileCollection();
        set => _files = value ?? new FakeFormFileCollection();
    }

    private IFormFileCollection _files = new FakeFormFileCollection();

    /// <summary>
    /// Adds a file to the file collection
    /// </summary>
    /// <param name="formFile"></param>
    public void AddFile(IFormFile formFile)
    {
        var files = _files.As<FakeFormFileCollection>();
        files.Add(formFile);
    }
}