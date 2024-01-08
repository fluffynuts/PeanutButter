using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.NetCore.Tests
{
    [TestFixture]
    public class TestBuilder
    {
        [TestFixture]
        public class FailureToBuild
        {
            [Test]
            public void ShouldThrowNotImplementedIfAttemptingToBuildInterface()
            {
                // Arrange
                // Act
                Expect(() => FooInterfaceBuilder.Create().Build())
                    .To.Throw<NotImplementedException>()
                    .With.Message.Containing("override ConstructEntity");
                // Assert
            }

            public interface IFoo
            {
            }

            public class FooInterfaceBuilder : Builder<FooInterfaceBuilder, IFoo>
            {
            }

            [Test]
            public void ShouldThrowNotImplementedIfEntityRequiresConstructorParams()
            {
                // Arrange
                // Act
                Expect(() => FooConcreteBuilder.Create().Build())
                    .To.Throw<NotImplementedException>()
                    .With.Message.Containing("override ConstructEntity");
                // Assert
            }

            public class Foo
            {
                public string Name { get; }

                public Foo(string name)
                {
                    Name = name;
                }
            }

            public class FooConcreteBuilder : Builder<FooConcreteBuilder, Foo>
            {
            }
        }

        [TestFixture]
        public class NormalOperations
        {
            [Test]
            public void ShouldBeAbleToConstructDefaultEntity()
            {
                // Arrange
                // Act
                var result = SomeEntityBuilder.Create().Build();
                // Assert
                Expect(result).Not.To.Be.Null();
                Expect(result.Name).To.Equal(default);
                Expect(result.Id).To.Equal(default);
            }

            [Test]
            public void ShouldApplyTransforms()
            {
                // Arrange
                var name = GetRandomString(1);
                var id = GetRandomInt(1);
                // Act
                var result = SomeEntityBuilder.Create()
                    .WithProp(o => o.Name = name)
                    .WithProp(o => o.Id = id)
                    .Build();
                // Assert
                Expect(result).Not.To.Be.Null();
                Expect(result.Id).To.Equal(id);
                Expect(result.Name).To.Equal(name);
            }

            public class SomeEntity
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            public class SomeEntityBuilder : Builder<SomeEntityBuilder, SomeEntity>
            {
            }
        }

        [TestFixture]
        public class Overrides
        {
            [Test]
            public void ShouldBeAbleToOverrideEntityConstruction()
            {
                // Arrange
                // Act
                var result1 = SomeEntityBuilderOverridingConstruction.Create()
                    .WithProp(o => o.Name = GetRandomString())
                    .Build();
                var result2 = SomeEntityBuilderOverridingConstruction.Create()
                    .WithProp(o => o.Name = GetRandomString())
                    .Build();
                // Assert
                Expect(result1).Not.To.Be.Null();
                Expect(result2).Not.To.Be.Null();
                Expect(result1.Id).To.Equal(1);
                Expect(result2.Id).To.Equal(2);
            }

            public class SomeEntity
            {
                public int Id { get; set; }
                public string Name { get; set; }

                public SomeEntity(
                    int id)
                {
                    Id = id;
                }
            }

            public class SomeEntityBuilderOverridingConstruction
                : Builder<SomeEntityBuilderOverridingConstruction, SomeEntity>
            {
                private static int _id = 0;

                protected override SomeEntity ConstructEntity()
                {
                    return new SomeEntity(++_id);
                }
            }

            [Test]
            public void ShouldBeAbleToOverrideBuilding()
            {
                // Arrange
                // Act
                var result1 = SomeEntityBuilderOverridingBuild.Create()
                    .WithProp(o => o.Name = GetRandomString())
                    .Build();
                var result2 = SomeEntityBuilderOverridingBuild.Create()
                    .WithProp(o => o.Name = GetRandomString())
                    .Build();
                // Assert
                Expect(result1).Not.To.Be.Null();
                Expect(result2).Not.To.Be.Null();
                Expect(result1.Id).To.Equal(1);
                Expect(result2.Id).To.Equal(2);
            }

            public class SomeEntityBuilderOverridingBuild : Builder<SomeEntityBuilderOverridingBuild, SomeEntity>
            {
                private static int _id = 0;

                protected override SomeEntity ConstructEntity()
                {
                    return new SomeEntity(0);
                }

                public override SomeEntity Build()
                {
                    var result = base.Build();
                    result.Id = ++_id;
                    return result;
                }
            }
        }

        [TestFixture]
        public class OperatingOnStructs
        {
            [Test]
            public void ShouldBeAbleToConstructTheStructType()
            {
                // Arrange
                var id = GetRandomInt(1);
                var name = GetRandomString(1);
                // Act
                var result = FooBuilder.Create()
                    .WithProp((ref Foo o) => o.Id = id)
                    .WithProp((ref Foo o) => o.Name = name)
                    .Build();
                // Assert
                Expect(result.Id).To.Equal(id);
                Expect(result.Name).To.Equal(name);
            }

            public struct Foo
            {
                public int Id;
                public string Name;
            }

            public class FooBuilder : Builder<FooBuilder, Foo>
            {
            }
        }

        [TestFixture]
        public class BuildTimeTransforms
        {
            [Test]
            public void ShouldApplyBuildTimeTransform()
            {
                // Arrange
                // Act
                var result = FooBuilder.Create()
                    .WithId(42)
                    .Build();
                // Assert
                Expect(result.Id).To.Equal(42);
                Expect(result.Name).To.Equal("Id: 42");
            }

            [Test]
            public void ShouldNotKeepBuildTimeTransformsBetweenInstanceUsages()
            {
                // Arrange
                var builder = FooBuilder.Create();
                // Act
                builder
                    .WithId(13)
                    .Build();
                builder.Depth = 0;
                Expect(() => builder.Build())
                    .Not.To.Throw();
                // Assert
            }

            public class Foo
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            public class FooBuilder : Builder<FooBuilder, Foo>
            {
                public int Depth = 0;

                public FooBuilder WithId(int id)
                {
                    return WithProp(o =>
                    {
                        o.Id = id;
                        WithProp(o2 =>
                        {
                            if (Depth++ > 1)
                            {
                                throw new InvalidOperationException("Have already set id");
                            }

                            o2.Name = $"Id: {id}";
                        });
                        Depth++;
                    });
                }
            }

            [Test]
            public void ShouldNotStackOverflowOnBuildTimeTransforms()
            {
                // Arrange
                // Act
                Expect(() => ReEntrantBuilder.Create()
                        .WithId(12)
                        .Build()
                    ).To.Throw<InvalidOperationException>()
                    .With.Message.Containing("re-entrant");
                // Assert
            }

            public class ReEntrantBuilder : Builder<ReEntrantBuilder, Foo>
            {
                public ReEntrantBuilder WithId(int id)
                {
                    return WithProp(o =>
                    {
                        WithId(id);
                    });
                }
            }
        }
    }
}