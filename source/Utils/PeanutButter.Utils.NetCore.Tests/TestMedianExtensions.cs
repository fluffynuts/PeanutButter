using System;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestMedianExtensions
    {
        [TestFixture]
        public class Ints
        {
            [Test]
            public void ShouldThrowForEmptyCollection()
            {
                // Arrange
                var data = new int[0];
                // Act
                Expect(() => data.Median())
                    .To.Throw<ArgumentException>()
                    .With.Message.Containing("empty");
                // Assert
            }

            [Test]
            public void ShouldReturnTheOnlyItemInSingleItemCollection()
            {
                // Arrange
                var data = new[] { 1 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(1);
            }

            [Test]
            public void ShouldReturnTheMiddleItemInOddNumberedCollection()
            {
                // Arrange
                var data = new[] { 1, 2, 3 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(2);
            }

            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedData()
            {
                // Arrange
                var data = new[] { 10, 16, 20, 40 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
            
            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedData2()
            {
                // Arrange
                var data = new[] { 5,  10, 16, 20, 40, 50 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
        }
        
        [TestFixture]
        public class Decimals
        {
            [Test]
            public void ShouldThrowForEmptyCollection()
            {
                // Arrange
                var data = new decimal[0];
                // Act
                Expect(() => data.Median())
                    .To.Throw<ArgumentException>()
                    .With.Message.Containing("empty");
                // Assert
            }

            [Test]
            public void ShouldReturnTheOnlyItemInSingleItemCollection()
            {
                // Arrange
                var data = new decimal[] { 1 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(1);
            }

            [Test]
            public void ShouldReturnTheMiddleItemInOddNumberedCollection()
            {
                // Arrange
                var data = new decimal[] { 1, 2, 3 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(2);
            }

            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedData()
            {
                // Arrange
                var data = new decimal[] { 10, 16, 20, 40 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
            
            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePoointsInEvenNumberedData2()
            {
                // Arrange
                var data = new decimal[] { 5,  10, 16, 20, 40, 50 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
        }
        
        [TestFixture]
        public class Longs
        {
            [Test]
            public void ShouldThrowForEmptyCollection()
            {
                // Arrange
                var data = new long[0];
                // Act
                Expect(() => data.Median())
                    .To.Throw<ArgumentException>()
                    .With.Message.Containing("empty");
                // Assert
            }

            [Test]
            public void ShouldReturnTheOnlyItemInSingleItemCollection()
            {
                // Arrange
                var data = new long[] { 1 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(1);
            }

            [Test]
            public void ShouldReturnTheMiddleItemInOddNumberedCollection()
            {
                // Arrange
                var data = new long[] { 1, 2, 3 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(2);
            }

            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedData()
            {
                // Arrange
                var data = new long[] { 10, 16, 20, 40 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
            
            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedData2()
            {
                // Arrange
                var data = new long[] { 5,  10, 16, 20, 40, 50 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
        }
        
        [TestFixture]
        public class Floats
        {
            [Test]
            public void ShouldThrowForEmptyCollection()
            {
                // Arrange
                var data = new float[0];
                // Act
                Expect(() => data.Median())
                    .To.Throw<ArgumentException>()
                    .With.Message.Containing("empty");
                // Assert
            }

            [Test]
            public void ShouldReturnTheOnlyItemInSingleItemCollection()
            {
                // Arrange
                var data = new float[] { 1 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(1);
            }

            [Test]
            public void ShouldReturnTheMiddleItemInOddNumberedCollection()
            {
                // Arrange
                var data = new float[] { 1, 2, 3 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(2);
            }

            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedData()
            {
                // Arrange
                var data = new float[] { 10, 16, 20, 40 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
            
            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedData2()
            {
                // Arrange
                var data = new float[] { 5,  10, 16, 20, 40, 50 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
        }
        
        [TestFixture]
        public class Doubles
        {
            [Test]
            public void ShouldThrowForEmptyCollection()
            {
                // Arrange
                var data = new double[0];
                // Act
                Expect(() => data.Median())
                    .To.Throw<ArgumentException>()
                    .With.Message.Containing("empty");
                // Assert
            }

            [Test]
            public void ShouldReturnTheOnlyItemInSingleItemCollection()
            {
                // Arrange
                var data = new double[] { 1 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(1);
            }

            [Test]
            public void ShouldReturnTheMiddleItemInOddNumberedCollection()
            {
                // Arrange
                var data = new double[] { 1, 2, 3 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(2);
            }

            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedData()
            {
                // Arrange
                var data = new double[] { 10, 16, 20, 40 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
            
            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedData2()
            {
                // Arrange
                var data = new double[] { 5,  10, 16, 20, 40, 50 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
        }
        
        [TestFixture]
        public class Bytes
        {
            [Test]
            public void ShouldThrowForEmptyCollection()
            {
                // Arrange
                var data = new byte[0];
                // Act
                Expect(() => data.Median())
                    .To.Throw<ArgumentException>()
                    .With.Message.Containing("empty");
                // Assert
            }

            [Test]
            public void ShouldReturnTheOnlyItemInSingleItemCollection()
            {
                // Arrange
                var data = new byte[] { 1 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(1);
            }

            [Test]
            public void ShouldReturnTheMiddleItemInOddNumberedCollection()
            {
                // Arrange
                var data = new byte[] { 1, 2, 3 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(2);
            }

            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedData()
            {
                // Arrange
                var data = new byte[] { 10, 16, 20, 40 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
            
            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumeberedData2()
            {
                // Arrange
                var data = new byte[] { 5,  10, 16, 20, 40, 50 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
        }
        
        [TestFixture]
        public class UInts
        {
            [Test]
            public void ShouldThrowForEmptyCollection()
            {
                // Arrange
                var data = new uint[0];
                // Act
                Expect(() => data.Median())
                    .To.Throw<ArgumentException>()
                    .With.Message.Containing("empty");
                // Assert
            }

            [Test]
            public void ShouldReturnTheOnlyItemInSingleItemCollection()
            {
                // Arrange
                var data = new uint[] { 1 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(1);
            }

            [Test]
            public void ShouldReturnTheMiddleItemInOddNumberedCollection()
            {
                // Arrange
                var data = new uint[] { 1, 2, 3 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(2);
            }

            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePodata()
            {
                // Arrange
                var data = new uint[] { 10, 16, 20, 40 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
            
            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedCollection()
            {
                // Arrange
                var data = new uint[] { 5,  10, 16, 20, 40, 50 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
        }
        
        [TestFixture]
        public class ULongs
        {
            [Test]
            public void ShouldThrowForEmptyCollection()
            {
                // Arrange
                var data = new ulong[0];
                // Act
                Expect(() => data.Median())
                    .To.Throw<ArgumentException>()
                    .With.Message.Containing("empty");
                // Assert
            }

            [Test]
            public void ShouldReturnTheOnlyItemInSingleItemCollection()
            {
                // Arrange
                var data = new ulong[] { 1 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(1);
            }

            [Test]
            public void ShouldReturnTheMiddleItemInOddNumberedCollection()
            {
                // Arrange
                var data = new ulong[] { 1, 2, 3 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(2);
            }

            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedCollection()
            {
                // Arrange
                var data = new ulong[] { 10, 16, 20, 40 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
            
            [Test]
            public void ShouldReturnTheAverageOfTwoMiddlePointsInEvenNumberedCollection2()
            {
                // Arrange
                var data = new ulong[] { 5,  10, 16, 20, 40, 50 };
                // Act
                var result = data.Median();
                // Assert
                Expect(result)
                    .To.Equal(18);
            }
        }
    }
}