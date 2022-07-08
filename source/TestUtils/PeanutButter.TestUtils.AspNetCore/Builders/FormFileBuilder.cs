using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    /// <summary>
    /// Builds a form file
    /// </summary>
    public class FormFileBuilder : Builder<FormFileBuilder, IFormFile>
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
}