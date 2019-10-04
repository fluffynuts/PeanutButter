using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

// ReSharper disable AssignNullToNotNullAttribute

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestAutoTempFolder
    {
        [Test]
        public void ShouldImplement_IDisposable()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(AutoTempFolder);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(sut).To.Implement<IDisposable>();

            //---------------Test Result -----------------------
        }

        [TestFixture]
        public class Construction
        {
            [Test]
            public void ShouldMakeNewEmptyFolderAvailable()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using (var folder = new AutoTempFolder())
                {
                    var result = folder.Path;
                    //---------------Test Result -----------------------

                    Expect(result).Not.To.Be.Null();
                    Expect(result).To.Be.A.Folder();
                    var entries = Directory.EnumerateFileSystemEntries(result, "*", SearchOption.AllDirectories);
                    Expect(entries).To.Be.Empty();
                }
            }

            [Test]
            public void GivenCustomBasePath_ShouldUseThatPathAsBaseForTempFolder()
            {
                //---------------Set up test pack-------------------
                var baseFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using (var folder = new AutoTempFolder(baseFolder))
                {
                    //---------------Test Result -----------------------
                    Expect(Path.GetDirectoryName(
                            folder.Path
                        ))
                        .To.Equal(
                            baseFolder
                        );
                }
            }

            [Test]
            public void GivenCustomBasePathWhichDoesNotExistYet_ShouldUseThatPathAsBaseForTempFolder()
            {
                //---------------Set up test pack-------------------
                var baseFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                baseFolder = Path.Combine(baseFolder, GetRandomString(10, 15));

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using (var folder = new AutoTempFolder(baseFolder))
                {
                    //---------------Test Result -----------------------
                    Expect(Path.GetDirectoryName(folder.Path))
                        .To.Equal(baseFolder);
                }
            }
        }

        [TestFixture]
        public class Dispose
        {
            [Test]
            public void ShouldRemoveEmptyTempFolder()
            {
                //---------------Set up test pack-------------------

                string folderPath;
                using (var folder = new AutoTempFolder())
                {
                    folderPath = folder.Path;
                    //---------------Assert Precondition----------------
                    Assert.IsNotNull(folderPath);
                    Assert.IsTrue(Directory.Exists(folderPath));
                    var entries = Directory.EnumerateFileSystemEntries(folderPath, "*", SearchOption.AllDirectories);
                    CollectionAssert.IsEmpty(entries);
                    //---------------Execute Test ----------------------
                }

                //---------------Test Result -----------------------
                Assert.IsFalse(Directory.Exists(folderPath));
            }

            [Test]
            public void ShouldRemoveNonEmptyTempFolder()
            {
                //---------------Set up test pack-------------------
                string folderPath;
                using (var folder = new AutoTempFolder())
                {
                    folderPath = folder.Path;
                    Assert.IsNotNull(folderPath);


                    //---------------Assert Precondition----------------
                    Assert.IsTrue(Directory.Exists(folderPath));
                    var entries = Directory.EnumerateFileSystemEntries(folderPath, "*", SearchOption.AllDirectories);
                    CollectionAssert.IsEmpty(entries);

                    //---------------Execute Test ----------------------
                    File.WriteAllBytes(Path.Combine(folderPath, GetRandomString(2, 10)),
                        GetRandomBytes());
                    File.WriteAllBytes(Path.Combine(folderPath, GetRandomString(11, 20)),
                        GetRandomBytes());
                    entries = Directory.EnumerateFileSystemEntries(folderPath, "*", SearchOption.AllDirectories);
                    CollectionAssert.IsNotEmpty(entries);
                }

                //---------------Test Result -----------------------
                Assert.IsFalse(Directory.Exists(folderPath));
            }
        }
    }
}