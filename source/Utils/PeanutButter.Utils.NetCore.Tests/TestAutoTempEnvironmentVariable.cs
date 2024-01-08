using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.NetCore.Tests
{
    [TestFixture]
    public class TestAutoTempEnvironmentVariable
    {
        [TestFixture]
        public class WhenConstructed
        {
            [Test]
            public void ShouldSetTheNewVariable()
            {
                // Arrange
                var variableName = GetRandomString(10);
                var expected = GetRandomString(10);
                // Act
                Expect(Environment.GetEnvironmentVariable(variableName))
                    .To.Be.Null();
                using var _ = new AutoTempEnvironmentVariable(
                    variableName,
                    expected
                );
                Expect(Environment.GetEnvironmentVariable(variableName))
                    .To.Equal(expected);
                // Assert
            }
        }

        [TestFixture]
        public class WhenDisposed
        {
            [Test]
            public void ShouldResetTheOriginalValue()
            {
                // Arrange
                var variableName = GetRandomString(10);
                var originalValue = GetRandomString(10);
                var tempValue = GetAnother(originalValue);
                Environment.SetEnvironmentVariable(variableName, originalValue);
                using var r = new AutoResetter(
                    () => Environment.SetEnvironmentVariable(variableName, originalValue),
                    () => Environment.SetEnvironmentVariable(variableName, null)
                );

                // Act
                using (var _ = new AutoTempEnvironmentVariable(variableName, tempValue))
                {
                    Expect(Environment.GetEnvironmentVariable(variableName))
                        .To.Equal(tempValue);
                }

                // Assert
                Expect(Environment.GetEnvironmentVariable(variableName))
                    .To.Equal(originalValue);
            }

            [Test]
            public void ShouldOnlyResetOnce()
            {
                // Arrange
                var name = GetRandomString(10);
                var value1 = GetRandomString(10);
                var value2 = GetAnother(value1);
                var value3 = GetAnother(new[] { value1, value2 });
                using var r = new AutoResetter(
                    () => Environment.SetEnvironmentVariable(name, value1),
                    () => Environment.SetEnvironmentVariable(name, null)
                );
                // Act
                var t1 = new AutoTempEnvironmentVariable(name, value1);
                t1.Dispose();
                var t2 = new AutoTempEnvironmentVariable(name, value2);
                t1.Dispose();
                
                // Assert
                Expect(Environment.GetEnvironmentVariable(name))
                    .To.Equal(value2);
            }
        }
    }
}