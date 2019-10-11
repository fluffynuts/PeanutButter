using NExpect;
using NUnit.Framework;
using PeanutButter.TempDb.MySql.Base;
using static NExpect.Expectations;

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
            Expect(sut.InnoDbIoCapacity).To.Equal(200);
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
            Expect(sut.InnoDbIoCapacity).To.Equal(3000);
        }
    }
}