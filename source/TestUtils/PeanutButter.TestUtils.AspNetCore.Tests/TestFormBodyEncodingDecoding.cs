using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.TestUtils.AspNetCore.Utils;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestFormBodyEncodingDecoding
{
    [TestFixture]
    public class UrlEncodedForms
    {
        [Test]
        public void ShouldEncodeDecodeUrlEncodedForm()
        {
            // Arrange
            var form = new FakeFormCollection
            {
                FormValues =
                {
                    ["foo"] = "bar",
                    ["one"] = "two"
                }
            };
            var encoder = new UrlEncodedBodyEncoder();
            var decoder = new FormDecoder();
            // Act
            var stream = encoder.Encode(form);
            var result = decoder.Decode(stream);
            // Assert
            Expect(result.Files.Count)
                .To.Equal(0);
            Expect(result.Keys)
                .To.Equal(form.Keys);
            foreach (var key in result.Keys)
            {
                Expect(result[key])
                    .To.Equal(form[key]);
            }
        }
    }

    [TestFixture]
    public class MultiPartForms
    {
        [Test]
        public void ShouldEncodeDecodeFormFields()
        {
            // Arrange
            var form = new FakeFormCollection()
            {
                FormValues =
                {
                    ["one"] = "first",
                    ["two"] = "second"
                }
            };
            var encoder = new MultiPartBodyEncoder();
            var decoder = new FormDecoder();

            // Act
            var stream = encoder.Encode(form);
            var result = decoder.Decode(stream);

            // Assert

            Expect(result.Files.Count)
                .To.Equal(0);
            Expect(result.Keys)
                .To.Equal(form.Keys);
            foreach (var key in result.Keys)
            {
                Expect(result[key])
                    .To.Equal(form[key]);
            }
        }

        [TestCase("\r\n")]
        [TestCase("\n")]
        public void ShouldEncodeDecodeFormFieldAndFile(string eol)
        {
            // Arrange
            var svg1 = $"<svg>{eol}<circle />{eol}</svg>";
            var svg2 = $"<svg>{eol}<point />{eol}</svg>";
            var form = new FakeFormCollection()
            {
                FormValues =
                {
                    ["oneone"] = "eleven",
                    ["onetwo"] = "twelve"
                },
                Files = new FakeFormFileCollection()
                {
                    new FakeFormFile(svg1, "image1", "image1.svg"),
                    new FakeFormFile(svg2, "image2", "image2.svg")
                }
            };
            var encoder = new MultiPartBodyEncoder();
            var decoder = new FormDecoder();

            // Act
            var stream = encoder.Encode(form);
            var encoded = stream.ReadAllText();
            Expect(encoded).To.Equal(stream.ReadAllText());
            var result = decoder.Decode(stream);
            // Assert
            Expect(result.Keys)
                .To.Equal(form.Keys);
            foreach (var key in result.Keys)
            {
                Expect(result[key])
                    .To.Equal(form[key]);
            }
            
            Expect(result.Files.Count)
                .To.Equal(2);
            
            var first = result.Files[0];
            Expect(first.Name)
                .To.Equal("image1");
            Expect(first.FileName)
                .To.Equal("image1.svg");
            using var s1 = first.OpenReadStream();
            Expect(s1.ReadAllText())
                .To.Equal(svg1);
            
            var second = result.Files[1];
            Expect(second.Name)
                .To.Equal("image2");
            Expect(second.FileName)
                .To.Equal("image2.svg");
            using var s2 = second.OpenReadStream();
            Expect(s2.ReadAllText())
                .To.Equal(svg2);
            
        }
    }
}