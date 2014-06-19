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
                Write(String.Join("\n", lines));
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
        [Test]
        public void Construct_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            INIFile iniFile = null;
            Assert.DoesNotThrow(() => iniFile = new INIFile());

            //---------------Test Result -----------------------
            Assert.IsNotNull(iniFile.Sections);
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
                var iniFile = new INIFile(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.IsNotNull(iniFile.Sections);
                Assert.IsTrue(iniFile.Sections.Keys.Contains(section));
                Assert.AreEqual(iniFile.Sections[section][key], value);
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
                var iniFile = new INIFile(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.IsNotNull(iniFile.Sections);
                Assert.IsTrue(iniFile.Sections.Keys.Contains(section));
                Assert.AreEqual(value, iniFile.Sections[section][key]);
            }
        }

        [Test]
        public void AddSection_ShouldAddSectionToSections()
        {
            //---------------Set up test pack-------------------
            var iniFile = new INIFile();
            var section = RandString();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            iniFile.AddSection(section);
            //---------------Test Result -----------------------
            Assert.IsTrue(iniFile.Sections.Keys.Contains(section));
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
                INIFile iniFile = null;
                Assert.DoesNotThrow(() => iniFile = new INIFile(tempFile.Path));

                //---------------Test Result -----------------------
                Assert.IsNotNull(iniFile.Sections);
                Assert.IsFalse(iniFile.Sections.Keys.Any(k => k != ""));
            }
        }

        [Test]
        public void Persist_GivenFileName_ShouldWriteINIDataToFile()
        {
            //---------------Set up test pack-------------------
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                //---------------Assert Precondition----------------
                var writer = new INIFile(tempFile.Path);
                var section = RandString();
                var key = RandString();
                var value = RandString();
                writer.AddSection(section);
                writer.Sections[section][key] = value;

                //---------------Execute Test ----------------------
                writer.Persist(tempFile.Path);
                var reader = new INIFile(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.AreEqual(value, reader.Sections[section][key]);
            }
        }

        [Test]
        public void Persist_GivenNoFileName_ShouldWriteINIDataToOriginalFile()
        {
            //---------------Set up test pack-------------------
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                //---------------Assert Precondition----------------
                var writer = new INIFile(tempFile.Path);
                var section = RandString();
                var key = RandString();
                var value = RandString();
                writer.AddSection(section);
                writer.Sections[section][key] = value;

                //---------------Execute Test ----------------------
                writer.Persist();
                var reader = new INIFile(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.AreEqual(value, reader.Sections[section][key]);
            }
        }

        [Test]
        public void Persist_GivenFileName_ShouldPersistTheGlobalEmptySection()
        {
            //---------------Set up test pack-------------------
            using (var tempFile = new AutoDeletingTempFile(".ini"))
            {
                //---------------Assert Precondition----------------
                var writer = new INIFile(tempFile.Path);
                var section = "";
                var key = RandString();
                var value = RandString();
                writer.AddSection(section);
                writer.Sections[section][key] = value;

                //---------------Execute Test ----------------------
                writer.Persist();
                var reader = new INIFile(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.AreEqual(value, reader.Sections[section][key]);
            }
        }

        [Test]
        public void Persist_GivenNoFileName_WhenNoFileNameSpecifiedInConstructor_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------
            var writer = new INIFile();
            var section = RandString();
            var key = RandString();
            var value = RandString();
            writer.AddSection(section);
            writer.Sections[section][key] = value;

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
                var iniFile = new INIFile(tempFile.Path);

                //---------------Test Result -----------------------
                Assert.AreEqual("C:\\tmp\\something.txt", iniFile.Sections["Section1"]["path"]);
                Assert.AreEqual("Red", iniFile.Sections["Section1"]["color"]);
                Assert.AreEqual("12", iniFile.Sections["section2"]["number"]);
                Assert.AreEqual("Green", iniFile.Sections["Section2"]["season"]);
            }
        }

        [Test]
        public void SetValue_GivenSectionKeyAndValue_WhenSectionDoesNotExist_CreatesItAndSetsValue()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var value = RandString();
            var iniFile = new INIFile();
            //---------------Assert Precondition----------------
            Assert.IsFalse(iniFile.Sections.Keys.Any(s => s == section));

            //---------------Execute Test ----------------------
            iniFile.SetValue(section, key, value);

            //---------------Test Result -----------------------
            Assert.AreEqual(value, iniFile.Sections[section][key]);
        }

        [Test]
        public void SetValue_GivenSectionKeyAndValue_WhenSectionDoesExist_SetsValue()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var value1 = RandString();
            var value2 = RandString();
            var iniFile = new INIFile();
            //---------------Assert Precondition----------------
            Assert.IsFalse(iniFile.Sections.Keys.Any(s => s == section));

            //---------------Execute Test ----------------------
            iniFile.SetValue(section, key, value1);
            iniFile.SetValue(section, key, value2);

            //---------------Test Result -----------------------
            Assert.AreEqual(value2, iniFile.Sections[section][key]);
        }

        [Test]
        public void GetValue_GivenKnownSectionAndKey_ReturnsValue()
        {
            //---------------Set up test pack-------------------
            var section = RandString();
            var key = RandString();
            var value = RandString();
            var iniFile = new INIFile();
            iniFile.SetValue(section, key, value);

            //---------------Assert Precondition----------------
            Assert.AreEqual(value, iniFile.Sections[section][key]);
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
            var iniFile = new INIFile();
            iniFile.SetValue(section, key, value);
            var otherKey = key + RandString();
            //---------------Assert Precondition----------------
            Assert.IsFalse(iniFile.Sections[section].Keys.Contains(otherKey));
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
            var iniFile = new INIFile();
            iniFile.SetValue(section, key, value);
            var otherKey = key + RandString();
            var otherSection = section + RandString();
            //---------------Assert Precondition----------------
            Assert.IsFalse(iniFile.Sections.Keys.Contains(otherSection));
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
            var iniFile = new INIFile();
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
            var iniFile = new INIFile();
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
            var iniFile = new INIFile();
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
            var iniFile = new INIFile();
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
            var iniFile = new INIFile();
            iniFile.SetValue(section, key, value);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = iniFile.HasSetting(section, key + RandString());

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }
    }

}
