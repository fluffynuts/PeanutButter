using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.DuckTyping.Tests.Extensions;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.DuckTyping.Tests
{
    [TestFixture]
    public class TestCreate : AssertionHelper
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
            Expect(result1, Is.Not.Null);
            Expect(result1, Is.InstanceOf<ICreateMe>());
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
            Expect(result1.GetType(), Is.EqualTo(result2.GetType()));
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
            Expect(() =>
                result.Id = expectedId,
                Throws.Nothing);
            Expect(() =>
                result.Name = expectedName,
                Throws.Nothing);
            Expect(result.Id, Is.EqualTo(expectedId));
            Expect(result.Name, Is.EqualTo(expectedName));
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
            Assert.IsNotNull(result.Payload);
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
            var attribs = propInfo.GetCustomAttributes().OfType<MyAttribute>();

            //--------------- Assert -----------------------
            Expect(attribs, Is.Not.Empty);
            Expect(attribs.First().Data, Is.EqualTo("goodness"));
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
            Expect(attribs, Is.Not.Empty);
            Expect(attribs.First().Data, Is.EqualTo("Cow"));
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



        public class ActivityParameters<T> : ActivityParameters, IActivityParameters<T>
        {
            public T Payload { get; set; }

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
        public interface IActivityParameters<T> : IActivityParameters
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
