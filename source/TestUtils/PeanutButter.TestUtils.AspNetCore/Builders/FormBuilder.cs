using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

public class FormBuilder : Builder<FormBuilder, IFormCollection>
{
    protected override IFormCollection ConstructEntity()
    {
        return new FakeFormCollection();
    }

    public override FormBuilder Randomize()
    {
        var fieldCount = GetRandomInt(1, 4);
        for (var i = 0; i < fieldCount; i++)
        {
            WithField(GetRandomString(10), GetRandomString(10));
        }

        return this;
    }

    public FormBuilder WithField(
        string name,
        string value
    )
    {
        return With<FakeFormCollection>(
            o => o.FormValues[name] = value
        );
    }

    public FormBuilder WithFile(
        string contents,
        string name
    )
    {
        return WithFile(contents, name, name);
    }

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

    public FormBuilder WithFile(
        byte[] contents,
        string name
    )
    {
        return WithFile(contents, name, name);
    }

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

    public FormBuilder WithRandomFile()
    {
        return With<FakeFormCollection>(
            o => o.AddFile(FormFileBuilder.BuildRandom()
            )
        );
    }

    public FormBuilder WithFile(IFormFile file)
    {
        return With<FakeFormCollection>(
            o => o.AddFile(file)
        );
    }
}