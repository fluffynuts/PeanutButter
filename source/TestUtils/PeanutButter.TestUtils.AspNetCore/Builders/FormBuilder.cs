using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// Builds a form for an asp.net request
/// </summary>
public class FormBuilder : RandomizableBuilder<FormBuilder, IFormCollection>
{
    /// <summary>
    /// Constructs the fake form
    /// </summary>
    /// <returns></returns>
    protected override IFormCollection ConstructEntity()
    {
        return new FakeFormCollection();
    }

    /// <summary>
    /// Randomizes the form
    /// </summary>
    /// <returns></returns>
    public override FormBuilder Randomize()
    {
        var fieldCount = GetRandomInt(1, 4);
        for (var i = 0; i < fieldCount; i++)
        {
            WithField(GetRandomString(10), GetRandomString(10));
        }

        return this;
    }

    /// <summary>
    /// Sets a form field. If the field already exists by name, it is overwritten.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public FormBuilder WithField(
        string name,
        string value
    )
    {
        return With<FakeFormCollection>(
            o => o.FormValues[name] = value
        );
    }

    /// <summary>
    /// Sets a form file
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public FormBuilder WithFile(
        string contents,
        string name
    )
    {
        return WithFile(contents, name, name);
    }

    /// <summary>
    /// Sets a form file
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public FormBuilder WithFile(
        string contents,
        string name,
        string fileName
    )
    {
        return WithFile(
            Encoding.UTF8.GetBytes(contents),
            name,
            fileName
        );
    }

    /// <summary>
    /// Sets a form file
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public FormBuilder WithFile(
        byte[] contents,
        string name
    )
    {
        return WithFile(contents, name, name);
    }

    /// <summary>
    /// Sets a form file
    /// </summary>
    /// <param name="contents"></param>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public FormBuilder WithFile(
        byte[] contents,
        string name,
        string fileName
    )
    {
        return WithFile(
            new MemoryStream(contents),
            name,
            fileName
        );
    }

    /// <summary>
    /// Sets a form file
    /// </summary>
    /// <param name="content"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public FormBuilder WithFile(
        Stream content,
        string name
    )
    {
        return WithFile(
            content,
            name,
            name
        );
    }

    /// <summary>
    /// Adds a form file
    /// </summary>
    /// <param name="content"></param>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public FormBuilder WithFile(
        Stream content,
        string name,
        string fileName
    )
    {
        return WithFile(
            new FakeFormFile(
                content,
                name,
                fileName
            )
        );
    }

    /// <summary>
    /// Adds a random file to the form
    /// </summary>
    /// <returns></returns>
    public FormBuilder WithRandomFile()
    {
        return With<FakeFormCollection>(
            o => o.AddFile(FormFileBuilder.BuildRandom()
            )
        );
    }

    /// <summary>
    /// Adds a form file
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public FormBuilder WithFile(IFormFile file)
    {
        return With<FakeFormCollection>(
            o => o.AddFile(file)
        );
    }
}