using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

// ReSharper disable MemberCanBePrivate.Global
#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
using static Imported.PeanutButter.RandomGenerators.RandomValueGen;

namespace Imported.PeanutButter.TestUtils.AspNetCore.Builders;
#else
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Builders;
#endif

/// <summary>
/// Builds a form file
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FormFileBuilder : RandomizableBuilder<FormFileBuilder, IFormFile>
{
    /// <summary>
    /// Randomizes the file
    /// </summary>
    /// <returns></returns>
    public override FormFileBuilder Randomize()
    {
        return WithName(GetRandomString(10))
            .WithFileName(GetRandomFileName())
            .WithContent(GetRandomWords());
    }

    /// <summary>
    /// Sets the field name of the file
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public FormFileBuilder WithName(string name)
    {
        return With<FakeFormFile>(
            o => o.Name = name
        );
    }

    /// <summary>
    /// Sets the filename of the file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public FormFileBuilder WithFileName(string fileName)
    {
        return With<FakeFormFile>(
            o => o.FileName = fileName
        );
    }

    /// <summary>
    /// Sets the content of the file
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public FormFileBuilder WithContent(string content)
    {
        return WithContent(Encoding.UTF8.GetBytes(content));
    }

    /// <summary>
    /// Sets the content of the file
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public FormFileBuilder WithContent(byte[] content)
    {
        return WithContent(new MemoryStream(content));
    }

    /// <summary>
    /// Sets the content of the file
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public FormFileBuilder WithContent(Stream stream)
    {
        return With<FakeFormFile>(
            o => o.SetContent(stream)
        );
    }

    /// <summary>
    /// Constructs the fake form file
    /// </summary>
    /// <returns></returns>
    protected override IFormFile ConstructEntity()
    {
        return new FakeFormFile();
    }
}