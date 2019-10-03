using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;
using NExpect;

namespace PeanutButter.XmlUtils.Tests
{
    [TestFixture]
    public class TestXElementExtensions
    {
        [TestFixture]
        public class Text
        {
            [Test]
            public void ShouldReturnSingleTextNodeOfElement()
            {
                // test setup
                var text = RandomValueGen.GetRandomString(10, 20);
                var tag = RandomValueGen.GetRandomAlphaString(10, 20);
                var el = new XElement(tag, new XText(text));

                // pre-conditions

                // execute test
                var result = el.Text();

                // test result
                Expect(result)
                    .Not.To.Be.Null();
                Expect(result)
                    .To.Equal(text);
            }

            [Test]
            public void ShouldReturnMultipleTextNodesSeparatedWithNewlines()
            {
                // test setup
                var t1 = RandomValueGen.GetRandomAlphaString(10, 20);
                var t2 = RandomValueGen.GetRandomAlphaString(10, 20);
                var tag = RandomValueGen.GetRandomAlphaString(8, 20);
                var el = new XElement(tag, new XText(t1), new XText(t2));

                // pre-conditions

                // execute test
                var result = el.Text();

                // test result
                var parts = result.Split('\n');
                
                Expect(parts)
                    .To.Equal(new[] { t1, t2 });
            }
        }

        [TestFixture]
        public class ScrubNamespaces
        {
            [Test]
            public void ShouldScrubNamespaces()
            {
                // Arrange
                var doc = XDocument.Parse(CSPROJ.Trim());
                var xpath = "/Project";
                // get null, because of namespace :/
                Expect(doc.XPathSelectElement(xpath))
                    .To.Be.Null();
                // Act
                doc.ScrubNamespaces();
                // Assert
                Expect(doc.XPathSelectElement(xpath))
                    .Not.To.Be.Null();
            }
            
            private const string CSPROJ = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""15.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
    <PropertyGroup>
        <LangVersion>latest</LangVersion>
        <TargetFrameworkVersion>v4.6.2</TargetFrameworkVersion>
        <ImplicitlyExpandNETStandardFacades>false</ImplicitlyExpandNETStandardFacades>
    </PropertyGroup>
    <ItemGroup>
        <Reference Include=""Analytics.NET, Version=2.0.3.0, Culture=neutral, processorArchitecture=MSIL"">
            <HintPath>..\packages\Analytics.3.0.0\lib\Analytics.NET.dll</HintPath>
        </Reference>
        <Reference Include=""BouncyCastle.Crypto, Version=1.8.3.0, Culture=neutral, PublicKeyToken=0e99375e54769942"">
            <HintPath>..\packages\BouncyCastle.1.8.3.1\lib\BouncyCastle.Crypto.dll</HintPath>
            <Private>True</Private>
        </Reference>
    </ItemGroup>
    <ItemGroup>
        <Compile Include=""Commands\Moo\MooCommand.cs"" />
        <Compile Include=""Queries\Cow\CowQuery.cs"" />
        <Compile Include=""Base.cs"" />
    </ItemGroup>
</Project>
";

        }
    }
}
