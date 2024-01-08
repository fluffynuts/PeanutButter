using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace PeanutButter.DuckTyping.Tests
{
    [TestFixture]
    public class TestCreate
    {
        public interface ICreateMe
        {
            int Id { get; set; }
            string Name { get; set; }
        }
        [Test]
        public void InstanceOf_InvokedWithInterfaceType_ShouldReturnNewInstanceImplementingThatType()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = Create.InstanceOf<ICreateMe>();

            //--------------- Assert -----------------------
            Expect(result1).Not.To.Be.Null();
            Expect(result1).To.Be.An.Instance.Of<ICreateMe>();
        }

        [Test]
        public void InstanceOf_ShouldReturnSameTypeForMultipleCallsOnSameInterface()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = Create.InstanceOf<ICreateMe>();
            var result2 = Create.InstanceOf<ICreateMe>();

            //--------------- Assert -----------------------
            Expect(result1.GetType()).To.Equal(result2.GetType());
        }

        [Test]
        public void InstanceOf_ShouldReturnInstanceWithWorkingPropertiesAsPerInterface()
        {
            //--------------- Arrange -------------------
            var expectedId = GetRandomInt();
            var expectedName = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = Create.InstanceOf<ICreateMe>();

            //--------------- Assert -----------------------
            Expect(() =>result.Id = expectedId)
                .Not.To.Throw();
            Expect(() => result.Name = expectedName)
                .Not.To.Throw();
            Expect(result.Id).To.Equal(expectedId);
            Expect(result.Name).To.Equal(expectedName);
        }

        [Test]
        public void InstanceOf_DuplicateKeyIssueSeenInWild_ShouldNotOccur()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Assert.DoesNotThrow(() =>
                Create.InstanceOf<ICaptureDetailsTravelRequestActivityParameters>()
            );

            //--------------- Assert -----------------------
        }

        [Test]
        public void InstanceOf_ShouldCreateInstancesAllTheWayDown()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = Create.InstanceOf<ICaptureDetailsTravelRequestActivityParameters>();

            //--------------- Assert -----------------------
            Expect(result.Payload)
                .Not.To.Be.Null();
        }

        public class MyAttribute: Attribute
        {
            public string Data { get; }

            public MyAttribute(string data)
            {
                Data = data;
            }
        }
        public interface IWithCustomAttribute
        {
            [My("goodness")]
            string Name { get; set; }

            [My("Cow")]
            void Moo();
        }

        [Test]
        public void InstanceOf_ShouldCopyPropertyCustomAttributes()
        {
            //--------------- Arrange -------------------
            var result = Create.InstanceOf<IWithCustomAttribute>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var propInfo = result.GetType().GetProperty("Name");
            Expect(propInfo).Not.To.Be.Null();
            var attribs = propInfo.GetCustomAttributes().OfType<MyAttribute>();

            //--------------- Assert -----------------------
            Expect(attribs).Not.To.Be.Empty();
            Expect(attribs.First().Data).To.Equal("goodness");
        }

        [Test]
        public void InstanceOf_ShouldCopyMethodCustomAttributes()
        {
            //--------------- Arrange -------------------
            var result = Create.InstanceOf<IWithCustomAttribute>();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var propInfo = result.GetType().GetMethod("Moo");
            var attribs = propInfo.GetCustomAttributes().OfType<MyAttribute>();

            //--------------- Assert -----------------------
            Expect(attribs).Not.To.Be.Empty();
            Expect(attribs.First().Data).To.Equal("Cow");
        }

        public interface IActor
        {
            Guid Id { get; set; }
            string Name { get; set; }
            string Email { get; set; }
        }

        public interface ITravellerDetails
        {
            string IdNumber { get; set; }
            string[] PassportNumbers { get; set; }
            string MealPreferences { get; set; } // Halaal? Vegan?
            string TravelPreferences { get; set; } // seats near emergency exits for more leg-room?
        }
        public interface ITraveller : IActor, ITravellerDetails
        {
        }

        [Test]
        public void InstanceOf_GivenTypeWithStringArrayProperty_ShouldNotSplode()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Assert.DoesNotThrow(() => Create.InstanceOf<ITraveller>());

            //--------------- Assert -----------------------
        }

        public interface IHasAnArray
        {
            string[] Stuff { get; set; }
        }

        [Test]
        public void InstanceOf_ShouldCreateArrayPropertiesAsEmptyArrays()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = Create.InstanceOf<IHasAnArray>();

            //--------------- Assert -----------------------
            Expect(result.Stuff).Not.To.Be.Null();
            Expect(result.Stuff).To.Be.Empty();
        }

        public interface IHasAList
        {
            List<string> Stuff { get; set; }
        }

        [Test]
        public void InstanceOf_ShouldCreateListPropertiesAsEmptyLists()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = Create.InstanceOf<IHasAnArray>();

            //--------------- Assert -----------------------
            Expect(result.Stuff).Not.To.Be.Null();
            Expect(result.Stuff).To.Be.Empty();
        }

        public class ActivityParameters<T> : ActivityParameters, IActivityParameters<T>
        {
            public T Payload { get; }

            public ActivityParameters(Guid actorId, Guid taskId, T payload)
                : base(actorId, taskId)
            {
                Payload = payload;
            }
        }

        public class ActivityParameters : IActivityParameters
        {
            public Guid ActorId { get; }
            public Guid TaskId { get; }
            public void DoNothing()
            {
                /* does nothing */
            }

            public ActivityParameters(Guid actorId, Guid taskId)
            {
                ActorId = actorId;
                TaskId = taskId;
            }
        }

        public interface IActivityParameters
        {
            Guid ActorId { get; }
            Guid TaskId { get; }
            void DoNothing();
        }

        public interface IActivityParameters<out T> : IActivityParameters
        {
            T Payload { get; }
        }

        public interface ICaptureDetailsTravelRequestActivityParameters :
            IActivityParameters<ITravelRequestDetails>
        {
        }

        public interface ITravelRequestDetails
        {
            DateTime Initiated { get; set; }
            string DepartingFrom { get; set; }
            string TravellingTo { get; set; }
            DateTime ExpectedDeparture { get; set; }
            string PreferredDepartureTime { get; set; }
            DateTime ExpectedReturn { get; set; }
            string PreferredReturnTime { get; set; }
            string Reason { get; set; }
            bool CarRequired { get; set; }
            bool AccomodationRequired { get; set; }
            string AccommodationRequiredNotes { get; set; }
        }


    }
}
