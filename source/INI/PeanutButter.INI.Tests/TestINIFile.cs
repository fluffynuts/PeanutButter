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
using NSubstitute;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable MemberHidesStaticFromOuterClass
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
                using var tempFile = new AutoDeletingIniFile();
                var section = RandString();
                var key = RandString();
                var value = RandString();
                var iniDataLines = new[] {"[" + section + "]", key + "=" + value};
                tempFile.Write(iniDataLines);

                //---------------Assert Precondition----------------
                FileSystemMatchers.File(Expect(tempFile.Path).To.Be.A);

                //---------------Execute Test ----------------------
                var sut = Create(tempFile.Path) as INIFile_EXPOSES_Sections;

                //---------------Test Result -----------------------
                Expect(sut.Data).Not.To.Be.Null();
                Expect(sut.Data).To.Contain.Key(section);
                Expect(sut.Data[section]).To.Contain.Key(key)
                    .With.Value(value);
            }

            [TestFixture]
            public class PathProperty
            {
                [Test]
                public void AfterLoadingFile_ShouldBePathToFile()
                {
                    // Arrange
                    using var tempFile = new AutoDeletingIniFile();
                    // Pre-assert
                    var sut = Create(tempFile.Path);
                    // Act
                    var result = sut.Path;
                    // Assert
                    Expect(result).To.Equal(tempFile.Path);
                }

                [Test]
                public void WhenHaveNoLoadedFile_ShouldBeNull()
                {
                    // Arrange
                    var sut = new INIFile();
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
                    using var tempFile = new AutoDeletingIniFile();
                    var sut = new INIFile();
                    // Pre-assert
                    Expect(sut.Path).To.Be.Null();
                    // Act
                    sut.Load(tempFile.Path);
                    // Assert
                    Expect(sut.Path).To.Equal(tempFile.Path);
                }
            }

            [TestFixture]
            public class MergingConfigurations
            {
                [Test]
                public void GivenInvalidPath_ShouldDoNothing()
                {
                    // Arrange
                    using var original = new AutoDeletingIniFile();
                    using var mergeFile = new AutoDeletingIniFile();
                    var section = GetRandomString(2);
                    var setting = GetRandomString(2);
                    var value = GetRandomString(2);
                    var sut = Create(original.Path);
                    sut.SetValue(section, setting, value);
                    File.Delete(mergeFile.Path);
                    // Pre-assert
                    FileSystemMatchers.File(Expect(mergeFile.Path).Not.To.Be.A);
                    // Act
                    sut.Merge(mergeFile.Path, MergeStrategies.OnlyAddIfMissing);
                    // Assert
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

                        using var original = new AutoDeletingIniFile();
                        using var merge = new AutoDeletingIniFile();
                        var sut = Create(original.Path);
                        var other = Create(merge.Path);
                        sut.SetValue(section1, setting1, value1);
                        sut.SetValue(sharedSection, sharedSetting, originalSharedValue);
                        other.SetValue(section2, setting2, value2);
                        other.SetValue(sharedSection, sharedSetting, mergedSharedValue);
                        other.Persist();
                        // Pre-assert

                        // Act
                        sut.Merge(merge.Path, MergeStrategies.OnlyAddIfMissing);

                        // Assert
                        Expect(sut.GetValue(section1, setting1))
                            .To.Equal(value1, "Missing original setting");
                        Expect(sut.GetValue(section2, setting2))
                            .To.Equal(value2, "Missing distinct merged-in setting");
                        Expect(sut.GetValue(sharedSection, sharedSetting))
                            .To.Equal(originalSharedValue, "Should have original shared setting");
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

                        using var original = new AutoDeletingIniFile();
                        using var merge = new AutoDeletingIniFile();
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

                        using var original = new AutoDeletingIniFile();
                        using var merge = new AutoDeletingIniFile();
                        var sut = Create(original.Path);
                        var other = Create(merge.Path);
                        sut[section1][setting1] = value1;
                        sut[sharedSection][sharedSetting] = originalSharedValue;
                        other[section2][setting2] = value2;
                        other[sharedSection][sharedSetting] = mergedSharedValue;
                        other.Persist();
                        // Pre-assert

                        // Act
                        sut.Merge(merge.Path, MergeStrategies.OnlyAddIfMissing);

                        // Assert
                        Expect(sut[section1][setting1])
                            .To.Equal(value1, "Missing original setting");
                        Expect(sut[section2][setting2])
                            .To.Equal(value2, "Missing distinct merged-in setting");
                        Expect(sut[sharedSection][sharedSetting])
                            .To.Equal(originalSharedValue, "Should have original shared setting");
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

                        using var original = new AutoDeletingIniFile();
                        using var merge = new AutoDeletingIniFile();
                        var sut = Create(original.Path);
                        var other = Create(merge.Path);
                        sut[section1][setting1] = value1;
                        sut[sharedSection][sharedSetting] = originalSharedValue;
                        other[section2][setting2] = value2;
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

                [TestFixture]
                public class Persistence
                {
                    [Test]
                    public void GivenExcludeMergedConfigurations_ShouldNotPersistMergedConfig()
                    {
                        // Arrange
                        using var original = new AutoDeletingIniFile();
                        using var merge = new AutoDeletingIniFile();
                        original.Write("[section]\nkey=value");
                        merge.Write("[merge]\nmerge_key=merge_value");
                        var sut = Create(original);
                        sut.Merge(merge.Path, MergeStrategies.OnlyAddIfMissing);
                        // Pre-assert
                        // Act
                        sut["section"]["key"] = "new_value";
                        sut.Persist();
                        // Assert
                        var other = Create(original);
                        Expect(other.Sections).Not.To.Contain("merge");
                        Expect(other["section"]["key"]).To.Equal("new_value");
                    }

                    [Test]
                    public void GivenIncludeMergedConfigurations_WhenOnlyAddMerge_ShouldPersistEntireConfig()
                    {
                        // Arrange
                        using var original = new AutoDeletingIniFile();
                        using var merge = new AutoDeletingIniFile();
                        original.Write("[section]\nkey=value");
                        merge.Write("[section]\nkey=other_value\n[merge]\nmerge_key=merge_value");
                        var sut = Create(original);
                        sut.Merge(merge.Path, MergeStrategies.OnlyAddIfMissing);
                        // Pre-assert
                        // Act
                        sut.Persist(PersistStrategies.IncludeMergedConfigurations);
                        // Assert
                        var other = Create(original);
                        Expect(other["section"]["key"]).To.Equal("value");
                        Expect(other.Sections).To.Contain("merge");
                        Expect(other["merge"]).To.Contain.Key("merge_key")
                            .With.Value("merge_value");
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
                    using var original = new AutoDeletingIniFile(originalContents);
                    using var merge = new AutoDeletingIniFile(mergeContents);
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
            public void GivenFileNameWhichDoesntExist_ShouldNotThrow()
            {
                //---------------Set up test pack-------------------
                using var tempFile = new AutoDeletingIniFile();
                File.Delete(tempFile.Path);
                //---------------Assert Precondition----------------
                FileSystemMatchers.File(Expect(tempFile.Path).Not.To.Be.A);
                //---------------Execute Test ----------------------
                var sut = Create(tempFile.Path) as INIFile_EXPOSES_Sections;

                //---------------Test Result -----------------------
                Expect(sut.Data).Not.To.Be.Null();
                Expect(sut.Data.Keys.Any(k => k != ""))
                    .To.Be.False();
            }

            [Test]
            public void WhenGivenPathButFileDoesntExist_ShouldCreateFile()
            {
                //---------------Set up test pack-------------------
                using var tempFile = new AutoDeletingIniFile();
                File.Delete(tempFile.Path);
                //---------------Assert Precondition----------------
                FileSystemMatchers.File(Expect(tempFile.Path).Not.To.Be.A);

                //---------------Execute Test ----------------------
                Create(tempFile.Path);

                //---------------Test Result -----------------------
                FileSystemMatchers.File(Expect(tempFile.Path).To.Be.A);
            }

            [Test]
            public void WhenSourceContainsComments_ShouldIgnoreCommentedParts()
            {
                using var tempFile = new AutoDeletingIniFile();
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

            [Test]
            public void ShouldNotConsiderSemiColonsInQuotedValuesToBeCommentDelimiters()
            {
                // Arrange
                using var tmp = new AutoDeletingTempFile();
                var expected =
                    "DRIVER={SQL Server};SERVER=192.168.1.2\\SQLEXPRESS02;DATABASE=MyDb;UID=sa;PWD=sneaky;TRUSTED_CONNECTION=NO;CONNECTION TIMEOUT=60;";
                var src = new[]
                {
                    "[PATHS]",
                    "; The line below contains the SQL connection string to customer DB",
                    $"DBPATH=\"{expected}\""
                };
                File.WriteAllBytes(
                    tmp.Path,
                    Encoding.UTF8.GetBytes(
                        string.Join(Environment.NewLine, src)
                    )
                );

                // Act
                var ini = Create(tmp.Path);
                // Assert
                Expect(ini["PATHS"]["DBPATH"])
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class AddCommentToSection
        {
            [Test]
            public void ShouldAddCommentToOutput()
            {
                using var tempFile = new AutoDeletingIniFile();
                // Arrange
                var sut = Create(tempFile.Path);
                // Act
                sut.AddSection("whitelist", "some comment");
                sut.Persist();
                // Assert
                var contents = File.ReadAllText(tempFile.Path);
                Console.WriteLine(contents);
                var lines = contents.Split(
                        new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .ToArray();
                Expect(lines).To.Contain.Exactly(1)
                    .Equal.To("[whitelist]");
                Expect(lines).To.Contain.Exactly(1)
                    .Equal.To(";some comment");
            }
        }

        [TestFixture]
        public class ParsingFiles
        {
            [Test]
            public void WhenValueIsQuoted_ShouldSetDataValueUnQuoted()
            {
                //---------------Set up test pack-------------------
                using var tempFile = new AutoDeletingIniFile();
                var section = RandString();
                var key = RandString();
                var value = RandString();
                var iniDataLines = new[] {"[" + section + "]", key + "=\"" + value + "\""};
                tempFile.Write(iniDataLines);

                //---------------Assert Precondition----------------
                FileSystemMatchers.File(Expect(tempFile.Path).To.Be.A);

                //---------------Execute Test ----------------------
                var sut = Create(tempFile.Path) as INIFile_EXPOSES_Sections;

                //---------------Test Result -----------------------
                Expect(sut.Data).Not.To.Be.Null();
                Expect(sut.Data).To.Contain.Key(section);
                Expect(sut.Data[section]).To.Contain.Key(key)
                    .With.Value(value);
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
                using var tempFile = new AutoDeletingIniFile();
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
                using var tempFile = new AutoTempFile();
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
            [TestFixture]
            public class GivenNoFileName
            {
                [TestFixture]
                public class WhenNoFileNameSpecifiedInConstructor
                {
                    [Test]
                    public void ShouldThrowException()
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
                }

                [Test]
                public void ShouldWriteINIDataToOriginalFile()
                {
                    //---------------Set up test pack-------------------
                    using var tempFile = new AutoDeletingIniFile();
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

            [TestFixture]
            public class GivenFileName
            {
                [Test]
                public void ShouldWriteINIDataToFile()
                {
                    //---------------Set up test pack-------------------
                    using var tempFile = new AutoDeletingIniFile();
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

                [Test]
                public void ShouldPersistTheGlobalEmptySection()
                {
                    //---------------Set up test pack-------------------
                    using var tempFile = new AutoDeletingIniFile();
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

            [TestFixture]
            public class GivenStream
            {
                [TestFixture]
                public class PersistingComments
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
                        using var memStream = new MemoryStream(new byte[1024], true);
                        //---------------Test Result -----------------------
                        sut.Persist(memStream);
                        var lines = memStream.AsString()
                            .Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                        Expect(lines).To.Contain.Exactly(1)
                            .Equal.To("; this is the general section");
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
                        using var memStream = new MemoryStream(new byte[1024], true);
                        //---------------Test Result -----------------------
                        sut.Persist(memStream);
                        var lines = memStream.AsString()
                            .Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                        Expect(lines).To.Contain.Exactly(1)
                            .Equal.To("; this is the general section");
                        Expect(lines).To.Contain.Exactly(1)
                            .Equal.To("; this is the general section again!");
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
                        using var memStream = new MemoryStream(new byte[1024], true);
                        //---------------Test Result -----------------------
                        sut.Persist(memStream);
                        var lines = memStream.AsString()
                            .Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                        Expect(lines).To.Contain.Exactly(1)
                            .Equal.To("; this is the general section");
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
                        using var memStream = new MemoryStream(new byte[1024], true);
                        //---------------Test Result -----------------------
                        sut.Persist(memStream);
                        var lines = memStream.AsString()
                            .Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                        Assert.IsTrue(lines.Any(l => l == "; this is the general section"));
                        Assert.IsTrue(lines.Any(l => l == "; this is the general section again!"));
                    }
                }

                [Test]
                public void ShouldWriteOutToStream()
                {
                    //---------------Set up test pack-------------------
                    var sut = Create();
                    sut.AddSection("general");
                    sut["general"]["foo"] = "bar";

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    using var memStream = new MemoryStream(new byte[1024], true);
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
        public class RemovingSettings
        {
            [Test]
            public void ShouldRemoveSettingWhenKeyIsRemoved()
            {
                // Arrange
                var ini = @"
[main]
key1=""value1""
key2=""value2""
";
                var expected = @"
[main]
key2=""value2""
";
                var sut = new INIFile();
                sut.Parse(ini);
                Expect(sut["main"]["key1"])
                    .To.Equal("value1");
                Expect(sut["main"]["key2"])
                    .To.Equal("value2");
                // Act
                var section = sut["main"];
                section.Remove("key1");
                var memStream = new MemoryStream();
                sut.Persist(memStream);
                // Assert
                var updatedIni = Encoding.UTF8.GetString(memStream.ToArray());
                Expect(updatedIni.Trim())
                    .To.Equal(expected.Trim());
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
                using var tempFile = new AutoDeletingIniFile();
                var ini = new INIFile(tempFile.Path);
                ini[section][key] = value;
                ini.Persist();

                //---------------Execute Test ----------------------
                var ini2 = new INIFile(tempFile.Path);
                var result = ini2[section][key];

                //---------------Test Result -----------------------
                Expect(result).To.Equal(value);
            }
        }

        [TestFixture]
        public class Sections
        {
            [Test]
            public void ShouldReturnSectionNames()
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
        public class Renaming
        {
            [Test]
            public void ShouldReturnOnNonExistentSection()
            {
                var sut = Create();
                var section = GetRandomString(4);

                // Assert
                Expect(sut).Not.To.Have.Section(section);
            }

            [Test]
            public void ShouldRenameSection()
            {
                var sut = Create();
                var section = GetRandomString(4);
                var newSection = GetRandomString(5);
                sut.AddSection(section);
                var key = GetRandomString(2);
                var value = GetRandomString(2);
                sut.SetValue(section, key, value);
                sut.RenameSection(section, newSection);

                // Assert
                Expect(sut).Not.To.Have.Section(section);
                Expect(sut).To.Have.Section(newSection);
                Expect(sut[newSection][key]).To.Equal(value);
            }

            [Test]
            [Ignore("Requires an ordered, generic dictionary")]
            public void ShouldNotReOrderSections()
            {
                // Arrange
                var originalIni = @"[section1]
key1=""value1""

[section2]
key2=""value2""";
                var expected = originalIni.RegexReplace("section1", "section_the_first");
                var sut = Create();
                sut.Parse(originalIni);
                // Act
                sut.RenameSection("section1", "section_the_first");
                using var memStream = new MemoryStream();
                sut.Persist(memStream);
                var result = Encoding.UTF8.GetString(memStream.ToArray());
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class SectionSeparator
        {
            [Test]
            public void ShouldAddNoSeparatorWhenNull()
            {
                // Arrange
                var ini = Create();
                // Act
                ini.SectionSeparator = null;
                ini["section1"]["key1"] = "value1";
                ini["section2"]["key2"] = "value2";
                using var memStream = new MemoryStream();
                ini.Persist(memStream);
                var result = memStream.AsString();
                // Assert
                Expect(result)
                    .To.Equal(@"[section1]
key1=""value1""
[section2]
key2=""value2""");
            }

            [Test]
            public void ShouldDefaultToWritingOutEmptyLine()
            {
                // Arrange
                var ini = Create();
                // Act
                ini["section1"]["key1"] = "value1";
                ini["section2"]["key2"] = "value2";
                using var memStream = new MemoryStream();
                ini.Persist(memStream);
                var result = memStream.AsString();
                // Assert
                Expect(result)
                    .To.Equal(@"[section1]
key1=""value1""

[section2]
key2=""value2""");
            }

            [Test]
            public void ShouldCommentOutCustomSeparatorIfNecessary()
            {
                // Arrange
                var ini = Create();
                // Act
                ini.SectionSeparator = "----";
                ini["section1"]["key1"] = "value1";
                ini["section2"]["key2"] = "value2";
                using var memStream = new MemoryStream();
                ini.Persist(memStream);
                var result = memStream.AsString();
                // Assert
                Expect(result)
                    .To.Equal(@"[section1]
key1=""value1""
;----
[section2]
key2=""value2""");
            }

            [Test]
            public void ShouldNotCommentOutCommentedSeparator()
            {
                // Arrange
                var ini = Create();
                // Act
                ini.SectionSeparator = ";----";
                ini["section1"]["key1"] = "value1";
                ini["section2"]["key2"] = "value2";
                using var memStream = new MemoryStream();
                ini.Persist(memStream);
                var result = memStream.AsString();
                // Assert
                Expect(result)
                    .To.Equal(@"[section1]
key1=""value1""
;----
[section2]
key2=""value2""");
            }
        }

        [TestFixture]
        public class AppendTrailingNewLine
        {
            [Test]
            public void ShouldDefaultToNotAppendTrailingNewLine()
            {
                // Arrange
                var ini = Create();
                // Act
                ini["section"]["key"] = "value";
                using var memStream = new MemoryStream();
                ini.Persist(memStream);
                var result = memStream.AsString();
                // Assert
                Expect(result)
                    .To.Equal(@"[section]
key=""value""");
            }

            [Test]
            public void ShouldAppendTrailingNewLineWhenIsTrue()
            {
                // Arrange
                var ini = Create();
                // Act
                ini.AppendTrailingNewLine = true;
                ini["section"]["key"] = "value";
                using var memStream = new MemoryStream();
                ini.Persist(memStream);
                var result = memStream.AsString();
                // Assert
                Expect(result)
                    .To.Equal(@"[section]
key=""value""
");
            }
        }

        [TestFixture]
        public class Reloading
        {
            [Test]
            public void WhenFileOnDiskHasChanged_ShouldReflectChanges()
            {
                // Arrange
                using var tempFile = new AutoDeletingIniFile();
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

            [Test]
            public void WhenMergedFileOnDiskChanged_ShouldReflectChanges()
            {
                // Arrange
                using var mainFile = new AutoDeletingIniFile();
                using var mergeFile = new AutoDeletingIniFile();
                mainFile.Write("[section1]\nkey1=value1");
                mergeFile.Write("[section2]\nkey2=value2");
                var sut = Create(mainFile.Path);
                sut.Merge(mergeFile.Path, MergeStrategies.OnlyAddIfMissing);
                // Pre-assert
                Expect(sut["section2"]["key2"]).To.Equal("value2");
                // Act
                mergeFile.Write("[section2]\nkey2=value3");
                sut.Reload();
                // Assert
                Expect(sut["section2"]["key2"]).To.Equal("value3");
            }
        }

        [TestFixture]
        public class Encodings
        {
            [TestFixture]
            public class UTF8
            {
                [Test]
                public void ShouldPreserveUTF8Characters()
                {
                    // Arrange
                    using var tempFile = new AutoTempFile();
                    var ini1 = new INIFile()
                    {
                        SectionSeparator = null
                    };
                    var section = "Section≄";
                    var setting = "Setting⑹";
                    var value = "Valueäčɛ";
                    ini1.SetValue(section, setting, value);
                    ini1.Persist(tempFile.Path);
                    // Act

                    var ini2 = new INIFile(tempFile.Path)
                    {
                        SectionSeparator = null
                    };

                    // Assert
                    Expect(ini2.HasSection(section))
                        .To.Be.True();
                    Expect(ini2.HasSetting(section, setting))
                        .To.Be.True();
                    Expect(ini2[section][setting])
                        .To.Equal(value);
                }

                [Test]
                public void ShouldPreserveUTF8CharactersWithOverrideEncoding()
                {
                    // Arrange
                    using var tempFile = new AutoTempFile();
                    var ini1 = new INIFile()
                    {
                        SectionSeparator = null,
                        DefaultEncoding = Encoding.ASCII
                    };
                    var section = "Section≄";
                    var setting = "Setting⑹";
                    var value = "Valueäčɛ";
                    ini1.SetValue(section, setting, value);
                    ini1.Persist(tempFile.Path, Encoding.UTF8);
                    // Act

                    var ini2 = new INIFile(tempFile.Path)
                    {
                        SectionSeparator = null
                    };

                    // Assert
                    Expect(ini2.HasSection(section))
                        .To.Be.True();
                    Expect(ini2.HasSetting(section, setting))
                        .To.Be.True();
                    Expect(ini2[section][setting])
                        .To.Equal(value);
                }

                [Test]
                public void ShouldReadAndWriteExampleINIFile()
                {
                    // Arrange
                    var iniFilePath = PathTo("ExampleSettings.ini");
                    var bytesBefore = File.ReadAllBytes(iniFilePath);
                    Expect(iniFilePath).To.Exist();
                    // Act
                    var ini1 = new INIFile(iniFilePath);
                    var current = ini1["DrawingViewer"]["EnableLogging"];
                    var enabled = Convert.ToBoolean(current);
                    ini1["DrawingViewer"]["EnableLogging"] = (!enabled).ToString();
                    ini1.SectionSeparator = null;
                    ini1.Persist();

                    var ini2 = new INIFile(iniFilePath);
                    var result = Convert.ToBoolean(
                        ini2["DrawingViewer"]["EnableLogging"]
                    );
                    // Assert
                    Expect(result).Not.To.Equal(enabled);
                    // if you breakpoint above here (before re-writing the file
                    //  with the setting "False", and open the file with Notepad,
                    //  it will render fine!
                    // reset file

                    ini2["DrawingViewer"]["EnableLogging"] = "False";
                    ini2.SectionSeparator = null;
                    ini2.Persist();
                    // suddenly, Notepad (and no other editor) renders the file in Chinese
                    var bytesAfter = File.ReadAllBytes(iniFilePath);
                    var strBefore = Encoding.UTF8.GetString(bytesBefore);
                    var strAfter = Encoding.UTF8.GetString(bytesAfter);
                    Expect(bytesBefore)
                        .To.Equal(bytesAfter,
                            () => FindDifference(bytesBefore, bytesAfter)
                        );
                }

                [Test]
                public void ShouldReadINIWithBOMCorrectly()
                {
                    // Arrange
                    var iniFilePath = PathTo("bom-ini.ini");

                    // Act
                    var ini = new INIFile(iniFilePath);

                    // Assert
                    Expect(ini.HasSection("Zielordner"))
                        .To.Be.True();
                    Expect(ini["Zielordner"]["Ordner"])
                        .To.Equal("C:\\capitan-Data\\CNC\\20210202\\");
                }

                private static string PathTo(string fileName)
                {
                    var assemblyPath = new Uri(
                        typeof(TestINIFile).Assembly.Location
                    ).LocalPath;
                    return Path.Combine(
                        Path.GetDirectoryName(assemblyPath),
                        fileName
                    );
                }

                private string FindDifference(byte[] bytesBefore, byte[] bytesAfter)
                {
                    var offset = 0;
                    var max = Math.Max(bytesBefore.Length, bytesAfter.Length);
                    while (offset < max)
                    {
                        if (bytesBefore[offset] != bytesAfter[offset])
                        {
                            break;
                        }

                        offset++;
                    }

                    if (offset == max)
                    {
                        return "Not sure where the difference comes in";
                    }

                    var before = Encoding.UTF8.GetString(bytesBefore);
                    var after = Encoding.UTF8.GetString(bytesAfter);

                    return new[]
                    {
                        $"Difference is at {offset} bytes",
                        $"before bytes: {before.Skip(offset - 4).Take(8).JoinWith("")}",
                        $"after bytes: {after.Skip(offset - 4).Take(8).JoinWith("")}",
                        before,
                        "\n\n",
                        after
                    }.JoinWith("\n");
                }
            }
        }

        [TestFixture]
        public class RealWorldIssues
        {
            [Test]
            public void ShouldReturnFullQuotedValueIrrespectiveOfEmbeddedSemiColon()
            {
                // Arrange
                var contents = @"
[notification]
Email_Message=""[HTML] [HEAD] [style type=\""text/css\""] body{font-family: Verdana, Geneva, sans-serif; font-size:10pt;} td{font-family: Verdana, Geneva, sans-serif;font-size:10pt;} p{margin-top: 10px; margin-bottom: 10px;} ol,ul{margin-top: -10px; margin-bottom: -10px;} [/style] [/HEAD] [BODY]test [font style=\""background-color: rgb(255, 255, 192);\""]{MovedFileFullPath}[/font][/BODY][/HTML]""
";
                var expected =
                    @"[HTML] [HEAD] [style type=""text/css""] body{font-family: Verdana, Geneva, sans-serif; font-size:10pt;} td{font-family: Verdana, Geneva, sans-serif;font-size:10pt;} p{margin-top: 10px; margin-bottom: 10px;} ol,ul{margin-top: -10px; margin-bottom: -10px;} [/style] [/HEAD] [BODY]test [font style=""background-color: rgb(255, 255, 192);""]{MovedFileFullPath}[/font][/BODY][/HTML]";
                using var file = new AutoTempFile(contents);
                var sut = Create(file);
                // Act
                var result = sut["notification"]["Email_Message"];
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldPersistQuotesInValuesWithEscapingBackslashesWhenStrict()
            {
                // Arrange
                using var file = new AutoTempFile();
                var expected = @"
[test]
value=""some value containing \""quotes\""""
".Trim();
                var sut = Create(file.Path);
                sut.ParseStrategy = ParseStrategies.Strict;
                // Act
                sut.AddSection("test");
                sut["test"]["value"] = @"some value containing ""quotes""";
                sut.Persist();
                // Assert
                var contents = File.ReadAllText(file.Path).Trim();
                Expect(contents)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldPersistEscapedQuotesWhichCameInEscapedInBestEffortMode()
            {
                // Arrange
                using var file = new AutoTempFile();
                var expected = @"
[test]
value=""some value containing \""quotes\""""
".Trim();
                File.WriteAllText(file.Path, expected);
                var sut = Create(file.Path);
                sut.ParseStrategy = ParseStrategies.BestEffort;
                // Act
                sut.Persist();
                // Assert
                var contents = File.ReadAllText(file.Path).Trim();
                Expect(contents)
                    .To.Equal(expected);
            }

            [TestFixture]
            public class NullDerefOnLoad_Issue60
            {
                [Test]
                public void ShouldNotThrow()
                {
                    // Arrange
                    var sut = new INIFile(new IniFileLineParser());
                    // Act
                    Expect(() => sut.Parse(iniData))
                        .Not.To.Throw();
                    // Assert
                }
                
                class IniFileLineParser : BestEffortLineParser
                {
                    public IniFileLineParser()
                    {
                        base.CommentDelimiter = "'";
                    }
                }

                
                const string iniData = @"[GENERALE]
FILENAME=
AUTHOR=THE CONNELLS
TITLE='74-'75
LENGTH=266397
START=0
INTRO=0
STOP=0
OUTTRO=0
MIXIN=0
MIXOUT=2000
STARTLAP=0
ENDLAP=0
BPM=75
CUEPOINTS
VOLPOINTS=M:\RADIORTL\AUDIO\MUSIC\MTP\DAA_AF67B4E83DD94F47AC90E53DDFAD69CD.MTP
FASTOPEN=58526F7E44744161576F5175280729100B4A72516D7558775D74557B506A
RADIO=M:\RadioRTL\CONFIG\
RUNTIMEOPERATIONS=
CUEPOINTS=

[EXCHANGE-RECORDER]
CALLER=CANFONTI

[DJ-PLAYER]
POSITION=-1
TOP=11790
LEFT=420
NOAUTOPLAY=1
START=1
TIMERUN=0
h";
            }
        }

        [TestFixture]
        public class CustomLineParsers
        {
            [Test]
            public void ShouldRetainCustomParser()
            {
                // Arrange
                var parser = Substitute.For<ILineParser>();
                parser.Parse(Arg.Any<string>())
                    .Returns(new ParsedLine("key", "value", "comment", false));
                using var tmpFile = new AutoTempFile(@"
[section]
key=value
".Trim());
                var sut = new INIFile(parser);
                Expect(sut.ParseStrategy)
                    .To.Equal(ParseStrategies.Custom);
                Expect(sut.CustomLineParser)
                    .To.Be(parser);

                // Act
                sut.Load(tmpFile.Path);
                Expect(parser)
                    .To.Have.Received(2)
                    .Parse(Arg.Any<string>());
                sut.ParseStrategy = ParseStrategies.BestEffort;
                sut.ParseStrategy = ParseStrategies.Custom;
                parser.ClearReceivedCalls();
                sut.Load(tmpFile.Path);
                // Assert
                Expect(parser)
                    .To.Have.Received(2)
                    .Parse(Arg.Any<string>());
            }

            [Test]
            public void ShouldUseInjectedCustomLineParserToParseString()
            {
                // Arrange
                var parser = Substitute.For<ILineParser>();
                parser.Parse(Arg.Any<string>())
                    .Returns(new ParsedLine("key", "value", "comment", false));
                var contents = @"
[section]
key=value
".Trim();
                var sut = new INIFile(parser);
                Expect(sut.ParseStrategy)
                    .To.Equal(ParseStrategies.Custom);
                Expect(sut.CustomLineParser)
                    .To.Be(parser);

                // Act
                sut.Parse(contents);
                Expect(parser)
                    .To.Have.Received(2)
                    .Parse(Arg.Any<string>());
                sut.ParseStrategy = ParseStrategies.BestEffort;
                sut.ParseStrategy = ParseStrategies.Custom;
                parser.ClearReceivedCalls();
                sut.Parse(contents);
                // Assert
                Expect(sut.ParseStrategy)
                    .To.Equal(ParseStrategies.Custom);
                Expect(parser)
                    .To.Have.Received(2)
                    .Parse(Arg.Any<string>());
            }

            [Test]
            public void ShouldRespectCommentDelimiterOnLineParser()
            {
                // Arrange
                var iniData = @"
[SomeSection]
'Commented=true
Uncommented=false
";
                var parser = new BestEffortLineParser()
                {
                    CommentDelimiter = "'"
                };
                var sut = Create(parser);
                // Act
                sut.Parse(iniData);
                // Assert
                Expect(sut["SomeSection"])
                    .To.Contain.Key("Uncommented")
                    .With.Value("false");
                Expect(sut["SomeSection"])
                    .Not.To.Contain.Key("Commented");
            }

            [Test]
            public void ShouldRetainCommentDelimiterOnLineParser()
            {
                // Arrange
                var iniData = @"
[SomeSection]
'Commented=""true""
Uncommented=""false""
".Replace(Environment.NewLine, "\n").Trim();
                var parser = new BestEffortLineParser()
                {
                    CommentDelimiter = "'"
                };
                var sut = Create(parser);
                sut.Parse(iniData);
                using var tempFile = new AutoTempFile();
                sut.Persist(tempFile.Path);
                // Act
                var persisted = File.ReadAllText(tempFile.Path);
                // Assert
                var trimmed = persisted.Replace(Environment.NewLine, "\n")
                    .Trim();
                Expect(trimmed)
                    .To.Equal(iniData);
            }
        }

        private static string RandString()
        {
            return GetRandomAlphaString(1, 10);
        }

        private static IINIFile Create(AutoTempFile iniFile)
        {
            return Create(iniFile.Path);
        }

        private static IINIFile Create(AutoDeletingIniFile iniFile)
        {
            return Create(iniFile.Path);
        }

        private static IINIFile Create(ILineParser lineParser)
        {
            return new INIFile_EXPOSES_Sections(lineParser);
        }

        private static IINIFile Create(string path = null)
        {
            return new INIFile_EXPOSES_Sections(path);
        }

        [TestFixture]
        public class WrapValueInQuotes
        {
            [TestFixture]
            public class WhenWrapValueInQuotesIsTrue
            {
                [TestFixture]
                public class WhenValueIsNotNullOrEmpty
                {
                    [Test]
                    public void ShouldWriteValueWithQuotesToStream()
                    {
                        //---------------Set up test pack-------------------
                        var sut = Create();
                        sut.WrapValueInQuotes = true;
                        sut[""]["foo"] = "bar";

                        //---------------Assert Precondition----------------

                        //---------------Execute Test ----------------------
                        using var memStream = new MemoryStream(new byte[1024], true);
                        sut.Persist(memStream);

                        //---------------Test Result -----------------------
                        var result = Encoding.UTF8.GetString(memStream.ReadAllBytes()).Split('\0').First();
                        Expect(result).To.Equal("foo=\"bar\"");
                    }
                }

                [TestFixture]
                public class WhenValueIsNull
                {
                    [Test]
                    public void ShouldWriteOnlyKeyToStream()
                    {
                        //---------------Set up test pack-------------------
                        var sut = Create();
                        sut.WrapValueInQuotes = true;
                        sut[""]["foo"] = null;

                        //---------------Assert Precondition----------------

                        //---------------Execute Test ----------------------
                        using var memStream = new MemoryStream(new byte[1024], true);
                        sut.Persist(memStream);

                        //---------------Test Result -----------------------
                        var result = Encoding.UTF8.GetString(memStream.ReadAllBytes()).Split('\0').First();
                        Expect(result).To.Equal("foo");
                    }
                }

                [TestFixture]
                public class WhenValueIsEmptyString
                {
                    [Test]
                    public void ShouldWriteKeyWithEqualsAndQuotesToStream()
                    {
                        //---------------Set up test pack-------------------
                        var sut = Create();
                        sut.WrapValueInQuotes = true;
                        sut[""]["foo"] = string.Empty;

                        //---------------Assert Precondition----------------

                        //---------------Execute Test ----------------------
                        using var memStream = new MemoryStream(new byte[1024], true);
                        sut.Persist(memStream);

                        //---------------Test Result -----------------------
                        var result = Encoding.UTF8.GetString(memStream.ReadAllBytes()).Split('\0').First();
                        Expect(result).To.Equal("foo=\"\"");
                    }
                }
            }

            [TestFixture]
            public class WhenWrapValueInQuotesIsFalse
            {
                [TestFixture]
                public class WhenValueIsNotNullOrEmpty
                {
                    [Test]
                    public void ShouldWriteValueWithoutQuotesToStream()
                    {
                        //---------------Set up test pack-------------------
                        var sut = Create();
                        sut.WrapValueInQuotes = false;
                        sut[""]["foo"] = "bar";

                        //---------------Assert Precondition----------------

                        //---------------Execute Test ----------------------
                        using var memStream = new MemoryStream(new byte[1024], true);
                        sut.Persist(memStream);

                        //---------------Test Result -----------------------
                        var result = Encoding.UTF8.GetString(memStream.ReadAllBytes()).Split('\0').First();
                        Expect(result).To.Equal("foo=bar");
                    }
                }

                [TestFixture]
                public class WhenValueIsNull
                {
                    [Test]
                    public void ShouldWriteOnlyKeyToStream()
                    {
                        //---------------Set up test pack-------------------
                        var sut = Create();
                        sut.WrapValueInQuotes = false;
                        sut[""]["foo"] = null;

                        //---------------Assert Precondition----------------

                        //---------------Execute Test ----------------------
                        using var memStream = new MemoryStream(new byte[1024], true);
                        sut.Persist(memStream);

                        //---------------Test Result -----------------------
                        var result = Encoding.UTF8.GetString(memStream.ReadAllBytes()).Split('\0').First();
                        Expect(result).To.Equal("foo");
                    }
                }

                [TestFixture]
                public class WhenValueIsEmptyString
                {
                    [Test]
                    public void ShouldWriteKeyWithEqualsToStream()
                    {
                        //---------------Set up test pack-------------------
                        var sut = Create();
                        sut.WrapValueInQuotes = false;
                        sut[""]["foo"] = string.Empty;

                        //---------------Assert Precondition----------------

                        //---------------Execute Test ----------------------
                        using var memStream = new MemoryStream(new byte[1024], true);
                        sut.Persist(memStream);

                        //---------------Test Result -----------------------
                        var result = Encoding.UTF8.GetString(memStream.ReadAllBytes()).Split('\0').First();
                        Expect(result).To.Equal("foo=");
                    }
                }
            }
        }

        [TestFixture]
        public class KeysWithoutValues
        {
            [Test]
            public void ShouldReadAsNull()
            {
                // Arrange
                var ini = @"
[general]
some-key
";
                var sut = Create();
                // Act
                sut.Parse(ini);
                // Assert
                Expect(sut)
                    .To.Have.Setting("general", "some-key");
                Expect(sut["general"]["some-key"])
                    .To.Be.Null();
            }

            [Test]
            public void ShouldStoreNullAsKeyOnly()
            {
                // Arrange
                using var tempFile = new AutoTempFile();
                var sut = Create();
                // Act
                sut.AddSection("general");
                sut["general"]["some-key"] = null;
                sut.Persist(tempFile.Path);
                // Assert
                Expect(tempFile.StringData.Trim())
                    .To.Equal(@"
[general]
some-key
".Trim());
            }
        }

        private class INIFile_EXPOSES_Sections : INIFile
        {
            public INIFile_EXPOSES_Sections(string path) : base(path)
            {
            }

            public INIFile_EXPOSES_Sections(ILineParser lineParser) : base(lineParser)
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
                {
                    _tempFile += append;
                }
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
        public static IMore<string> File(this IA<string> to)
        {
            return to.AddMatcher(
                actual =>
                {
                    var passed = System.IO.File.Exists(actual);
                    return new MatcherResult(
                        passed,
                        $"Expected '{actual}' {passed.AsNot()}to be a file"
                    );
                });
        }

        public static IMore<IINIFile> Setting(
            this IHave<IINIFile> have,
            string section,
            string setting,
            string value
        )
        {
            return have.Compose(actual =>
            {
                Expect(actual).To.Have.Setting(section, setting);
                Expect(actual[section][setting])
                    .To.Equal(value, $"Expected '[{section}]'/'{setting}' to equal '{value}'");
            });
        }

        public static IMore<IINIFile> Setting(
            this IHave<IINIFile> have,
            string section,
            string setting)
        {
            return have.AddMatcher(actual =>
            {
                var passed = actual.HasSetting(section, setting);
                return new MatcherResult(
                    passed,
                    () => $"Expected {passed.AsNot()}to have setting '{section}'/'{setting}'"
                );
            });
        }

        public static IMore<IINIFile> Section(this IHave<IINIFile> have, string section)
        {
            return have.AddMatcher(actual =>
            {
                var passed = actual.HasSection(section);
                return new MatcherResult(
                    passed,
                    () => $"Expected {passed.AsNot()}to have section '{section}'"
                );
            });
        }

        public static IMore<string> Exist(this ITo<string> to)
        {
            return to.AddMatcher(actual =>
            {
                var passed = System.IO.File.Exists(actual);
                return new MatcherResult(
                    passed,
                    () => $"Expected file '{actual}' {passed.AsNot()}to exist"
                );
            });
        }
    }
}