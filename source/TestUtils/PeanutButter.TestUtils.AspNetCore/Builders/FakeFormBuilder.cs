using System.IO;
using System.Text;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

public class FakeFormBuilder : Builder<FakeFormBuilder, FakeFormCollection>
{
    static FakeFormBuilder()
    {
        InstallRandomGenerators();
    }

    public static void InstallRandomGenerators()
    {
        Run.Once<FakeFormBuilder>(() =>
        {
            InstallRandomGenerator<FakeFormCollection>(
                BuildRandom
            );
        });
    }

    public static FakeFormBuilder Create()
    {
        return new FakeFormBuilder();
    }

    public static FakeFormCollection BuildDefault()
    {
        return Create().Build();
    }

    public static FakeFormCollection BuildRandom()
    {
        return Create().Randomize().Build();
    }

    public FakeFormBuilder Randomize()
    {
        var fieldCount = GetRandomInt(1, 4);
        for (var i = 0; i < fieldCount; i++)
        {
            WithField(GetRandomString(10), GetRandomString(10));
        }

        return this;
    }

    public FakeFormBuilder WithField(
        string name,
        string value
    )
    {
        return With(
            o => o.FormValues[name] = value
        );
    }

    public FakeFormBuilder WithFile(
        string contents,
        string name
    )
    {
        return WithFile(contents, name, name);
    }

    public FakeFormBuilder WithFile(
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

    public FakeFormBuilder WithFile(
        byte[] contents,
        string name
    )
    {
        return WithFile(contents, name, name);
    }

    public FakeFormBuilder WithFile(
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

    public FakeFormBuilder WithFile(
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

    public FakeFormBuilder WithFile(
        Stream content,
        string name,
        string fileName
    )
    {
        return With(
            o => o.AddFile(new FakeFormFile(content, name, fileName))
        );
    }
}