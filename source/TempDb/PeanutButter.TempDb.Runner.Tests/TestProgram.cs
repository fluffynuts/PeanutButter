using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using NExpect;
using PeanutButter.Utils;
using static NExpect.Expectations;

// ReSharper disable AccessToDisposedClosure

namespace PeanutButter.TempDb.Runner.Tests
{
    [TestFixture]
    public class TestProgram
    {
        public const int DEFAULT_TIMEOUT = 90000;

        [Test]
        public void ShouldBeAbleToStartDefaultAsMySql()
        {
            // Arrange
            using var arena = new TestArena();
            // Act
            arena.WaitForProgramToListen();
            var interestingLine = arena.StdOut.FirstOrDefault(
                line => line.StartsWith(
                    "connection string",
                    StringComparison.InvariantCultureIgnoreCase
                )
            );
            Expect(interestingLine)
                .Not.To.Be.Null(
                    "TempDb runner should emit the connection string on stdout"
                );
            var connectionString = interestingLine!.Split(':')
                .Skip(1)
                .JoinWith(":")
                .Trim();
            using var connection = new MySqlConnection(connectionString);
            // Assert
            Expect(() => connection.Open())
                .Not.To.Throw();
        }

        [TestFixture]
        public class ExplicitMySqlEngineTesting
        {
            [Test]
            [Timeout(DEFAULT_TIMEOUT)]
            public void ShouldBeAbleToExplicitlyStart()
            {
                // Arrange
                using var arena = new TestArena("-e", "mysql");
                // Act
                arena.WaitForProgramToListen();
                var interestingLine = arena.StdOut.FirstOrDefault(
                    line => line.StartsWith(
                        "connection string",
                        StringComparison.InvariantCultureIgnoreCase
                    )
                );
                Expect(interestingLine)
                    .Not.To.Be.Null(
                        "TempDb runner should emit the connection string on stdout"
                    );
                var connectionString = interestingLine!.Split(':')
                    .Skip(1)
                    .JoinWith(":")
                    .Trim();
                using (var connection = new MySqlConnection(connectionString))
                {
                    // Assert
                    Expect(() => connection.Open())
                        .Not.To.Throw();
                }
            }

            [Test]
            [Timeout(DEFAULT_TIMEOUT)]
            public void ShouldBeAbleToStopViaStdIn()
            {
                // Arrange
                using var arena = new TestArena("-e", "mysql");
                // Act
                arena.WaitForProgramToListen();
                var interestingLine = arena.StdOut.FirstOrDefault(
                    line => line.StartsWith(
                        "connection string",
                        StringComparison.InvariantCultureIgnoreCase
                    )
                );
                Expect(interestingLine)
                    .Not.To.Be.Null(
                        "TempDb runner should emit the connection string on stdout"
                    );
                var connectionString = interestingLine!.Split(':')
                    .Skip(1)
                    .JoinWith(":")
                    .Trim();
                using (var connection = new MySqlConnection(connectionString))
                {
                    // Assert
                    Expect(() => connection.Open())
                        .Not.To.Throw();
                }

                arena.WriteStdIn("stop");
                arena.WaitForProgramToExit();

                using (var dead = new MySqlConnection(connectionString))
                {
                    Expect(() => dead.Open())
                        .To.Throw<MySqlException>();
                }
            }
        }

        [TestFixture]
        public class ExplicitLocalDbEngineTesting
        {
            [OneTimeSetUp]
            public void OneTimeSetup()
            {
                if (!Platform.IsWindows)
                {
                    Assert.Ignore(
                        "Test requires LocalDb, found on windows"
                    );
                }
            }

            [Test]
            [Timeout(DEFAULT_TIMEOUT)]
            public void ShouldBeAbleToExplicitlyStart()
            {
                // Arrange
                using var arena = new TestArena("-e", "localdb");
                // Act
                arena.WaitForProgramToListen();
                var interestingLine = arena.StdOut.FirstOrDefault(
                    line => line.StartsWith(
                        "connection string",
                        StringComparison.InvariantCultureIgnoreCase
                    )
                );
                Expect(interestingLine)
                    .Not.To.Be.Null(
                        "TempDb runner should emit the connection string on stdout"
                    );
                var connectionString = interestingLine!.Split(':')
                    .Skip(1)
                    .JoinWith(":")
                    .Trim();
                using (var connection = new SqlConnection(connectionString))
                {
                    // Assert
                    Expect(() => connection.Open())
                        .Not.To.Throw();
                }
            }

            [Test]
            [Timeout(DEFAULT_TIMEOUT)]
            public void ShouldBeAbleToStopViaStdIn()
            {
                // Arrange
                using var arena = new TestArena("-e", "localdb");
                // Act
                arena.WaitForProgramToListen();
                var interestingLine = arena.StdOut.FirstOrDefault(
                    line => line.StartsWith(
                        "connection string",
                        StringComparison.InvariantCultureIgnoreCase
                    )
                );
                Expect(interestingLine)
                    .Not.To.Be.Null(
                        "TempDb runner should emit the connection string on stdout"
                    );
                var connectionString = interestingLine!.Split(':')
                    .Skip(1)
                    .JoinWith(":")
                    .Trim();
                using (var connection = new SqlConnection(connectionString))
                {
                    // Assert
                    Expect(() => connection.Open())
                        .Not.To.Throw();
                }

                arena.WriteStdIn("stop");
                arena.WaitForProgramToExit();

                using (var dead = new SqlConnection(connectionString))
                {
                    Expect(() => dead.Open())
                        .To.Throw<SqlException>();
                }
            }
        }

