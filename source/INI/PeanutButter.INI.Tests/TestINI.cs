using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;
using NExpect;
using NExpect.Implementations;
using NExpect.Interfaces;
using NExpect.MatcherLogic;
using PeanutButter.INIFile;
// ReSharper disable PossibleNullReferenceException

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.INI.Tests
{
    [TestFixture]
    public class TestINIFile
    {
        [TestFixture]
        public class Construction
        {
            [Test]
            public void ShouldEstablishData()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var sut = Create() as INIFile_EXPOSES_Sections;

                //---------------Test Result -----------------------
                Assert.IsNotNull(sut.Data);
            }

            [Test]
            public void GivenFileName_ShouldLoadFile()
            {
                //---------------Set up test pack-------------------
                using (var tempFile = new AutoDeletingIniFile())
                {
                    var section = RandString();
                    var key = RandString();
                    var value = RandString();
                    var iniDataLines = new[] {"[" + section + "]", key + "=" + value};
                    tempFile.Write(iniDataLines);

                    //---------------Assert Precondition----------------
                    Expect(tempFile.Path).To.Be.A.File();

                    //---------------Execute Test ----------------------
                    var sut = Create(tempFile.Path) as INIFile_EXPOSES_Sections;

                    //---------------Test Result -----------------------
                    Expect(sut.Data).Not.To.Be.Null();
                    Expect(sut.Data).To.Contain.Key(section);
                    Expect(sut.Data[section]).To.Contain.Key(key)
                        .With.Value(value);
                }
            }

            [TestFixture]
            public class PathProperty
            {
                [Test]
                public void AfterLoadingFile_ShouldBePathToFile()
                {
                    // Arrange
                    using (var tempFile = new AutoDeletingIniFile())
                    {
                        // Pre-assert
                        var sut = Create(tempFile.Path);
                        // Act
                        var result = sut.Path;
                        // Assert
                        Expect(result).To.Equal(tempFile.Path);
                    }
                }

                [Test]
                public void WhenHaveNoLoadedFile_ShouldBeNull()
                {
                    // Arrange
                    var sut = new INIFile.INIFile();
                    // Pre-assert
                    // Act
                    var result = sut.Path;
                    // Assert
                    Expect(result).To.Be.Null();
                }

                [Test]
                public void AfterLoad_ShouldBeLoadedPath()
                {
                    // Arrange
                    using (var tempFile = new AutoDeletingIniFile())
                    {
                        var sut = new INIFile.INIFile();
                        // Pre-assert
                        Expect(sut.Path).To.Be.Null();
                        // Act
                        sut.Load(tempFile.Path);
                        // Assert
                        Expect(sut.Path).To.Equal(tempFile.Path);
                    }
                }
            }

            [TestFixture]
            public class MergingConfigurations
            {
                [Test]
                public void GivenInvalidPath_ShouldDoNothing()
                {
                    // Arrange
                    using (var original = new AutoDeletingIniFile())
                    using (var mergeFile = new AutoDeletingIniFile())
                    {
                        var section = GetRandomString(2);
                        var setting = GetRandomString(2);
                        var value = GetRandomString(2);
                        var sut = Create(original.Path);
                        sut.SetValue(section, setting, value);
                        File.Delete(mergeFile.Path);
                        // Pre-assert
                        Expect(mergeFile.Path).Not.To.Be.A.File();
                        // Act
                        sut.Merge(mergeFile.Path, MergeStrategies.AddIfMissing);
                        // Assert
                    }
                }

                [TestFixture]
                public class UsingGetValue
                {
                    [Test]
                    public void GivenValidPath_AndAddIfMissingStrategy_ShouldMergeInWhereNotAlreadyExisting()
                    {
                        // Arrange
                        var section1 = $"original-section-{GetRandomString(4)}";
                        var section2 = $"merged-section-{GetAnother(section1)}";
                        var setting1 = $"original-setting-{GetRandomString(4)}";
                        var value1 = $"original-value-{GetRandomString(4)}";
                        var setting2 = $"merged-setting-{GetRandomString(4)}";
                        var value2 = $"merged-value-{GetRandomString(4)}";
                        var sharedSection = $"shared-section-{GetAnother<string>(new[] {section1, section2})}";
                        var sharedSetting = $"shared-setting-{GetRandomString(4)}";
                        var originalSharedValue = $"original-shared-value-{GetRandomString(4)}";
                        var mergedSharedValue = $"merged-shared-value-{GetRandomString(4)}";

                        using (var original = new AutoDeletingIniFile())
                        using (var merge = new AutoDeletingIniFile())
                        {
                            var sut = Create(original.Path);
                            var other = Create(merge.Path);
                            sut.SetValue(section1, setting1, value1);
                            sut.SetValue(sharedSection, sharedSetting, originalSharedValue);
                            other.SetValue(section2, setting2, value2);
                            other.SetValue(sharedSection, sharedSetting, mergedSharedValue);
                            other.Persist();
                            // Pre-assert

                            // Act
                            sut.Merge(merge.Path, MergeStrategies.AddIfMissing);

                            // Assert
                            Expect(sut.GetValue(section1, setting1))
                                .To.Equal(value1, "Missing original setting");
                            Expect(sut.GetValue(section2, setting2))
                                .To.Equal(value2, "Missing distinct merged-in setting");
                            Expect(sut.GetValue(sharedSection, sharedSetting))
                                .To.Equal(originalSharedValue, "Should have original shared setting");
                        }
                    }

                    [Test]
                    public void GivenValidPath_AndOverrideStrategy_ShouldMergeInWhereNotAlreadyExisting()
                    {
                        // Arrange
                        var section1 = $"original-section-{GetRandomString(4)}";
                        var section2 = $"merged-section-{GetAnother(section1)}";
                        var setting1 = $"original-setting-{GetRandomString(4)}";
                        var value1 = $"original-value-{GetRandomString(4)}";
                        var setting2 = $"merged-setting-{GetRandomString(4)}";
                        var value2 = $"merged-value-{GetRandomString(4)}";
                        var sharedSection = $"shared-section-{GetAnother<string>(new[] {section1, section2})}";
                        var sharedSetting = $"shared-setting-{GetRandomString(4)}";
                        var originalSharedValue = $"original-shared-value-{GetRandomString(4)}";
                        var mergedSharedValue = $"merged-shared-value-{GetRandomString(4)}";

                        using (var original = new AutoDeletingIniFile())
                        using (var merge = new AutoDeletingIniFile())
                        {
                            var sut = Create(original.Path);
                            var other = Create(merge.Path);
                            sut.SetValue(section1, setting1, value1);
                            sut.SetValue(sharedSection, sharedSetting, originalSharedValue);
                            other.SetValue(section2, setting2, value2);
                            other.SetValue(sharedSection, sharedSetting, mergedSharedValue);
                            other.Persist();
                            // Pre-assert

                            // Act
                            sut.Merge(merge.Path, MergeStrategies.Override);

                            // Assert
                            Expect(sut.GetValue(section1, setting1))
                                .To.Equal(value1, "Missing original setting");
                            Expect(sut.GetValue(section2, setting2))
                                .To.Equal(value2, "Missing distinct merged-in setting");
                            Expect(sut.GetValue(sharedSection, sharedSetting))
                                .To.Equal(mergedSharedValue, "Should have original shared setting");
                        }
                    }
                }                
                
                [TestFixture]
                public class UsingIndexing
                {
                    [Test]
                    public void GivenValidPath_AndAddIfMissingStrategy_ShouldMergeInWhereNotAlreadyExisting()
                    {
                        // Arrange
                        var section1 = $"original-section-{GetRandomString(4)}";
                        var section2 = $"merged-section-{GetAnother(section1)}";
                        var setting1 = $"original-setting-{GetRandomString(4)}";
                        var value1 = $"original-value-{GetRandomString(4)}";
                        var setting2 = $"merged-setting-{GetRandomString(4)}";
                        var value2 = $"merged-value-{GetRandomString(4)}";
                        var sharedSection = $"shared-section-{GetAnother<string>(new[] {section1, section2})}";
                        var sharedSetting = $"shared-setting-{GetRandomString(4)}";
                        var originalSharedValue = $"original-shared-value-{GetRandomString(4)}";
                        var mergedSharedValue = $"merged-shared-value-{GetRandomString(4)}";

                        using (var original = new AutoDeletingIniFile())
                        using (var merge = new AutoDeletingIniFile())
                        {
                            var sut = Create(original.Path);
                            var other = Create(merge.Path);
                            sut[section1][setting1] = value1;
                            sut[sharedSection][sharedSetting] = originalSharedValue;
                            other[section2][setting2] = value2;
                            other[sharedSection][sharedSetting] = mergedSharedValue;
                            other.Persist();
                            // Pre-assert

                            // Act
                            sut.Merge(merge.Path, MergeStrategies.AddIfMissing);

                            // Assert
                            Expect(sut[section1][setting1])
                                .To.Equal(value1, "Missing original setting");
                            Expect(sut[section2][setting2])
                                .To.Equal(value2, "Missing distinct merged-in setting");
                            Expect(sut[sharedSection][sharedSetting])
                                .To.Equal(originalSharedValue, "Should have original shared setting");
                        }
                    }

                    [Test]
                    public void GivenValidPath_AndOverrideStrategy_ShouldMergeInWhereNotAlreadyExisting()
                    {
                        // Arrange
                        var section1 = $"original-section-{GetRandomString(4)}";
                        var section2 = $"merged-section-{GetAnother(section1)}";
                        var setting1 = $"original-setting-{GetRandomString(4)}";
                        var value1 = $"original-value-{GetRandomString(4)}";
                        var setting2 = $"merged-setting-{GetRandomString(4)}";
                        var value2 = $"merged-value-{GetRandomString(4)}";
                        var sharedSection = $"shared-section-{GetAnother<string>(new[] {section1, section2})}";
                        var sharedSetting = $"shared-setting-{GetRandomString(4)}";
                        var originalSharedValue = $"original-shared-value-{GetRandomString(4)}";
                        var mergedSharedValue = $"merged-shared-value-{GetRandomString(4)}";

                        using (var original = new AutoDeletingIniFile())
                        using (var merge = new AutoDeletingIniFile())
                        {
                            var sut = Create(original.Path);
                            var other = Create(merge.Path);
                            sut[section1][setting1] = value1;
                            sut[sharedSection][sharedSetting] = originalSharedValue;
                            other[section2][setting2] =value2;
                            other[sharedSection][sharedSetting] = mergedSharedValue;
                            other.Persist();
                            // Pre-assert

                            // Act
                            sut.Merge(merge.Path, MergeStrategies.Override);

                            // Assert
                            Expect(sut[section1][setting1])
                                .To.Equal(value1, "Missing original setting");
                            Expect(sut[section2][setting2])
                                .To.Equal(value2, "Missing distinct merged-in setting");
                            Expect(sut[sharedSection][sharedSetting])
                                .To.Equal(mergedSharedValue, "Should have original shared setting");
                        }
                    }
                }
            }

            [TestFixture]
            public class SavingMergedConfigurations
            {
                [Test]
                public void DefaultPersist_ShouldPersistToOriginalFileWithoutMergedConfigurations()
                {
                    // Arrange
                    var originalContents = @"
[original]
setting=value";
                    var mergeContents = @"
[original]
moo=cow1
[merged]
otherSetting=otherValue";
                    using (var original = new AutoDeletingIniFile(originalContents))
                    using (var merge = new AutoDeletingIniFile(mergeContents))
                    {
                        // Pre-assert
                        var sut = Create(original.Path);
                        sut.Merge(merge.Path, MergeStrategies.Override);
                        // Act
                        sut.Persist();
                        // Assert
                        var result = Create(original.Path);
                        Expect(result).Not.To.Have.Section("merged");
                        Expect(result).To.Have.Setting("original", "setting", "value");
                    }
                }

                [Test]
                public void DefaultPersist_WhenHaveOverriddenMergeValue_ShouldPersistOverriddenValue()
                {
                    // Arrange
                    var originalContents = @"
[original]
setting=value";
                    var mergeContents = @"
[merged]
otherSetting=otherValue";
                    using (var original = new AutoDeletingIniFile(originalContents))
                    using (var merge = new AutoDeletingIniFile(mergeContents))
                    {
                        // Pre-assert
                        var sut = Create(original.Path);
                        sut.Merge(merge.Path, MergeStrategies.Override);
                        var expected = GetRandomString(5);
                        // Act
                        sut["merged"]["otherSetting"] = expected;
                        sut.Persist();
                        // Assert
                        var result = Create(original.Path);
                        Expect(result).To.Have.Section("merged");
                    }
                }
            }

            [Test]
            public void GivenFileNameWhichDoesntExist_ShouldNotThrow()
            {
                //---------------Set up test pack-------------------
                using (var tempFile = new AutoDeletingIniFile())
                {
                    File.Delete(tempFile.Path);
                    //---------------Assert Precondition----------------
                    Expect(tempFile.Path).Not.To.Be.A.File();
                    //---------------Execute Test ----------------------
                    var sut = Create(tempFile.Path) as INIFile_EXPOSES_Sections;

                    //---------------Test Result -----------------------
                    Expect(sut.Data).Not.To.Be.Null();
                    Expect(sut.Data.Keys.Any(k => k != ""))
                        .To.Be.False();
                }
            }

            [Test]
            public void WhenGivenPathButFileDoesntExist_ShouldCreateFile()
            {
                //---------------Set up test pack-------------------
                using (var tempFile = new AutoDeletingIniFile())
                {
                    File.Delete(tempFile.Path);
                    //---------------Assert Precondition----------------
                    Expect(tempFile.Path).Not.To.Be.A.File();

                    //---------------Execute Test ----------------------
                    Create(tempFile.Path);

                    //---------------Test Result -----------------------
                    Expect(tempFile.Path).To.Be.A.File();
                }
            }

            [Test]
            public void WhenSourceContainsComments_ShouldIgnoreCommentedParts()
            {
                using (var tempFile = new AutoDeletingIniFile())
                {
                    //---------------Set up test pack-------------------
                    var src = new[]
                    {
                        "; created by some generator",
                        "[general] ; general settings go here",
                        ";this line should be ignored",
                        "key=value ; this part of the line should be ignored"
                    };
                    File.WriteAllBytes(
                        tempFile.Path,
                        Encoding.UTF8.GetBytes(
                            string.Join(Environment.NewLine, src)
                        ));

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var ini = Create(tempFile.Path) as INIFile_EXPOSES_Sections;

                    //---------------Test Result -----------------------
                    Expect(ini.Data).To.Contain.Only(1).Item();
                    Expect(ini["general"]).To.Contain.Only(1).Item();
                    Expect(ini["GeNeRal"]).To.Contain.Only(1).Item();
                    Expect(ini["general"]).To.Contain.Key("key").With.Value("value");
                }
            }
        }

        [TestFixture]
        public class ParsingFiles
        {
            [Test]
            public void WhenValueIsQuoted_ShouldSetDataValueUnQuoted()
            {
                //---------------Set up test pack-------------------
                using (var tempFile = new AutoDeletingIniFile())
                {
                    var section = RandString();
                    var key = RandString();
                    var value = RandString();
                    var iniDataLines = new[] {"[" + section + "]", key + "=\"" + value + "\""};
                    tempFile.Write(iniDataLines);

                    //---------------Assert Precondition----------------
                    Expect(tempFile.Path).To.Be.A.File();

                    //---------------Execute Test ----------------------
                    var sut = Create(tempFile.Path) as INIFile_EXPOSES_Sections;

                    //---------------Test Result -----------------------
                    Expect(sut.Data).Not.To.Be.Null();
                    Expect(sut.Data).To.Contain.Key(section);
                    Expect(sut.Data[section]).To.Contain.Key(key)
                        .With.Value(value);
                }
            }

            [Test]
            public void GivenStringContents_WhenSourceContainsComments_ShouldIgnoreCommentedParts()
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
                var ini = Create() as INIFile_EXPOSES_Sections;
                ini.Parse(string.Join(Environment.NewLine, src));

                //---------------Test Result -----------------------

                Expect(ini.Data).To.Contain.Only(1).Item();
                Expect(ini["general"]).To.Contain.Only(2).Items();
                Expect(ini["General"]).To.Contain.Only(2).Items();
                Expect(ini["general"])
                    .To.Contain.Key("key")
                    .With.Value("value");
                Expect(ini["general"])
                    .To.Contain.Key("something")
                    .With.Value("");
            }

            [Test]
            public void ShouldLoadMultipleSectionsAndKeys_WithCaseInsensitivityForSectionNamesAndSettingNames()
            {
                using (var tempFile = new AutoDeletingIniFile())
                {
                    //---------------Set up test pack-------------------
                    var lines = new[]
                    {
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
                    var sut = Create(tempFile.Path) as INIFile_EXPOSES_Sections;

                    //---------------Test Result -----------------------

                    Expect(sut.Data["Section1"])
                        .To.Contain.Key("path")
                        .With.Value("C:\\tmp\\something.txt");
                    Expect(sut.Data["Section1"])
                        .To.Contain.Key("color")
                        .With.Value("Red");

                    Expect(sut.Data["section2"])
                        .To.Contain.Key("number")
                        .With.Value("12");
                    // NExpect doesn't know (yet) how to tell that the 
                    Expect(sut.Data["sectioN2"]["NUMber"])
                        .To.Equal("12");
                    Expect(sut.Data["Section2"])
                        .To.Contain.Key("Season")
                        .With.Value("Green");
                }
            }

            [Test]
            public void SectionWithKeysOnly_ShouldHaveKeysAndNullValues()
            {
                //---------------Set up test pack-------------------
                var key1 = GetRandomString();
                var key2 = GetRandomString();
                var src = new[]
                {
                    "[section]",
                    key1,
                    key2
                };

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var sut = Create();
                sut.Parse(src.AsText());

                //---------------Test Result -----------------------
                var data = sut["section"];

                Expect(data).To.Contain.Key(key1)
                    .With.Value(null);
                Expect(data).To.Contain.Key(key2)
                    .With.Value(null);
            }

            [Test]
            public void SectionWithKeysOnly_WhenPersisting_ShouldNotAddTrailingEqualitySign()
            {
                //---------------Set up test pack-------------------
                var key1 = GetRandomString();
                var key2 = GetRandomString();
                var src = new[]
                {
                    "[section]",
                    key1,
                    key2
                };

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var sut = Create();
                sut.Parse(src.AsText());
                using (var tempFile = new AutoTempFile())
                {
                    sut.Persist(tempFile.Path);

                    //---------------Test Result -----------------------
                    var outputData = File.ReadAllLines(tempFile.Path);

                    Expect(outputData).To.Equal(
                        new[]
                        {
                            "[section]",
                            key1,
                            key2
                        });
                }
            }
        }

        [TestFixture]
        public class ManipulatingInMemoryData
        {
            [Test]
            public void ShouldBeAbleToAddSections()
            {
                //---------------Set up test pack-------------------
                var sut = Create();
                var section = RandString();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                sut.AddSection(section);
                //---------------Test Result -----------------------
                Expect(sut).To.Have.Section(section);
            }

            [Test]
            public void ShouldIgnoreNullSection()
            {
                // Arrange
                var sut = Create() as INIFile_EXPOSES_Sections;
                // Pre-assert
                // Act
                Expect(() => sut.AddSection(null))
                    .Not.To.Throw();
                // Assert
                Expect(sut.Data).To.Be.Empty();
            }
        }

        [TestFixture]
        public class Persistence
        {
            [Test]
            public void GivenFileName_ShouldWriteINIDataToFile()
            {
                //---------------Set up test pack-------------------
                using (var tempFile = new AutoDeletingIniFile())
                {
                    //---------------Assert Precondition----------------
                    var writer = Create(tempFile.Path);
                    var section = RandString();
                    var key = RandString();
                    var value = RandString();
                    writer.SetValue(section, key, value);

                    //---------------Execute Test ----------------------
                    writer.Persist(tempFile.Path);
                    var reader = Create(tempFile.Path);

                    //---------------Test Result -----------------------
                    Expect(reader[section])
                        .To.Contain.Key(key)
                        .With.Value(value);
                }
            }

            [Test]
            public void GivenNoFileName_ShouldWriteINIDataToOriginalFile()
            {
                //---------------Set up test pack-------------------
                using (var tempFile = new AutoDeletingIniFile())
                {
                    //---------------Assert Precondition----------------
                    var writer = Create(tempFile.Path);
                    var section = RandString();
                    var key = RandString();
                    var value = RandString();
                    writer.AddSection(section);
                    writer[section][key] = value;

                    //---------------Execute Test ----------------------
                    writer.Persist();
                    var reader = Create(tempFile.Path);

                    //---------------Test Result -----------------------
                    Expect(reader[section])
                        .To.Contain.Key(key)
                        .With.Value(value);
                }
            }

            [Test]
            public void GivenFileName_ShouldPersistTheGlobalEmptySection()
            {
                //---------------Set up test pack-------------------
                using (var tempFile = new AutoDeletingIniFile())
                {
                    //---------------Assert Precondition----------------
                    var writer = Create(tempFile.Path);
                    var section = "";
                    var key = RandString();
                    var value = RandString();
                    writer.AddSection(section);
                    writer[section][key] = value;

                    //---------------Execute Test ----------------------
                    writer.Persist();
                    var reader = Create(tempFile.Path);

                    //---------------Test Result -----------------------
                    Expect(reader[section])
                        .To.Contain.Key(key)
                        .With.Value(value);
                }
            }

            [Test]
            public void GivenNoFileName_WhenNoFileNameSpecifiedInConstructor_ShouldThrowException()
            {
                //---------------Set up test pack-------------------
                //---------------Assert Precondition----------------
                var writer = Create();
                var section = RandString();
                var key = RandString();
                var value = RandString();
                writer.AddSection(section);
                writer[section][key] = value;

                //---------------Execute Test ----------------------
                Expect(() => writer.Persist())
                    .To.Throw<ArgumentException>()
                    .With.Message.Containing(
                        "No path specified to persist to and INIFile instantiated without an auto-path"
                    );

                //---------------Test Result -----------------------
            }

            [Test]
            public void Persist_GivenStream_ShouldWriteOutToStream()
            {
                //---------------Set up test pack-------------------
                var sut = Create();
                sut.AddSection("general");
                sut["general"]["foo"] = "bar";

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using (var memStream = new MemoryStream(new byte[1024], true))
                {
                    sut.Persist(memStream);
                    //---------------Test Result -----------------------
                    var resultBytes = memStream.ReadAllBytes();
                    var result = Encoding.UTF8.GetString(resultBytes);
                    var firstNull = result.IndexOf('\0');
                    result = result.Substring(0, firstNull);
                    var newIni = Create();
                    newIni.Parse(result);
                    Expect(newIni["general"])
                        .To.Contain.Key("foo")
                        .With.Value("bar");
                }
            }
        }

        [TestFixture]
        public class SettingValuesViaMethod
        {
            [Test]
            public void GivenSectionKeyAndValue_WhenSectionDoesNotExist_CreatesItAndSetsValue()
            {
                //---------------Set up test pack-------------------
                var section = RandString();
                var key = RandString();
                var value = RandString();
                var sut = Create();
                //---------------Assert Precondition----------------
                Expect(sut).Not.To.Have.Section(section);

                //---------------Execute Test ----------------------
                sut.SetValue(section, key, value);

                //---------------Test Result -----------------------
                Expect(sut[section])
                    .To.Contain.Key(key)
                    .With.Value(value);
            }

            [Test]
            public void GivenSectionKeyAndValue_WhenSectionDoesExist_SetsValue()
            {
                //---------------Set up test pack-------------------
                var section = RandString();
                var key = RandString();
                var value1 = RandString();
                var value2 = RandString();
                var sut = Create() as INIFile_EXPOSES_Sections;
                //---------------Assert Precondition----------------
                Assert.IsFalse(sut.Data.Keys.Any(s => s == section));

                //---------------Execute Test ----------------------
                sut.SetValue(section, key, value1);
                sut.SetValue(section, key, value2);

                //---------------Test Result -----------------------
                Expect(sut.Data[section])
                    .To.Contain.Key(key)
                    .With.Value(value2);
            }
        }

        [TestFixture]
        public class GettingValuesViaMethod
        {
            [Test]
            public void GivenKnownSectionAndKey_ReturnsValue()
            {
                //---------------Set up test pack-------------------
                var section = RandString();
                var key = RandString();
                var value = RandString();
                var sut = Create();
                sut.SetValue(section, key, value);

                //---------------Assert Precondition----------------
                Expect(sut[section]).To.Contain.Key(key)
                    .With.Value(value);
                //---------------Execute Test ----------------------
                var result = sut.GetValue(section, key);
                //---------------Test Result -----------------------
                Expect(result).To.Equal(value);
            }

            [Test]
            public void GivenKnownSectionWithUnknownKeyAndDefault_ShouldReturnDefaultValue()
            {
                //---------------Set up test pack-------------------
                var section = RandString();
                var key = RandString();
                var value = RandString();
                var defaultValue = RandString();
                var sut = Create();
                sut.SetValue(section, key, value);
                var otherKey = key + RandString();
                //---------------Assert Precondition----------------
                Assert.IsFalse(sut[section].Keys.Contains(otherKey));
                //---------------Execute Test ----------------------
                var result = sut.GetValue(section, otherKey, defaultValue);
                //---------------Test Result -----------------------
                Expect(result).To.Equal(defaultValue);
            }

            [Test]
            public void GivenUnKnownSectionWithUnknownKeyAndDefault_ShouldReturnDefaultValue()
            {
                //---------------Set up test pack-------------------
                var section = RandString();
                var key = RandString();
                var value = RandString();
                var defaultValue = RandString();
                var sut = Create() as INIFile_EXPOSES_Sections;
                sut.SetValue(section, key, value);
                var otherKey = key + RandString();
                var otherSection = section + RandString();
                //---------------Assert Precondition----------------
                Assert.IsFalse(sut.Data.Keys.Contains(otherSection));
                //---------------Execute Test ----------------------
                var result = sut.GetValue(otherSection, otherKey, defaultValue);
                //---------------Test Result -----------------------
                Expect(result).To.Equal(defaultValue);
            }
        }

        [TestFixture]
        public class TestingForSection
        {
            [Test]
            public void GivenNameOfExistingSection_ShouldReturnTrue()
            {
                //---------------Set up test pack-------------------
                var section = RandString();
                var sut = Create();
                sut.AddSection(section);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.HasSection(section);

                //---------------Test Result -----------------------
                Expect(result).To.Be.True();
            }

            [Test]
            public void GivenNameOfNonExistingSection_ShouldReturnTrue()
            {
                //---------------Set up test pack-------------------
                var section = RandString();
                var sut = Create();
                sut.AddSection(section);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.HasSection(section + RandString());

                //---------------Test Result -----------------------
                Expect(result).To.Be.False();
            }
        }

        [TestFixture]
        public class TestingForSetting
        {
            [Test]
            public void GivenNameOfExistingSetting_ShouldReturnTrue()
            {
                //---------------Set up test pack-------------------
                var section = RandString();
                var key = RandString();
                var value = RandString();
                var sut = Create();
                sut.SetValue(section, key, value);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.HasSetting(section, key);

                //---------------Test Result -----------------------
                Expect(result).To.Be.True();
            }

            [Test]
            public void GivenNameOfNonExistingSection_ShouldReturnFalse()
            {
                //---------------Set up test pack-------------------
                var section = RandString();
                var key = RandString();
                var value = RandString();
                var sut = Create();
                sut.SetValue(section, key, value);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.HasSetting(section + RandString(), key);

                //---------------Test Result -----------------------
                Expect(result).To.Be.False();
            }

            [Test]
            public void GivenNameOfNonExistingSectting_ShouldReturnFalse()
            {
                //---------------Set up test pack-------------------
                var section = RandString();
                var key = RandString();
                var value = RandString();
                var sut = Create();
                sut.SetValue(section, key, value);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.HasSetting(section, key + RandString());

                //---------------Test Result -----------------------
                Expect(result).To.Be.False();
            }
        }


        [TestFixture]
        public class GettingValuesViaIndexers
        {
            [Test]
            public void WhenSectionDoesntExist_ShouldCreateSection()
            {
                //---------------Set up test pack-------------------
                var section = GetRandomString();
                var key = GetRandomString();
                var value = GetRandomString();

                //---------------Assert Precondition----------------
                using (var tempFile = new AutoDeletingIniFile())
                {
                    var ini = new INIFile.INIFile(tempFile.Path);
                    ini[section][key] = value;
                    ini.Persist();

                    //---------------Execute Test ----------------------
                    var ini2 = new INIFile.INIFile(tempFile.Path);
                    var result = ini2[section][key];

                    //---------------Test Result -----------------------
                    Expect(result).To.Equal(value);
                }
            }
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
            Expect(ini.Sections).To.Equal(
                new[]
                {
                    "general",
                    "section1",
                    "section2"
                });
        }

        [TestFixture]
        public class ShouldRetainCommentsWhenPersisting
        {
            [Test]
            public void ShouldRetainCommentsAboveSetting()
            {
                //---------------Set up test pack-------------------
                var input =
                    @"
[general]
; this is the general section
foo=bar
";
                var sut = Create();
                sut.Parse(input);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using (var memStream = new MemoryStream(new byte[1024], true))
                {
                    //---------------Test Result -----------------------
                    sut.Persist(memStream);
                    var lines = memStream.AsString().Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                    Expect(lines).To.Contain.Exactly(1)
                        .Equal.To("; this is the general section");
                }
            }

            [Test]
            public void ShouldRetainMultilineCommentsAboveSetting()
            {
                //---------------Set up test pack-------------------
                var input =
                    @"
[general]
; this is the general section
; this is the general section again!
foo=bar
";
                var sut = Create();
                sut.Parse(input);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using (var memStream = new MemoryStream(new byte[1024], true))
                {
                    //---------------Test Result -----------------------
                    sut.Persist(memStream);
                    var lines = memStream.AsString().Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                    Expect(lines).To.Contain.Exactly(1)
                        .Equal.To("; this is the general section");
                    Expect(lines).To.Contain.Exactly(1)
                        .Equal.To("; this is the general section again!");
                }
            }

            [Test]
            public void ShouldRetainCommentsAboveSection()
            {
                //---------------Set up test pack-------------------
                var input =
                    @"
; this is the general section
[general]
foo=bar
";
                var sut = Create();
                sut.Parse(input);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using (var memStream = new MemoryStream(new byte[1024], true))
                {
                    //---------------Test Result -----------------------
                    sut.Persist(memStream);
                    var lines = memStream.AsString().Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                    Expect(lines).To.Contain.Exactly(1)
                        .Equal.To("; this is the general section");
                }
            }


            [Test]
            public void ShouldRetainMultiLineCommentsAboveSection()
            {
                //---------------Set up test pack-------------------
                var input =
                    @"
; this is the general section
; this is the general section again!
[general]
foo=bar
";
                var sut = Create();
                sut.Parse(input);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using (var memStream = new MemoryStream(new byte[1024], true))
                {
                    //---------------Test Result -----------------------
                    sut.Persist(memStream);
                    var lines = memStream.AsString().Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                    Assert.IsTrue(lines.Any(l => l == "; this is the general section"));
                    Assert.IsTrue(lines.Any(l => l == "; this is the general section again!"));
                }
            }
        }

        [TestFixture]
        public class RemovingSections
        {
            [Test]
            public void WhenHaveNoSections_ShouldNotThrow()
            {
                // Arrange
                var sut = Create();
                var section = GetRandomString(2);
                // Pre-assert
                Expect(sut.Sections).To.Be.Empty();
                // Act
                Expect(() => sut.RemoveSection(section))
                    .Not.To.Throw();
                // Assert
            }

            [Test]
            public void ShouldNotThrowWhenAttemptingToRemoveUnknownSection()
            {
                // Arrange
                var sut = Create();
                var existing = GetRandomString(2);
                sut.AddSection(existing);
                var toRemove = GetAnother(existing);
                // Pre-assert
                // Act
                Expect(() => sut.RemoveSection(toRemove))
                    .Not.To.Throw();
                // Assert
            }

            [Test]
            public void ShouldIgnoreNullInput()
            {
                // Arrange
                var sut = Create();
                var existing = GetRandomString(2);
                sut.AddSection(existing);
                // Pre-assert
                // Act
                Expect(() => sut.RemoveSection(null))
                    .Not.To.Throw();
                // Assert
            }

            [Test]
            public void ShouldRemoveTheSectionWhenFoundByName()
            {
                // Arrange
                var sut = Create();
                var section = GetRandomString(4);
                sut.AddSection(section);
                sut.SetValue(section, GetRandomString(2), GetRandomString(2));
                // Pre-assert
                // Act
                Expect(() => sut.RemoveSection(section))
                    .Not.To.Throw();
                // Assert
                Expect(sut).Not.To.Have.Section(section);
            }
        }

        [TestFixture]
        public class Reloading
        {
            [Test]
            public void WhenFileOnDiskHasChanged_ShouldReflectChanges()
            {
                // Arrange
                using (var tempFile = new AutoDeletingIniFile())
                {
                    var initialContents = @"[section]
key=value";
                    var updatedContents = @"[section]
key=value2";
                    tempFile.Write(initialContents);
                    var sut = Create(tempFile.Path);
                    // Pre-assert
                    // Act
                    tempFile.Write(updatedContents);    
                    sut.Reload();
                    // Assert
                    Expect(sut["section"]["key"]).To.Equal("value2");
                }
            }

            [Test]
            public void WhenMergedFileOnDiskChanged_ShouldReflectChanges()
            {
                // Arrange
                using (var mainFile = new AutoDeletingIniFile())
                using (var mergeFile = new AutoDeletingIniFile())
                {
                    mainFile.Write("[section1]\nkey1=value1");
                    mergeFile.Write("[section2]\nkey2=value2");
                    var sut = Create(mainFile.Path);
                    sut.Merge(mergeFile.Path, MergeStrategies.AddIfMissing);
                    // Pre-assert
                    Expect(sut["section2"]["key2"]).To.Equal("value2");
                    // Act
                    mergeFile.Write("[section2]\nkey2=value3");
                    sut.Reload();
                    // Assert
                    Expect(sut["section2"]["key2"]).To.Equal("value3");
                }
            }
        }

        private static string RandString()
        {
            return GetRandomAlphaString(1, 10);
        }

        private static IINIFile Create(string path = null)
        {
            return new INIFile_EXPOSES_Sections(path);
        }

        private class INIFile_EXPOSES_Sections : INIFile.INIFile
        {
            public INIFile_EXPOSES_Sections(string path) : base(path)
            {
            }

            public new Dictionary<string, IDictionary<string, string>> Data => base.Data;
        }

        public class AutoDeletingIniFile : AutoDeletingTempFile
        {
            public AutoDeletingIniFile() : base(".ini")
            {
            }

            public AutoDeletingIniFile(string contents) : this()
            {
                Write(contents);
            }
        }

        public class AutoDeletingTempFile : IDisposable
        {
            public string Path => _tempFile;
            private readonly string _tempFile;

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
                catch
                {
                    // ignored
                }
            }
        }
    }

    public static class Matchers
    {
        public static void File(this IA<string> to)
        {
            to.AddMatcher(
                actual =>
                {
                    var passed = System.IO.File.Exists(actual);
                    return new MatcherResult(
                        passed,
                        $"Expected '{actual}' {passed.AsNot()}to be a file"
                    );
                });
        }

        public static void Setting(
            this IHave<IINIFile> have,
            string section,
            string setting,
            string value
        )
        {
            have.Compose(actual =>
            {
                Expect(actual).To.Have.Setting(section, setting);
                Expect(actual[section][setting])
                    .To.Equal(value, () => $"Expected '[{section}]'/'{setting}' to equal '{value}'");
            });
        }

        public static void Setting(
            this IHave<IINIFile> have,
            string section,
            string setting)
        {
            have.AddMatcher(actual =>
            {
                var passed = actual.HasSetting(section, setting);
                return new MatcherResult(
                    passed,
                    () => $"Expected {passed.AsNot()}to have setting '{section}'/'{setting}'"
                );
            });
        }

        public static void Section(this IHave<IINIFile> have, string section)
        {
            have.AddMatcher(actual =>
            {
                var passed = actual.HasSection(section);
                return new MatcherResult(
                    passed,
                    () => $"Expected {passed.AsNot()}to have section '{section}'"
                );
            });
        }
    }
}