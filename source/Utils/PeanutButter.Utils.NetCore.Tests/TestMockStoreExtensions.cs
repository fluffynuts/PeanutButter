using NSubstitute;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestMockStoreExtensions
{
    [TestFixture]
    public class FindOrAddMockStoreFor
    {
        [Test]
        public void ShouldCreateAValidatingStoreForTheType()
        {
            // Arrange
            var repo = Substitute.For<IRepository>();
            // Act
            var store = repo.FindOrAddMockStoreFor<Doggo>();
            // Assert
            Expect(store)
                .To.Be.An.Instance.Of<IDictionary<int, Doggo>>();
            Expect(() => store[1] = GetRandom<Doggo>())
                .Not.To.Throw();
            Expect(() => store[0] = GetRandom<Doggo>())
                .To.Throw<ArgumentException>()
                .With.Message.Containing(
                    "may only be populated with items that have a natural-number"
                ).Then("key");
        }

        [Test]
        public void ShouldFindThatStoreOnSecondInvocation()
        {
            // Arrange
            var repo = Substitute.For<IRepository>();
            var key = GetRandomInt(1);
            var value = GetRandom<Doggo>();
            // Act
            var store1 = repo.FindOrAddMockStoreFor<Doggo>();
            store1[key] = value;
            var store2 = repo.FindOrAddMockStoreFor<Doggo>();
            // Assert
            Expect(store2)
                .To.Be(store1);
            Expect(store2)
                .To.Contain.Key(key)
                .With.Value.Matched.By(
                    o => o.Is(value)
                );
        }
    }

    [TestFixture]
    public class WhenStoreHasParameterlessConstructor
    {
        [Test]
        public void ShouldCreateTheStore()
        {
            // Arrange
            var repo = Substitute.For<IRepository>();
            // Act
            var store = repo.FindOrAddMockStore<List<int>>();
            // Assert
            Expect(store)
                .To.Be.An.Instance.Of<List<int>>();
            Expect(store)
                .To.Be.Empty();
        }

        [Test]
        public void ShouldReturnTheOriginalOnTheSecondInvocation()
        {
            // Arrange
            var repo = Substitute.For<IRepository>();
            var expected = GetRandomInt(1);
            // Act
            var store1 = repo.FindOrAddMockStore<List<int>>();
            store1.Add(expected);
            var store2 = repo.FindOrAddMockStore<List<int>>();
            // Assert
            Expect(store2)
                .To.Be(store1);
            Expect(store2)
                .To.Contain(expected);
        }
    }

    [TestFixture]
    public class WhenGivenStoreFactory
    {
        [Test]
        public void ShouldCreateTheStore()
        {
            // Arrange
            var repo = Substitute.For<IRepository>();
            // Act
            var store = repo.FindOrAddMockStore<IList<int>>(() => new List<int>());
            // Assert
            Expect(store)
                .To.Be.An.Instance.Of<List<int>>();
            Expect(store)
                .To.Be.Empty();
        }

        [Test]
        public void ShouldReturnTheOriginalOnTheSecondInvocation()
        {
            // Arrange
            var repo = Substitute.For<IRepository>();
            var expected = GetRandomInt(1);
            // Act
            var store1 = repo.FindOrAddMockStore<IList<int>>(() => new List<int>());
            store1.Add(expected);
            var store2 = repo.FindOrAddMockStore<IList<int>>(() => new List<int>());
            // Assert
            Expect(store2)
                .To.Be(store1);
            Expect(store2)
                .To.Contain(expected);
        }
    }

    [TestFixture]
    public class WhenStoreAlreadyCreatedAndThenAskedForByDifferentType
    {
        [Test]
        public void ShouldCreateANewStore()
        {
            // stores are keyed by type - so we can have
            // multiple per owner. This means it's up
            // to the consumer to make sure that the
            // correct type is used appropriately, but
            // also means that we can store different
            // things for an owner
            // Arrange
            var repo = Substitute.For<IRepository>();
            // Act
            var store1 = repo.FindOrAddMockStore<List<int>>();
            var store2 = repo.FindOrAddMockStore<List<string>>();
            // Assert
            Expect(store1)
                .To.Be.An.Instance.Of<List<int>>();
            Expect(store2)
                .To.Be.An.Instance.Of<List<string>>();
        }
    }

    public interface IRepository;

    public class Doggo
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public void Wag()
        {
        }
    }
}