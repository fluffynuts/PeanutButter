using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using PeanutButter.SimpleHTTPServer;
using static NExpect.Expectations;

// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestStreamExtensions
    {
        [TestFixture]
        public class ReadingAllBytes
        {
            [Test]
            public void OperatingOnNullStream_ShouldReturnNull()
            {
                //---------------Set up test pack-------------------
                Stream src = null;
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = src.ReadAllBytes();

                //---------------Test Result -----------------------
                Expect(result).To.Be.Null();
            }

            [Test]
            public void OperatingOnStreamWithNoData_ShouldReturnEmptyArray()
            {
                //---------------Set up test pack-------------------
                using var memStream = new MemoryStream();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = memStream.ReadAllBytes();

                //---------------Test Result -----------------------
                Expect(result).To.Be.Empty();
            }

            [Test]
            public void OperatingOnStreamWithData_ShouldReturnAllData()
            {
                //---------------Set up test pack-------------------
                var expected = GetRandomBytes();
                using var memStream = new MemoryStream(expected);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = memStream.ReadAllBytes();

                //---------------Test Result -----------------------
                Expect(result).To.Equal(expected);
            }

            [Test]
            public void OperatingOnStreamWithData_WhenStreamIsNotAtBeginningAndCanSeek_ShouldReturnAllData()
            {
                //---------------Set up test pack-------------------
                var expected = GetRandomBytes(20, 50);
                using var memStream = new MemoryStream(expected);
                memStream.Seek(GetRandomInt(1, 10), SeekOrigin.Begin);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = memStream.ReadAllBytes();

                //---------------Test Result -----------------------
                Expect(result).To.Equal(expected);
            }

            [Test]
            public void OperatingOnStreamWithData_WhenCannotRewind_ShouldReadRemainingBytes()
            {
                // Arrange
                var data = GetRandomBytes(100, 1000);
                var part1 = new byte[1];
                using var server = new HttpServer();
                server.ServeFile("/bin.dat", data);
                // Act
                var req = WebRequest.Create(server.GetFullUrlFor("/bin.dat"));
                using var res = req.GetResponse();
                using var stream = res.GetResponseStream();
                var firstRead = stream.Read(part1, 0, 1);
                Expect(firstRead).To.Equal(1);
                var remainder = stream.ReadAllBytes();
                Expect(remainder.Length).To.Equal(data.Length - 1);
                Expect(remainder).To.Equal(
                    data.Skip(1).ToArray());

                // Assert
            }

            [Test]
            public void ShouldBeAbleToReadAllBytesFromWebResponseStream()
            {
                // Arrange
                var expected = GetRandomBytes(1024);
                using var server = new HttpServer();
                var path = $"/{GetRandomString(2)}";
                server.ServeFile(path, expected);
                // Act
                var req = WebRequest.Create(server.GetFullUrlFor(path));
                using var res = req.GetResponse();
                using var stream = res.GetResponseStream();
                var result = stream.ReadAllBytes();
                // Assert
                Expect(result).To.Equal(expected);
            }
        }

        [TestFixture]
        public class SavingStreamsToFiles
        {
            [Test]
            public void ShouldCreateTheFile()
            {
                // Arrange
                using var tempFile = new AutoTempFile();
                var data = GetRandomBytes(1024);
                using var src = new MemoryStream(data);
                // Act
                src.Save(tempFile.Path);
                // Assert
                var persisted = File.ReadAllBytes(tempFile.Path);
                Expect(persisted)
                    .To.Equal(data);
            }

            [Test]
            public void ShouldOverwriteAnExistingFile()
            {
                // Arrange
                using var tempFile = new AutoTempFile();
                var initialData = GetRandomBytes(1024);
                var data = GetRandomBytes(1024);
                using var src = new MemoryStream(data);
                using var initialSource = new MemoryStream(initialData);
                // Act
                initialSource.Save(tempFile.Path);
                src.Save(tempFile.Path);
                // Assert
                var persisted = File.ReadAllBytes(tempFile.Path);
                Expect(persisted)
                    .To.Equal(data);
            }

            [Test]
            public void ShouldCreateAnyRequiredContainingFolders()
            {
                // Arrange
                using var tempFolder = new AutoTempFolder();
                var target = Path.Combine(tempFolder.Path, "level1", "level2", "output.bin");
                var data = GetRandomBytes();
                using var src = new MemoryStream(data);
                // Act
                src.Save(target);
                // Assert
                var persisted = File.ReadAllBytes(target);
                Expect(persisted)
                    .To.Equal(data);
            }
        }

        [TestFixture]
        public class SavingStreamsToFilesAsync
        {
            [Test]
            public async Task ShouldCreateTheFile()
            {
                // Arrange
                using var tempFile = new AutoTempFile();
                var data = GetRandomBytes(1024);
                using var src = new MemoryStream(data);
                // Act
                await src.SaveAsync(tempFile.Path);
                // Assert
                var persisted = File.ReadAllBytes(tempFile.Path);
                Expect(persisted)
                    .To.Equal(data);
            }

            [Test]
            public async Task ShouldOverwriteAnExistingFile()
            {
                // Arrange
                using var tempFile = new AutoTempFile();
                var initialData = GetRandomBytes(1024);
                var data = GetRandomBytes(1024);
                using var src = new MemoryStream(data);
                using var initialSource = new MemoryStream(initialData);
                // Act
                await initialSource.SaveAsync(tempFile.Path);
                await src.SaveAsync(tempFile.Path);
                // Assert
                var persisted = File.ReadAllBytes(tempFile.Path);
                Expect(persisted)
                    .To.Equal(data);
            }

            [Test]
            public async Task ShouldCreateAnyRequiredContainingFolders()
            {
                // Arrange
                using var tempFolder = new AutoTempFolder();
                var target = Path.Combine(tempFolder.Path, "level1", "level2", "output.bin");
                var data = GetRandomBytes();
                using var src = new MemoryStream(data);
                // Act
                await src.SaveAsync(target);
                // Assert
                var persisted = File.ReadAllBytes(target);
                Expect(persisted)
                    .To.Equal(data);
            }
        }

        [TestFixture]
        public class ReadingAllBytesAsync
        {
            [Test]
            public async Task OperatingOnNullStream_ShouldReturnNull()
            {
                //---------------Set up test pack-------------------
                Stream src = null;
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = await src.ReadAllBytesAsync();

                //---------------Test Result -----------------------
                Expect(result).To.Be.Null();
            }

            [Test]
            public async Task OperatingOnStreamWithNoData_ShouldReturnEmptyArray()
            {
                //---------------Set up test pack-------------------
                using var memStream = new MemoryStream();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = await memStream.ReadAllBytesAsync();

                //---------------Test Result -----------------------
                Expect(result).To.Be.Empty();
            }

            [Test]
            public async Task OperatingOnStreamWithData_ShouldReturnAllData()
            {
                //---------------Set up test pack-------------------
                var expected = GetRandomBytes();
                using var memStream = new MemoryStream(expected);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = await memStream.ReadAllBytesAsync();

                //---------------Test Result -----------------------
                Expect(result).To.Equal(expected);
            }

            [Test]
            public async Task OperatingOnStreamWithData_WhenStreamIsNotAtBeginningAndCanSeek_ShouldReturnAllData()
            {
                //---------------Set up test pack-------------------
                var expected = GetRandomBytes(20, 50);
                using var memStream = new MemoryStream(expected);
                memStream.Seek(GetRandomInt(1, 10), SeekOrigin.Begin);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = await memStream.ReadAllBytesAsync();

                //---------------Test Result -----------------------
                Expect(result).To.Equal(expected);
            }

            [Test]
            public async Task OperatingOnStreamWithData_WhenCannotRewind_ShouldReadRemainingBytes()
            {
                // Arrange
                var data = GetRandomBytes(100, 1000);
                var part1 = new byte[1];
                using var server = new HttpServer();
                server.ServeFile("/bin.dat", data);
                // Act
                var req = WebRequest.Create(server.GetFullUrlFor("/bin.dat"));
                using var res = await req.GetResponseAsync();
                using var stream = res.GetResponseStream();
                var firstRead = await stream.ReadAsync(part1, 0, 1);
                Expect(firstRead).To.Equal(1);
                var remainder = await stream.ReadAllBytesAsync();
                Expect(remainder.Length).To.Equal(data.Length - 1);
                Expect(remainder).To.Equal(
                    data.Skip(1).ToArray());

                // Assert
            }

            [Test]
            public async Task ShouldBeAbleToReadAllBytesAsyncFromWebResponseStream()
            {
                // Arrange
                var expected = GetRandomBytes(1024);
                using var server = new HttpServer();
                var path = $"/{GetRandomString(2)}";
                server.ServeFile(path, expected);
                // Act
                var req = WebRequest.Create(server.GetFullUrlFor(path));
                using var res = await req.GetResponseAsync();
                using var stream = res.GetResponseStream();
                var result = await stream.ReadAllBytesAsync();
                // Assert
                Expect(result).To.Equal(expected);
            }
        }

        [TestFixture]
        public class WritingBytes
        {
            [Test]
            public void OperatingOnNullStream_ShouldThrowIOException()
            {
                //---------------Set up test pack-------------------
                Stream dst = null;
                var expected = GetRandomBytes();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(
                        () => dst.WriteAllBytes(expected)
                    )
                    .To.Throw<IOException>();

                //---------------Test Result -----------------------
            }

            [Test]
            public void OperatingOnNonNullStream_ShouldWriteAllBytes()
            {
                //---------------Set up test pack-------------------
                using var memStream = new MemoryStream();
                var expected = GetRandomBytes();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                memStream.WriteAllBytes(expected);

                //---------------Test Result -----------------------
                memStream.Seek(0, SeekOrigin.Begin);
                var result = memStream.ReadAllBytes();
                Expect(result).To.Equal(expected);
            }

            [Test]
            public void OperatingOnNonNullStream_GivenNulldata_ShouldNotThrow()
            {
                //---------------Set up test pack-------------------
                using var memStream = new MemoryStream();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                memStream.WriteAllBytes(null);

                //---------------Test Result -----------------------
                memStream.Seek(0, SeekOrigin.Begin);
                var result = memStream.ReadAllBytes();
                Expect(result).To.Be.Empty();
            }

            [Test]
            public void OperatingOnStreamWithExistingData_ShouldOverwrite()
            {
                //---------------Set up test pack-------------------
                var initial = GetRandomBytes();
                using var folder = new AutoTempFolder();
                var file = CreateRandomFileIn(folder.Path);
                using var fileStream = File.Open(
                    Path.Combine(folder.Path, file),
                    FileMode.Open,
                    FileAccess.ReadWrite);
                fileStream.Write(initial, 0, initial.Length);
                var expected = GetRandomBytes();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                fileStream.WriteAllBytes(expected);

                //---------------Test Result -----------------------
                fileStream.Rewind();
                var result = fileStream.ReadAllBytes();
                Expect(result).To.Equal(expected);
            }

            [Test]
            public void OperatingOnNullStream_ShouldThrow()
            {
                // Arrange
                // Pre-assert
                // Act
                Expect(
                        () => (null as MemoryStream).WriteAllBytes(GetRandomBytes())
                    ).To.Throw<IOException>()
                    .With.Message.Containing("stream is null");
                // Assert
            }

            [Test]
            public void GivenNull_ShouldNotWriteOrThrow()
            {
                // Arrange
                var buffer = GetRandomBytes(128);
                var copy = buffer.DeepClone();
                var stream = new MemoryStream(buffer);
                // Pre-assert
                // Act
                Expect(() => stream.WriteAllBytes(null))
                    .Not.To.Throw();
                // Assert
                Expect(buffer).To.Equal(copy);
            }

            [Test]
            public void GivenEmptyData_ShouldNotWriteOrThrow()
            {
                // Arrange
                var buffer = GetRandomBytes(128);
                var copy = buffer.DeepClone();
                var stream = new MemoryStream(buffer);
                // Pre-assert
                // Act
                Expect(() => stream.WriteAllBytes(new byte[0]))
                    .Not.To.Throw();
                // Assert
                Expect(buffer).To.Equal(copy);
            }
        }

        [TestFixture]
        public class WritingBytesAsync
        {
            [Test]
            public void OperatingOnNullStream_ShouldThrowIOException()
            {
                //---------------Set up test pack-------------------
                Stream dst = null;
                var expected = GetRandomBytes(128);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(
                        async () => await dst.WriteAllBytesAsync(expected)
                    )
                    .To.Throw<IOException>();

                //---------------Test Result -----------------------
            }

            [Test]
            public async Task OperatingOnNonNullStream_ShouldWriteAllBytesAsync()
            {
                //---------------Set up test pack-------------------
                using var memStream = new MemoryStream();
                var expected = GetRandomBytes(128);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                await memStream.WriteAllBytesAsync(expected);

                //---------------Test Result -----------------------
                memStream.Seek(0, SeekOrigin.Begin);
                var result = await memStream.ReadAllBytesAsync();
                Expect(result).To.Equal(expected);
            }

            [Test]
            public async Task OperatingOnNonNullStream_GivenNulldata_ShouldNotThrow()
            {
                //---------------Set up test pack-------------------
                using var memStream = new MemoryStream();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                await memStream.WriteAllBytesAsync(null);

                //---------------Test Result -----------------------
                memStream.Seek(0, SeekOrigin.Begin);
                var result = await memStream.ReadAllBytesAsync();
                Expect(result).To.Be.Empty();
            }

            [Test]
            public async Task OperatingOnStreamWithExistingData_ShouldOverwrite()
            {
                //---------------Set up test pack-------------------
                var initial = GetRandomBytes();
                using var folder = new AutoTempFolder();
                var file = CreateRandomFileIn(folder.Path);
                using var fileStream = File.Open(
                    Path.Combine(folder.Path, file),
                    FileMode.Open,
                    FileAccess.ReadWrite);
                fileStream.Write(initial, 0, initial.Length);
                var expected = GetRandomBytes(128);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                await fileStream.WriteAllBytesAsync(expected);

                //---------------Test Result -----------------------
                fileStream.Rewind();
                var result = await fileStream.ReadAllBytesAsync();
                Expect(result).To.Equal(expected);
            }

            [Test]
            public void OperatingOnNullStream_ShouldThrow()
            {
                // Arrange
                // Pre-assert
                // Act
                Expect(
                        async () => await (null as MemoryStream).WriteAllBytesAsync(GetRandomBytes())
                    ).To.Throw<IOException>()
                    .With.Message.Containing("stream is null");
                // Assert
            }

            [Test]
            public void GivenNull_ShouldNotWriteOrThrow()
            {
                // Arrange
                var buffer = GetRandomBytes(128);
                var copy = buffer.DeepClone();
                var stream = new MemoryStream(buffer);
                // Pre-assert
                // Act
                Expect(async () => await stream.WriteAllBytesAsync(null))
                    .Not.To.Throw();
                // Assert
                Expect(buffer).To.Equal(copy);
            }

            [Test]
            public void GivenEmptyData_ShouldNotWriteOrThrow()
            {
                // Arrange
                var buffer = GetRandomBytes(128);
                var copy = buffer.DeepClone();
                var stream = new MemoryStream(buffer);
                // Pre-assert
                // Act
                Expect(async () => await stream.WriteAllBytesAsync(new byte[0]))
                    .Not.To.Throw();
                // Assert
                Expect(buffer).To.Equal(copy);
            }
        }

        [TestFixture]
        public class AppendingData
        {
            [Test]
            public void OperatingOnNonNullStream_ShouldAppendData()
            {
                //---------------Set up test pack-------------------
                using var folder = new AutoTempFolder();
                var fileName = CreateRandomFileIn(folder.Path);
                var initial = File.ReadAllBytes(Path.Combine(folder.Path, fileName));
                using var fileStream =
                    File.Open(Path.Combine(folder.Path, fileName), FileMode.Open, FileAccess.ReadWrite);
                var toAdd = GetRandomBytes(1, 1);
                var expected = initial.Concat(toAdd).ToArray();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                fileStream.Append(toAdd);

                //---------------Test Result -----------------------
                var result = fileStream.ReadAllBytes();
                Expect(result).To.Equal(expected);
            }

            [Test]
            public void OperatingOnNullStream_ShouldThrowIOException()
            {
                //---------------Set up test pack-------------------
                Stream dst = null;
                var toAppend = GetRandomBytes();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(() => dst.Append(toAppend)).To.Throw<IOException>();

                //---------------Test Result -----------------------
            }
        }

        [TestFixture]
        public class AppendingDataAsync
        {
            [Test]
            public async Task OperatingOnNonNullStream_ShouldAppendData()
            {
                //---------------Set up test pack-------------------
                using var folder = new AutoTempFolder();
                var fileName = CreateRandomFileIn(folder.Path);
                var initial = File.ReadAllBytes(Path.Combine(folder.Path, fileName));
                using var fileStream =
                    File.Open(Path.Combine(folder.Path, fileName), FileMode.Open, FileAccess.ReadWrite);
                var toAdd = GetRandomBytes(1, 1);
                var expected = initial.Concat(toAdd).ToArray();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                await fileStream.AppendAsync(toAdd);

                //---------------Test Result -----------------------
                var result = await fileStream.ReadAllBytesAsync();
                Expect(result).To.Equal(expected);
            }

            [Test]
            public void OperatingOnNullStream_ShouldThrowIOException()
            {
                //---------------Set up test pack-------------------
                Stream dst = null;
                var toAppend = GetRandomBytes();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(
                    async () => await dst.AppendAsync(toAppend)
                ).To.Throw<IOException>();

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void AsString_GivenStreamWithStringAndNullPadding_ShouldReturnString()
        {
            //---------------Set up test pack-------------------
            var expected = GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using var memStream = new MemoryStream(new byte[1024], true);
            //---------------Test Result -----------------------
            memStream.WriteAllBytes(Encoding.UTF8.GetBytes(expected));
            memStream.Rewind();
            var result = memStream.AsString();
            Expect(result).To.Equal(expected);
        }

        [Test]
        public async Task AsStringAsync_GivenStreamWithStringAndNullPadding_ShouldReturnString()
        {
            //---------------Set up test pack-------------------
            var expected = GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using var memStream = new MemoryStream(new byte[1024], true);
            //---------------Test Result -----------------------
            await memStream.WriteAllBytesAsync(Encoding.UTF8.GetBytes(expected));
            memStream.Rewind();
            var result = await memStream.AsStringAsync();
            Expect(result).To.Equal(expected);
        }


        [TestFixture]
        public class WritingStrings
        {
            [Test]
            public void OperatingOnStream_GivenDataAndNoEncoding_ShouldWriteToStream_WithUtf8Encoding()
            {
                //--------------- Arrange -------------------
                var buffer = new byte[128];
                var toWrite = GetRandomString(5, 10);
                var expected = toWrite.AsBytes();

                using var stream = new MemoryStream(buffer);
                //--------------- Assume ----------------

                //--------------- Act ----------------------
                stream.WriteString(toWrite, Encoding.UTF8);

                //--------------- Assert -----------------------
                var copy = new byte[toWrite.Length];
                Buffer.BlockCopy(buffer, 0, copy, 0, toWrite.Length);
                Expect(copy).To.Equal(expected);
            }

            [Test]
            public void GivenNoEncodig_ShouldDefaultTo_UTF8()
            {
                // Arrange
                var buffer = new byte[128];
                var toWrite = GetRandomString(10);
                var expected = toWrite.AsBytes(Encoding.UTF8);
                // Pre-assert
                // Act
                using (var stream = new MemoryStream(buffer))
                {
                    stream.WriteString(toWrite);
                }

                // Assert
                var copy = new byte[toWrite.Length];
                Buffer.BlockCopy(buffer, 0, copy, 0, toWrite.Length);
                Expect(copy).To.Equal(expected);
            }

            [Test]
            public void OperatingOnStream_GivenNullData_ShouldWriteNothing()
            {
                //--------------- Arrange -------------------
                var size = 128;
                var buffer = new byte[size];
                var expectedValue = (byte) GetRandomInt(2, 10);
                for (var i = 0; i < buffer.Length; i++)
                    buffer[i] = expectedValue;
                using var stream = new MemoryStream(buffer);
                //--------------- Assume ----------------

                //--------------- Act ----------------------
                stream.WriteString(null, Encoding.UTF8);

                //--------------- Assert -----------------------
                Expect(buffer).To.Contain.Only(size).Equal.To(expectedValue);
            }

            public static Encoding[] Encodings { get; } =
            {
                Encoding.UTF8,
                Encoding.ASCII,
                Encoding.UTF7
            };

            [TestCaseSource(nameof(Encodings))]
            public void OperatingOnStream_GivenDataAndEncoding_ShouldWriteToStream(Encoding encoding)
            {
                //--------------- Arrange -------------------
                var buffer = new byte[128];
                var toWrite = GetRandomString(5, 10);
                var expected = toWrite.AsBytes(encoding);

                using var stream = new MemoryStream(buffer);
                //--------------- Assume ----------------

                //--------------- Act ----------------------
                stream.WriteString(toWrite, encoding);

                //--------------- Assert -----------------------
                var copy = new byte[toWrite.Length];
                Buffer.BlockCopy(buffer, 0, copy, 0, toWrite.Length);
                Expect(copy).To.Equal(expected);
            }
        }

        [TestFixture]
        public class WritingStringsAsync
        {
            [Test]
            public async Task OperatingOnStream_GivenDataAndNoEncoding_ShouldWriteToStream_WithUtf8Encoding()
            {
                //--------------- Arrange -------------------
                var buffer = new byte[128];
                var toWrite = GetRandomString(5, 10);
                var expected = toWrite.AsBytes();

                using var stream = new MemoryStream(buffer);
                //--------------- Assume ----------------

                //--------------- Act ----------------------
                await stream.WriteStringAsync(toWrite, Encoding.UTF8);

                //--------------- Assert -----------------------
                var copy = new byte[toWrite.Length];
                Buffer.BlockCopy(buffer, 0, copy, 0, toWrite.Length);
                Expect(copy).To.Equal(expected);
            }

            [Test]
            public async Task GivenNoEncodig_ShouldDefaultTo_UTF8()
            {
                // Arrange
                var buffer = new byte[128];
                var toWrite = GetRandomString(10);
                var expected = toWrite.AsBytes(Encoding.UTF8);
                // Pre-assert
                // Act
                using (var stream = new MemoryStream(buffer))
                {
                    await stream.WriteStringAsync(toWrite);
                }

                // Assert
                var copy = new byte[toWrite.Length];
                Buffer.BlockCopy(buffer, 0, copy, 0, toWrite.Length);
                Expect(copy).To.Equal(expected);
            }

            [Test]
            public async Task OperatingOnStream_GivenNullData_ShouldWriteNothing()
            {
                //--------------- Arrange -------------------
                var size = 128;
                var buffer = new byte[size];
                var expectedValue = (byte) GetRandomInt(2, 10);
                for (var i = 0; i < buffer.Length; i++)
                    buffer[i] = expectedValue;
                using var stream = new MemoryStream(buffer);
                //--------------- Assume ----------------

                //--------------- Act ----------------------
                await stream.WriteStringAsync(null, Encoding.UTF8);

                //--------------- Assert -----------------------
                Expect(buffer).To.Contain.Only(size).Equal.To(expectedValue);
            }

            public static Encoding[] Encodings { get; } =
            {
                Encoding.UTF8,
                Encoding.ASCII,
                Encoding.UTF7
            };

            [TestCaseSource(nameof(Encodings))]
            public async Task OperatingOnStream_GivenDataAndEncoding_ShouldWriteToStream(Encoding encoding)
            {
                //--------------- Arrange -------------------
                var buffer = new byte[128];
                var toWrite = GetRandomString(5, 10);
                var expected = toWrite.AsBytes(encoding);

                using var stream = new MemoryStream(buffer);
                //--------------- Assume ----------------

                //--------------- Act ----------------------
                await stream.WriteStringAsync(toWrite, encoding);

                //--------------- Assert -----------------------
                var copy = new byte[toWrite.Length];
                Buffer.BlockCopy(buffer, 0, copy, 0, toWrite.Length);
                Expect(copy).To.Equal(expected);
            }
        }

        [TestFixture]
        public class AppendString
        {
            [Test]
            public void ShouldAppendTheString()
            {
                // Arrange
                var str1 = GetRandomString();
                var str2 = GetRandomString();
                using var stream = new MemoryStream();
                // Act
                stream.AppendString(str1);
                stream.AppendString(str2);
                // Assert
                var result = Encoding.UTF8.GetString(
                    stream.ToArray()
                );
                Expect(result)
                    .To.Equal($"{str1}{str2}");
            }
        }

        [TestFixture]
        public class ReadAllText
        {
            [Test]
            public void ShouldReadTheStreamAsText()
            {
                // Arrange
                var expected = GetRandomWords();
                var memStream = new MemoryStream(
                    Encoding.UTF8.GetBytes(
                        expected
                    )
                );
                // Act
                var result = memStream.ReadAllText();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
            
            [Test]
            public void ShouldReadTheStreamAsTextTwice()
            {
                // Arrange
                var expected = GetRandomWords();
                var memStream = new MemoryStream(
                    Encoding.UTF8.GetBytes(
                        expected
                    )
                );
                // Act
                memStream.ReadAllText();
                var result = memStream.ReadAllText();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class ReadLines
        {
            [TestCase("\r\n")]
            [TestCase("\n")]
            public void ShouldReadLines(string newline)
            {
                // Arrange
                var expected = new[]
                {
                    GetRandomWords(),
                    GetRandomWords(),
                    GetRandomWords()
                };
                var fullText = expected.JoinWith(newline);
                var stream = new MemoryStream(
                    Encoding.UTF8.GetBytes(
                        fullText
                    )
                );
                var collected = new List<string>();
                // Act
                foreach (var line in stream.ReadLines())
                {
                    collected.Add(line);
                }

                // Assert
                Expect(collected)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldRetainUtf8()
            {
                // Arrange
                var text = "hello 🙄 world";
                var stream = new MemoryStream(
                    Encoding.UTF8.GetBytes(text)
                );
                // Act
                var result = stream.ReadLines().Single();
                // Assert
                Expect(result)
                    .To.Equal(text);
            }

            [Test]
            public void ShouldNotDiscardDanglingCarriageReturns()
            {
                // Arrange
                var lines = "foo\rbar\r\nquuz\napples";
                var expected = new[] { "foo\rbar", "quuz", "apples" };
                var stream = new MemoryStream(
                    Encoding.UTF8.GetBytes(lines)
                );
                // Act
                var result = stream.ReadLines().ToArray();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }
}