using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    public class FormFileBuilder : Builder<FormFileBuilder, IFormFile>
    {
        public override FormFileBuilder Randomize()
        {
            return WithName(GetRandomString(10))
                .WithFileName(GetRandomFileName())
                .WithContent(GetRandomWords());
        }

        public FormFileBuilder WithName(string name)
        {
            return With<FakeFormFile>(
                o => o.Name = name
            );
        }

        public FormFileBuilder WithFileName(string fileName)
        {
            return With<FakeFormFile>(
                o => o.FileName = fileName
            );
        }

        public FormFileBuilder WithContent(string content)
        {
            return WithContent(Encoding.UTF8.GetBytes(content));
        }

        public FormFileBuilder WithContent(byte[] content)
        {
            return WithContent(new MemoryStream(content));
        }

        public FormFileBuilder WithContent(Stream stream)
        {
            return With<FakeFormFile>(
                o => o.SetContent(stream)
            );
        }

        protected override IFormFile ConstructEntity()
        {
            return new FakeFormFile();
        }
    }
}