        [TestFixture]
        public class ExplicitSqliteEngineTesting
        {
            [Test]
            [Timeout(DEFAULT_TIMEOUT)]
            public void ShouldBeAbleToExplicitlyStart()
            {
                // Arrange
                using var arena = new TestArena("-e", "sqlite");
                // Act
                arena.WaitForProgramToListen();
                var interestingLine = arena.StdOut.FirstOrDefault(
                    line => line.StartsWith(
                        "connection string",
                        StringComparison.InvariantCultureIgnoreCase
                    )
                );
                Expect(interestingLine)
                    .Not.To.Be.Null(
                        "TempDb runner should emit the connection string on stdout"
                    );
                var connectionString = interestingLine!.Split(':')
                    .Skip(1)
                    .JoinWith(":")
                    .Trim();
                using (var connection = new SQLiteConnection(connectionString))
                {
                    // Assert
                    Expect(() => connection.Open())
                        .Not.To.Throw();
                }

                var connInfo = new SQLiteConnectionStringBuilder(connectionString);
                Expect(File.Exists(connInfo.DataSource))
                    .To.Be.False();
            }

            [Test]
            [Timeout(DEFAULT_TIMEOUT)]
            public void ShouldBeAbleToStopViaStdIn()
            {
                // Arrange
                using var arena = new TestArena("-e", "sqlite");
                // Act
                arena.WaitForProgramToListen();
                var interestingLine = arena.StdOut.FirstOrDefault(
                    line => line.StartsWith(
                        "connection string",
                        StringComparison.InvariantCultureIgnoreCase
                    )
                );
                Expect(interestingLine)
                    .Not.To.Be.Null(
                        "TempDb runner should emit the connection string on stdout"
                    );
                var connectionString = interestingLine!.Split(':')
                    .Skip(1)
                    .JoinWith(":")
                    .Trim();
                using (var connection = new SQLiteConnection(connectionString))
                {
                    // Assert
                    Expect(() => connection.Open())
                        .Not.To.Throw();
                }

                arena.WriteStdIn("stop");
                arena.WaitForProgramToExit();

                Expect(
                    () =>
                    {
                        using (var dead = new SqlConnection(connectionString))
                        {
                            dead.Open();
                        }
                    }
                );
            }
        }

        public class TestArena : IDisposable
        {
            private bool _haveStartedListening;
            private readonly Barrier _waitForListeningBarrier = new(2);
            private Barrier _readlineBarrier;
            private string _waitingInput;
            public List<string> StdOut { get; } = new();

            public TestArena(params string[] args)
            {
                Program.LineWriter = CaptureLine;
                InteractiveShell.ReadLine = ReadLine;
                _readlineBarrier = new Barrier(2);
                _waitingInput = null;
                Task.Run(
                    () =>
                    {
                        try
                        {
                            Program.Main(args);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Unable to start runner program:\n{ex.Message}");
                        }

                        if (!_haveStartedListening)
                        {
                            // let the test fail properly
                            WaitFor(_waitForListeningBarrier, "program to be listening");
                        }

                        WaitForProgramToExit();
                    }
                );
            }

            private void WaitFor(
                Func<bool> condition,
                string label
            )
            {
                var timeout = DateTime.Now.AddMilliseconds(DEFAULT_TIMEOUT);
                do
                {
                    if (condition())
                    {
                        return;
                    }

                    Thread.Sleep(500);
                } while (DateTime.Now < timeout);

                throw new TimeoutException(
                    $"Timed out whilst waiting for: {label}"
                );
            }

            private void WaitFor(
                Barrier barrier,
                string context
            )
            {
                if (!barrier.SignalAndWait(DEFAULT_TIMEOUT))
                {
                    var output = new List<string>();
                    if (StdOut.Any())
                    {
                        output.Add("stdout:");
                        output.AddRange(StdOut);
                    }

                    var moreInfo = output.Any()
                        ? $"\n{string.Join("\n", output)}"
                        : "";

                    throw new TimeoutException($"Timed out waiting for: {context}");
                }
            }

            public void WaitForProgramToListen()
            {
                WaitFor(
                    _waitForListeningBarrier,
                    "program to be listening"
                );
            }

            public void WaitForProgramToExit()
            {
                WaitFor(() => Program.Instance is not null, "program to have created tempdb instance");
                WaitFor(() => Program.Instance is null, "program to have finished using tempdb instance");
            }

            public void WriteStdIn(string line)
            {
                _waitingInput = $"{line}{Environment.NewLine}";
                _readlineBarrier.SignalAndWait();
                _readlineBarrier = new Barrier(2);
            }

            public string ReadLine()
            {
                _readlineBarrier.SignalAndWait();
                var result = _waitingInput;
                _waitingInput = null;
                return result;
            }

            private void CaptureLine(string line)
            {
                StdOut.Add(line);
                var isConnectionStringLine = (line ?? "")
                    .Contains("connection string", StringComparison.OrdinalIgnoreCase);
                if (!_haveStartedListening && isConnectionStringLine)
                {
                    _waitForListeningBarrier.SignalAndWait();
                    _haveStartedListening = true;
                }
            }

            public void Dispose()
            {
                Program.DestroyInstance();
                Program.LineWriter = Console.WriteLine;
                InteractiveShell.ReadLine = Console.ReadLine;
            }
        }
    }
}