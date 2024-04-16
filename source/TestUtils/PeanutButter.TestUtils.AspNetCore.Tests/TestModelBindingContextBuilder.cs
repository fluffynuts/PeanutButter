using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;

namespace PeanutButter.TestUtils.AspNetCore.Tests
{
    [TestFixture]
    public class TestModelBindingContextBuilder
    {
        [TestFixture]
        public class DefaultBuild
        {
            [Test]
            public void ShouldSetActionContextToANewControllerContext()
            {
                // Arrange
                // Act
                var result1 = BuildDefault();
                var result2 = BuildDefault();
                // Assert
                Expect(result1.ActionContext)
                    .Not.To.Be.Null()
                    .And
                    .To.Be.An.Instance.Of<ControllerContext>();
                Expect(result1.ActionContext)
                    .Not.To.Be(result2.ActionContext);
            }

            [TestCase("Model")]
            public void ShouldSetBinderModelNameTo_(string expected)
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.ModelName)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldSetBindingSourceBody()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.BindingSource)
                    .To.Equal(BindingSource.Body);
            }

            [TestCase("Field")]
            public void ShouldSetTheFieldNameTo_(string expected)
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.FieldName)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldSetIsTopLevelObjectTrue()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.IsTopLevelObject)
                    .To.Be.True();
            }

            [Test]
            public void ShouldSetEmptyModelMetadata()
            {
                // Arrange
                var expectedMetaData = ModelMetadataBuilder.BuildDefault();
                // Act
                var result = BuildDefault();
                // Assert
                var metaData = result.ModelMetadata;
                Expect(metaData)
                    .Not.To.Be.Null();
                Expect(metaData)
                    .To.Be.An.Instance.Of<FakeModelMetadata>();
                Expect(Guid.TryParse(metaData.BindingSource!.DisplayName, out _))
                    .To.Be.True();
                Expect(metaData.BindingSource.IsGreedy)
                    .To.Equal(expectedMetaData.BindingSource!.IsGreedy);
                Expect(metaData.BindingSource.IsFromRequest)
                    .To.Equal(expectedMetaData.BindingSource.IsFromRequest);
                Expect(metaData.BindingSource.Id)
                    .Not.To.Equal(expectedMetaData.BindingSource.Id,
                        () => "should have unique-ish ids for binding sources");
                // shortcut for deep equality testing
                var fake = expectedMetaData as FakeModelMetadata;
                fake!._BindingSource = metaData.BindingSource;
                fake!.SetContainerMetadata(metaData.ContainerMetadata);
                Expect(metaData)
                    .To.Deep.Equal(
                        expectedMetaData
                    );
            }

            [Test]
            public void ShouldSetModelToEmptyObject()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.Model)
                    .To.Deep.Equal(new { });
            }

            [Test]
            public void ShouldSetEmptyModelDictionary()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.ModelState)
                    .Not.To.Be.Null();
                Expect(result.ModelState)
                    .To.Be.Empty();
            }

            [Test]
            public void ShouldSetAlwaysTruePropertyFilter()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.PropertyFilter)
                    .Not.To.Be.Null();
                Expect(result.PropertyFilter!.Invoke(null))
                    .To.Be.True();
            }

            [Test]
            public void ShouldSetValidationState()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.ValidationState)
                    .Not.To.Be.Null();
                Expect(result.ValidationState)
                    .To.Be.Empty();
            }

            [Test]
            public void ShouldSetCompositeValueProvider()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.ValueProvider)
                    .Not.To.Be.Null()
                    .And
                    .To.Be.An.Instance.Of<CompositeValueProvider>();
                var composite = result.ValueProvider as CompositeValueProvider;
                var providers = composite!.ToArray();
                Expect(providers)
                    .To.Contain.Exactly(1)
                    .Matched.By(o => o is RouteValueProvider);
                Expect(providers)
                    .To.Contain.Exactly(1)
                    .Matched.By(o => o is FormValueProvider);
            }

            [Test]
            public void ShouldSetEmptyModelBindingResult()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.Result)
                    .Not.To.Be.Null();
                Expect(result.Result.IsModelSet)
                    .To.Be.True();
                Expect(result.Result.Model)
                    .To.Be(result.Model);
            }

            private static ModelBindingContext BuildDefault()
            {
                return ModelBindingContextBuilder.BuildDefault();
            }
        }

        [TestFixture]
        public class CustomisingTheResult
        {
            [Test]
            public void ShouldBeAbleToCompletelyOverrideIValueProvider()
            {
                // Arrange
                var expected = Substitute.For<IValueProvider>();
                // Act
                var result = ModelBindingContextBuilder.Create()
                    .WithValueProvider(expected)
                    .Build();
                // Assert
                Expect(result.ValueProvider)
                    .To.Be(expected);
            }
        }

        [TestFixture]
        public class ConvenientlyDontRequireAValueProvider
        {
            [Test]
            public void ShouldBeAbleToSetArbitraryValue()
            {
                // Arrange
                var name = GetRandomString();
                var value = GetRandomString();
                // Act
                var ctx = ModelBindingContextBuilder.Create()
                    .WithModelName(name)
                    .WithModelValue(value)
                    .Build();
                var result = ctx.ValueProvider.GetValue(ctx.ModelName);
                // Assert
                Expect(result.Values)
                    .To.Equal(new StringValues(value));
            }

            [Test]
            public void ShouldBeAbleToSetArbitraryValueOutOfOrder()
            {
                // Arrange
                var name = GetRandomString();
                var value = GetRandomString();
                // Act
                var ctx = ModelBindingContextBuilder.Create()
                    .WithModelValue(value)
                    .WithModelName(name)
                    .Build();
                var result = ctx.ValueProvider.GetValue(ctx.ModelName);
                // Assert
                Expect(result.Values)
                    .To.Equal(new StringValues(value));
            }
        }
    }
}