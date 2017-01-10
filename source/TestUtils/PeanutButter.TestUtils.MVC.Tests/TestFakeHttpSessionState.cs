using System.Collections.Generic;
using System.Web.SessionState;
using NUnit.Framework;
using PeanutButter.TestUtils.MVC.Builders;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.MVC.Tests
{
    [TestFixture]
    public class TestFakeHttpSessionState: AssertionHelper
    {
        [Test]
        public void Construct_ShouldSetSessionStateItemCollectionInternal()
        {
            //--------------- Arrange -------------------
            var items = new SessionStateItemCollection();
            var key = GetRandomString(5);
            var value = GetRandomString();
            items[key] = value;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sut = Create(items);

            //--------------- Assert -----------------------
            Expect(sut[key], Is.EqualTo(value));
        }

        [Test]
        public void Add_ShouldAddTheItem()
        {
            //--------------- Arrange -------------------
            var key = GetRandomString(5);
            var value = GetRandomString();
            var sut = Create();

            //--------------- Assume ----------------
            Expect(sut.Keys, Does.Not.Contain(key));

            //--------------- Act ----------------------
            sut.Add(key, value);

            //--------------- Assert -----------------------
            Expect(sut[key], Is.EqualTo(value));
        }

        [Test]
        public void Count_ShouldReturnItemCount()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------
            Expect(sut.Count, Is.EqualTo(0));

            //--------------- Act ----------------------
            sut.Add(GetRandomString(5), GetRandomString());
            Expect(sut.Count, Is.EqualTo(1));
            sut.Add(GetRandomString(5), GetRandomString());

            //--------------- Assert -----------------------
            Expect(sut.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetEnumerator_ShouldFacilitateForEach()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var key1 = GetRandomString(5);
            var value1 = GetRandomString();
            var key2 = GetRandomString(5);
            var value2 = GetRandomString();
            var collected = new List<object>();
            sut.Add(key1, value1);
            sut.Add(key2, value2);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            foreach (var val in sut)
                collected.Add(val);

            //--------------- Assert -----------------------
            Expect(collected, Does.Contain(key1));
            Expect(collected, Does.Contain(key2));
        }

        [Test]
        public void Timeout_ShouldBeReadWrite()
        {
            //--------------- Arrange -------------------
            var expected = GetRandomInt(10, 20);
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Timeout = expected;
            var result = sut.Timeout;

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(expected));
        }


        [Test]
        public void Remove_ShouldRemoveItemByKey()
        {
            //--------------- Arrange -------------------
            var key = GetRandomString(5);
            var value = GetRandomString();
            var sut = Create();
            sut.Add(key, value);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Remove(key);

            //--------------- Assert -----------------------
            Expect(sut.Keys, Does.Not.Contain(key));
        }

        [Test]
        public void Item_set_ShouldSetItem()
        {
            //--------------- Arrange -------------------
            var key = GetRandomString(5);
            var value = GetRandomString();
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut[key] = value;

            //--------------- Assert -----------------------
            Expect(sut[key], Is.EqualTo(value));
        }

        [Test]
        public void Clear_ShouldClearAndMarkFlag()
        {
            //--------------- Arrange -------------------
            var key = GetRandomString(5);
            var value = GetRandomString();
            var sut = Create();
            sut[key] = value;

            //--------------- Assume ----------------
            Expect(sut.ClearWasCalled, Is.False);

            //--------------- Act ----------------------
            sut.Clear();

            //--------------- Assert -----------------------
            Expect(sut.ClearWasCalled, Is.True);
            Expect(sut, Is.Empty);
        }



        private FakeHttpSessionState Create(
            SessionStateItemCollection items = null)
        {
            return new FakeHttpSessionState(items ?? new SessionStateItemCollection());
        }
    }
}