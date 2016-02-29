using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace PeanutButter.INI.Tests
{
    [TestFixture]
    public class TestINIFile
    {
        public class AutoDeletingTempFile: IDisposable
        {
            public string Path { get { return _tempFile; } }
            private string _tempFile;

            public AutoDeletingTempFile(string append = null)
            {
                _tempFile = System.IO.Path.GetTempFileName();
                if (append != null)
                    _tempFile += append;
            }

            public void Write(IEnumerable<string> lines)
            {
                Write(string.Join("\n", lines));
            }

            public void Write(string data)
            {
                File.WriteAllBytes(_tempFile, Encoding.UTF8.GetBytes(data));
            }

            public void Dispose()
            {
                try
                {
                    File.Delete(_tempFile);
                }
                catch { }
            }
        }

        private class INIFile_EXPOSES_Sections : INIFile.INIFile
        {
            public INIFile_EXPOSES_Sections()
            {
            }

            public INIFile_EXPOSES_Sections(string path) : base(path)
            {
            }

            public new Dictionary<string, Dictionary<string, string>> Data
            {
                get { return base.Data; }
            }
        }

        [Test]
        public void Construct_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            INIFile_EXPOSES_Sections iniFile = null;
            Assert.DoesNotThrow(() => iniFile = new INIFile_EXPOSES_Sections());

            //---------------Test Result -----------------------
            Assert.IsNotNull(iniFile.Data);
        }

        [Test]
        public void Construct_GivenFileName_ShouldLoadFile()
        {
            //---------------Set up test pack-------------------
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                var section = RandString();
                var key = RandString();
                var value = RandString();
                var iniDataLines = new[] { "[" + section + "]", key + "=" + value };
                tempFile.Write(iniDataLines);

                //---------------Assert Precondition----------------
                Assert.IsTrue(File.Exists(tempFile.Path));

                //---------------Execute Test ----------------------
                var iniFile = new INIFile_EXPOSES_Sections(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.IsNotNull(iniFile.Data);
                Assert.IsTrue(iniFile.Data.Keys.Contains(section));
                Assert.AreEqual(iniFile.Data[section][key], value);
            }
        }

        private static string RandString()
        {
            return RandomValueGen.GetRandomAlphaString(1, 10);
        }

        [Test]
        public void Parse_WhenValueIsQuoted_ShouldSetDataValueUnQuoted()
        {
            //---------------Set up test pack-------------------
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                var section = RandString();
                var key = RandString();
                var value = RandString();
                var iniDataLines = new[] { "[" + section + "]", key + "=\"" + value + "\"" };
                tempFile.Write(iniDataLines);

                //---------------Assert Precondition----------------
                Assert.IsTrue(File.Exists(tempFile.Path));

                //---------------Execute Test ----------------------
                var iniFile = new INIFile_EXPOSES_Sections(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.IsNotNull(iniFile.Data);
                Assert.IsTrue(iniFile.Data.Keys.Contains(section));
                Assert.AreEqual(value, iniFile.Data[section][key]);
            }
        }

        private INIFile_EXPOSES_Sections Create(string path = null)
        {
            return new INIFile_EXPOSES_Sections(path);
        }

        [Test]
        public void AddSection_ShouldAddSectionToSections()
        {
            //---------------Set up test pack-------------------
            var iniFile = Create();
            var section = RandString();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            iniFile.AddSection(section);
            //---------------Test Result -----------------------
            Assert.IsTrue(iniFile.Data.Keys.Contains(section));
        }

        [Test]
        public void Construct_GivenFileNameWhichDoesntExist_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                File.Delete(tempFile.Path);
                //---------------Assert Precondition----------------
                Assert.IsFalse(File.Exists(tempFile.Path));
                //---------------Execute Test ----------------------
                INIFile_EXPOSES_Sections iniFile = null;
                Assert.DoesNotThrow(() => iniFile = Create(tempFile.Path));

                //---------------Test Result -----------------------
                Assert.IsNotNull(iniFile.Data);
                Assert.IsFalse(iniFile.Data.Keys.Any(k => k != ""));
            }
        }

        [Test]
        public void Persist_GivenFileName_ShouldWriteINIDataToFile()
        {
            //---------------Set up test pack-------------------
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                //---------------Assert Precondition----------------
                var writer = Create(tempFile.Path);
                var section = RandString();
                var key = RandString();
                var value = RandString();
                writer.AddSection(section);
                writer.Data[section][key] = value;

                //---------------Execute Test ----------------------
                writer.Persist(tempFile.Path);
                var reader = Create(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.AreEqual(value, reader.Data[section][key]);
            }
        }

        [Test]
        public void Persist_GivenNoFileName_ShouldWriteINIDataToOriginalFile()
        {
            //---------------Set up test pack-------------------
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                //---------------Assert Precondition----------------
                var writer = Create(tempFile.Path);
                var section = RandString();
                var key = RandString();
                var value = RandString();
                writer.AddSection(section);
                writer.Data[section][key] = value;

                //---------------Execute Test ----------------------
                writer.Persist();
                var reader = Create(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.AreEqual(value, reader.Data[section][key]);
            }
        }

        [Test]
        public void Persist_GivenFileName_ShouldPersistTheGlobalEmptySection()
        {
            //---------------Set up test pack-------------------
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                //---------------Assert Precondition----------------
                var writer = Create(tempFile.Path);
                var section = "";
                var key = RandString();
                var value = RandString();
                writer.AddSection(section);
                writer.Data[section][key] = value;

                //---------------Execute Test ----------------------
                writer.Persist();
                var reader = Create(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.AreEqual(value, reader.Data[section][key]);
            }
        }

        [Test]
        public void Persist_GivenNoFileName_WhenNoFileNameSpecifiedInConstructor_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------
            var writer = Create();
            var section = RandString();
            var key = RandString();
            var value = RandString();
            writer.AddSection(section);
            writer.Data[section][key] = value;

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<ArgumentException>(() => writer.Persist());

            //---------------Test Result -----------------------
            Assert.AreEqual("path", ex.ParamName);
            StringAssert.StartsWith("No path specified to persist to and INIFile instantiated without an auto-path", ex.Message);
        }


        [Test]
        public void ShouldLoadMultipleSectionsAndKeys_NotCaringAboutKeyOrSectionCase()
        {
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                //---------------Set up test pack-------------------
                var lines = new[] {
                    "[Section1]",
                    "path=\"C:\\tmp\\something.txt\"",
                    "color=Red",
                    "",
                    "[Section2]",
                    "number=12",
                    "Season = Green"
                };
                tempFile.Write(lines);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var iniFile = Create(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.AreEqual("C:\\tmp\\something.txt", iniFile.Data["Section1"]["path"]);
                Assert.AreEqual("Red", iniFile.Data["Section1"]["color"]);
                Assert.AreEqual("12", iniFile.Data["section2"]["number"]);
                Assert.AreEqual("Green", iniFile.Data["Section2"]["season"]);
            }
        }

        [Test]
        public void SetValue_GivenSectionKeyAndValue_WhenSectionDoesNotExist_CreatesItAndSetsValue()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var value = RandString();
            var iniFile = Create();
            //---------------Assert Precondition----------------
            Assert.IsFalse(iniFile.Data.Keys.Any(s => s == section));

            //---------------Execute Test ----------------------
            iniFile.SetValue(section, key, value);

            //---------------Test Result -----------------------
            Assert.AreEqual(value, iniFile.Data[section][key]);
        }

        [Test]
        public void SetValue_GivenSectionKeyAndValue_WhenSectionDoesExist_SetsValue()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var value1 = RandString();
            var value2 = RandString();
            var iniFile = Create();
            //---------------Assert Precondition----------------
            Assert.IsFalse(iniFile.Data.Keys.Any(s => s == section));

            //---------------Execute Test ----------------------
            iniFile.SetValue(section, key, value1);
            iniFile.SetValue(section, key, value2);

            //---------------Test Result -----------------------
            Assert.AreEqual(value2, iniFile.Data[section][key]);
        }

        [Test]
        public void GetValue_GivenKnownSectionAndKey_ReturnsValue()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var value = RandString();
            var iniFile = Create();
            iniFile.SetValue(section, key, value);

            //---------------Assert Precondition----------------
            Assert.AreEqual(value, iniFile.Data[section][key]);
            //---------------Execute Test ----------------------
            var result = iniFile.GetValue(section, key);
            //---------------Test Result -----------------------
            Assert.AreEqual(value, result);
        }

        [Test]
        public void GetValue_GivenKnownSectionWithUnknownKeyAndDefault_ShouldReturnDefaultValue()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var value = RandString();
            var defaultValue = RandString();
            var iniFile = Create();
            iniFile.SetValue(section, key, value);
            var otherKey = key + RandString();
            //---------------Assert Precondition----------------
            Assert.IsFalse(iniFile.Data[section].Keys.Contains(otherKey));
            //---------------Execute Test ----------------------
            var result = iniFile.GetValue(section, otherKey, defaultValue);
            //---------------Test Result -----------------------
            Assert.AreEqual(defaultValue, result);
        }
        [Test]
        public void GetValue_GivenUnKnownSectionWithUnknownKeyAndDefault_ShouldReturnDefaultValue()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var value = RandString();
            var defaultValue = RandString();
            var iniFile = Create();
            iniFile.SetValue(section, key, value);
            var otherKey = key + RandString();
            var otherSection = section + RandString();
            //---------------Assert Precondition----------------
            Assert.IsFalse(iniFile.Data.Keys.Contains(otherSection));
            //---------------Execute Test ----------------------
            var result = iniFile.GetValue(otherSection, otherKey, defaultValue);
            //---------------Test Result -----------------------
            Assert.AreEqual(defaultValue, result);
        }

        [Test]
        public void HasSection_GivenNameOfExistingSection_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var iniFile = Create();
            iniFile.AddSection(section);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = iniFile.HasSection(section);

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void HasSection_GivenNameOfNonExistingSection_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var value = RandString();
            var iniFile = Create();
            iniFile.AddSection(section);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = iniFile.HasSection(section+RandString());

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void HasSetting_GivenNameOfExistingSetting_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var value = RandString();
            var iniFile = Create();
            iniFile.SetValue(section, key, value);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = iniFile.HasSetting(section, key);

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void HasSetting_GivenNameOfNonExistingSection_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var value = RandString();
            var iniFile = Create();
            iniFile.SetValue(section, key, value);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = iniFile.HasSetting(section + RandString(), key);

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void HasSetting_GivenNameOfNonExistingSectting_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var value = RandString();
            var iniFile = Create();
            iniFile.SetValue(section, key, value);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = iniFile.HasSetting(section, key + RandString());

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void Construct_WhenGivenPathButFileDoesntExist_ShouldCreateFile()
        {
            //---------------Set up test pack-------------------
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                File.Delete(tempFile.Path);
                //---------------Assert Precondition----------------
                Assert.IsFalse(File.Exists(tempFile.Path));

                //---------------Execute Test ----------------------
                var iniFile = Create(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.IsTrue(File.Exists(tempFile.Path));
            }
        }

        [Test]
        public void IndexedGetter_WhenSectionDoesntExist_ShouldCreateSection()
        {
            //---------------Set up test pack-------------------
            var section = RandomValueGen.GetRandomString();
            var key = RandomValueGen.GetRandomString();
            var value = RandomValueGen.GetRandomString();

            //---------------Assert Precondition----------------
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                var ini = new INIFile.INIFile(tempFile.Path);
                ini[section][key] = value;
                ini.Persist();

                //---------------Execute Test ----------------------
                var ini2 = new INIFile.INIFile(tempFile.Path);
                var result = ini2[section][key];

                //---------------Test Result -----------------------
                Assert.AreEqual(value, result);
            }
        }

        [Test]
        public void Construct_WhenSourceContainsComments_ShouldIgnoreCommentedParts()
        {
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                //---------------Set up test pack-------------------
                var src = new[]
                {
                    "; created by some generator",
                    "[general] ; general settings go here",
                    ";this line should be ignored",
                    "key=value ; this part of the line should be ignored"
                };
                File.WriteAllBytes(tempFile.Path, Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, src)));

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var ini = Create(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.AreEqual(1, ini.Data.Count);
                Assert.AreEqual(1, ini["general"].Keys.Count);
                Assert.AreEqual(1, ini["General"].Keys.Count);
                Assert.AreEqual("value", ini["general"]["key"]);
            }
        }

        [Test]
        public void Parse_GivenStringContents_WhenSourceContainsComments_ShouldIgnoreCommentedParts()
        {
            //---------------Set up test pack-------------------
            var src = new[]
                {
                    "; created by some generator",
                    "[general] ; general settings go here",
                    ";this line should be ignored",
                    "key=value ; this part of the line should be ignored",
                    "something="
                };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ini = Create();
            ini.Parse(string.Join(Environment.NewLine, src));

            //---------------Test Result -----------------------
            Assert.AreEqual(1, ini.Data.Count);
            Assert.AreEqual(2, ini["general"].Keys.Count);
            Assert.AreEqual(2, ini["General"].Keys.Count);
            Assert.AreEqual("value", ini["general"]["key"]);
            Assert.AreEqual("", ini["general"]["something"]);
        }

        [Test]
        public void Sections_ShouldReturnSectionNames()
        {
            //---------------Set up test pack-------------------
            var src = new[]
                {
                    "[general] ; general settings go here",
                    "key=value ; this part of the line should be ignored",
                    "something=",
                    "[section1]",
                    "[section2]"
                };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ini = Create();
            ini.Parse(string.Join(Environment.NewLine, src));

            //---------------Test Result -----------------------
            Assert.AreEqual(3, ini.Sections.Count());
            CollectionAssert.AreEqual(new[] {"general", "section1", "section2"}, ini.Sections);
        }
    }

}
