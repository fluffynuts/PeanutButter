using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestGenericEntityBuilder
    {
        public class SomeEntity
        {
            public int SomeEntityId { get; set; }
            [Required]
            [MaxLength(5)]
            public string Name { get; set; }
        }

        public class SomeParentEntity
        {
            public int SomeParentEntityId { get; set; }
            [MaxLength(6)]
            public string Name { get; set; }
            public ICollection<SomeChildEntity> Children { get; set; }
        }
        public class SomeChildEntity
        {
            public int SomeChildEntityId { get; set; }
            public virtual SomeParentEntity Parent { get; set; }
        }

        public class SomeEntityBuilder: GenericEntityBuilder<SomeEntityBuilder, SomeEntity>
        {
        }

        public class SomeParentEntityBuilder: GenericEntityBuilder<SomeParentEntityBuilder, SomeChildEntity>
        {
        }

        [Test]
        public void Type_ShouldInheritFromGenericBuilder()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (SomeEntityBuilder);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<GenericBuilder<SomeEntityBuilder, SomeEntity>>();

            //---------------Test Result -----------------------
        }

        private const int MAX_RANDOM_RUNS = 1000;

        [Test]
        public void ShouldTryToFitWithinMaxLength()
        {
            for (var i = 0; i < MAX_RANDOM_RUNS; i++)
            {
                //---------------Set up test pack-------------------
                var item = SomeEntityBuilder.BuildRandom();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = item.Name.Length;

                //---------------Test Result -----------------------
                Assert.That(result, Is.LessThanOrEqualTo(5));
            }
        }

        [Test]
        public void ShouldTryToFitWithinMaxLengthOnGeneratedNavigationProperties()
        {
            for (var i = 0; i < MAX_RANDOM_RUNS; i++)
            {
                //---------------Set up test pack-------------------
                var item = SomeParentEntityBuilder.BuildRandom();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = item.Parent.Name.Length;

                //---------------Test Result -----------------------
                Assert.That(result, Is.LessThanOrEqualTo(6));
            }
        }


    }
}
