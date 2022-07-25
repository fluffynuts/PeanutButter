using System;
using NExpect;
using NUnit.Framework;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TempDb.Tests
{
    [TestFixture]
    public class TestTempDbMySqlServerSettings
    {
        [Test]
        public void WhenOptimizedForPerformance_ShouldUpdateConfigAppropriately()
        {
            // arrange
            var sut = new TempDbMySqlServerSettings();

            // act
            sut.OptimizeForPerformance();

            // assert
            Expect(sut.GeneralLog).To.Equal(0);
            Expect(sut.SlowQueryLog).To.Equal(0);
            Expect(sut.SyncBinLog).To.Equal(0);
            Expect(sut.InnodbIoCapacity).To.Equal(200);
            Expect(sut.InnodbThreadConcurrency).To.Equal(0);
            Expect(sut.InnodbFlushLogAtTimeout).To.Equal(10);
            Expect(sut.InnodbFlushLogAtTrxCommit).To.Equal(2);
        }

        [Test]
        public void WhenOptimizedForPerformanceOnSSD_ShouldIncreaseIOCapacity()
        {
            // arrange
            var sut = new TempDbMySqlServerSettings();

            // act
            sut.OptimizeForPerformance(isRunningOnSsdDisk: true);

            // assert
            Expect(sut.InnodbIoCapacity).To.Equal(3000);
        }

        [Test]
        public void WhenOptimizedForPerformance_ShouldReturnSelf()
        {
            // arrange
            var sut = Create();

            // act
            var result = sut.OptimizeForPerformance();

            // assert
            Expect(result)
                .To.Be.An.Instance.Of<TempDbMySqlServerSettings>();
        }

        [Test]
        public void ShouldSetPortHintFromEnvironmentWhenAvailable()
        {
            // Arrange
            using var resetter = new AutoResetter<int?>(
                StoreCurrentPortHint,
                RestorePortHint);
            var expected = GetRandomInt(15000, 20000);
            Environment.SetEnvironmentVariable(
                TempDbMySqlServerSettings.EnvironmentVariables.PORT_HINT,
                expected.ToString()
            );
            Expect(
                Environment.GetEnvironmentVariable(
                    TempDbMySqlServerSettings.EnvironmentVariables.PORT_HINT
                )
            ).To.Equal(expected.ToString());
            // Act
            var sut = new TempDbMySqlServerSettings()
            {
                Options =
                {
                    DefaultSchema = GetRandomAlphaString(8, 8)
                },
                InnodbFlushLogAtTrxCommit = 2,
                SlowQueryLog = 0
            };
            // Assert
            Expect(sut.Options.PortHint)
                .To.Equal(expected);
        }

        private TempDbMySqlServerSettings Create()
        {
            return new TempDbMySqlServerSettings();
        }

        private void RestorePortHint(int? priorValue)
        {
            if (priorValue.HasValue)
            {
                Environment.SetEnvironmentVariable(
                    TempDbMySqlServerSettings
                        .EnvironmentVariables
                        .PORT_HINT,
                    priorValue.ToString()
                );
            }
            else
            {
                Environment.SetEnvironmentVariable(
                    TempDbMySqlServerSettings
                        .EnvironmentVariables
                        .PORT_HINT,
                    ""
                );
            }
        }

        private int? StoreCurrentPortHint()
        {
            var current = Environment.GetEnvironmentVariable(
                TempDbMySqlServerSettings
                    .EnvironmentVariables
                    .PORT_HINT
            );
            if (current is null)
            {
                return null;
            }

            return int.Parse(current);
        }
    }
}