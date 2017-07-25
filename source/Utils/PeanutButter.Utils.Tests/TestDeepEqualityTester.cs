using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestDeepEqualityTester : AssertionHelper
    {
        // mostly this class is tested through the DeepEquals()
        //  extension method testing. However, I'd like to allow
        //  for a slower operation where discrepencies are recorded
        [Test]
        public void AreDeepEqual_GivenTwoEqualPrimitives_ShouldNotPopulateErrors()
        {
            //--------------- Arrange -------------------
            var sut = Create(1, 1);

            //--------------- Assume ----------------
            Expect(sut.Errors, Is.Empty);

            //--------------- Act ----------------------
            Expect(sut.AreDeepEqual(), Is.True);

            //--------------- Assert -----------------------
            Expect(sut.Errors, Is.Empty);
        }

        [Test]
        public void AreDeepEqual_GivenTwoDifferentPrimitives_ShouldSetExpectedError()
        {
            //--------------- Arrange -------------------
            var sut = Create(true, false);
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(sut.AreDeepEqual(), Is.False);

            //--------------- Assert -----------------------
            Expect(sut.Errors, Does.Contain("Primitive values differ"));
        }

        [Test]
        public void AreDeepEqual_GivenDifferingComplexObjectsWithOnePropertyOfSameNameAndValue_ShouldRecordError()
        {
            //--------------- Arrange -------------------
            var item1 = new { foo = 1 };
            var item2 = new { foo = 2 };
            var sut = Create(item1, item2);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(sut.AreDeepEqual(), Is.False);

            //--------------- Assert -----------------------
            var error = sut.Errors.Last();
            Expect(error, Does.Contain("foo"));
            Expect(error, Does.Contain("1"));
            Expect(error, Does.Contain("2"));
        }

        [Test]
        public void AreDeepEqual_WhenBothItemsHaveCollections_ShouldCompareThem_Positive()
        {
            //--------------- Arrange -------------------
            var item1 = new 
            {
                Subs = new[] { 1, 2, }
            };
            var item2 = new ThingWithCollection()
            {
                Subs = new[] { 1, 2 }
            };
            var sut = Create(item1, item2);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(sut.AreDeepEqual(), Is.True);

            //--------------- Assert -----------------------
        }

        [Test]
        public void AreDeepEqual_WhenBothItemsHaveCollections_ShouldCompareThem_WithoutCaringAboutOrder()
        {
            //--------------- Arrange -------------------
            var item1 = new ThingWithCollection()
            {
                Subs = new[] { 2, 1, }
            };
            var item2 = new 
            {
                Subs = new[] { 1, 2 }
            };
            var sut = Create(item1, item2);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(sut.AreDeepEqual(), Is.True);

            //--------------- Assert -----------------------
        }

        [Test]
        public void AreDeepEqual_WhenBothItemsHaveCollections_ShouldCompareThem_Negative()
        {
            //--------------- Arrange -------------------
            var item1 = new ThingWithCollection()
            {
                Subs = new[] { 1, 2, 3 }
            };
            var item2 = new ThingWithCollection()
            {
                Subs = new[] { 1, 2 }
            };
            var sut = Create(item1, item2);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(sut.AreDeepEqual(), Is.False);

            //--------------- Assert -----------------------
        }

        [Test]
        public void AreDeepEqual_WhenBothItemsHaveCollections_ButOneIsNull_ShouldNotBarf()
        {
            //--------------- Arrange -------------------
            var item1 = new ThingWithCollection()
            {
                Subs = new[] { 1, 2, 3 }
            };
            var item2 = new ThingWithCollection();
            var sut = Create(item1, item2);

            //--------------- Assume ----------------
            Expect(item2.Subs, Is.Null);

            //--------------- Act ----------------------
            Expect(sut.AreDeepEqual(), Is.False);

            //--------------- Assert -----------------------
        }

        [Test]
        public void AreDeepEqual_WhenBothItemsHaveCollections_ButOneIsNull_ShouldNotBarfReversed()
        {
            //--------------- Arrange -------------------
            var item1 = new ThingWithCollection();
            var item2 = new ThingWithCollection()
            {
                Subs = new[] { 1, 2, 3 }
            };
            var sut = Create(item1, item2);

            //--------------- Assume ----------------
            Expect(item1.Subs, Is.Null);

            //--------------- Act ----------------------
            Expect(sut.AreDeepEqual(), Is.False);

            //--------------- Assert -----------------------
        }

        [Test]
        public void AreDeepEqual_WhenBothItemsHaveCollections_ButBothAreNull_ShouldBehaveAccordingly()
        {
            //--------------- Arrange -------------------
            var item1 = new ThingWithCollection();
            var item2 = new ThingWithCollection();
            var sut = Create(item1, item2);

            //--------------- Assume ----------------
            Expect(item1.Subs, Is.Null);
            Expect(item2.Subs, Is.Null);

            //--------------- Act ----------------------
            Expect(sut.AreDeepEqual(), Is.True);

            //--------------- Assert -----------------------
        }


        [Test]
        [Ignore("WIP: for now, short-circuit is ok; I'd like to use this in PropertyAssert though, which attempts to be more explicit about failures")]
        public void AreDeepEqual_WhenReportingIsEnabled_ShouldNotShortCircuitTests()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------

            //--------------- Assert -----------------------
            Assert.Fail("Test Not Yet Implemented");
        }

        [Test]
        public void DeepEquals_ShouldNotDerpRoundTwo() {
            // Arrange
            var left = JsonConvert.DeserializeObject<PublishableJourneyDto>(_left);
            var right = JsonConvert.DeserializeObject<PublishableJourneyDto>(_right);
            // Act
            var result = left.DeepEquals(right);
            // Assert
            Expect(result, Is.False);
        }

        private static string _left =
                @"{""AudiencesIncluded"":""5"",""AudiencesExcluded"":""55555"",""Id"":""c623ce7f-4e7e-4cdc-b4b0-68cf2b26b778"",""Name"":""Journey 5"",""Version"":{""Major"":1,""Minor"":0},""StartDate"":""2017-07-25T10:54:14.115024+02:00"",""EndDate"":""2017-07-26T23:59:59"",""CanContinueAfterEndDate"":false,""NoEndDate"":false,""IsExternalTimingEnabled"":false,""MaxTimesUserCanJoinJourney"":-1,""MaxAllowedUsersOnJourney"":-1,""MaxTimeOnJourney"":""10.00:00:00"",""StartStepId"":""e2dd57a7-3ea6-466f-83da-afc6dbd9d2a1"",""Steps"":[{""Id"":""e2dd57a7-3ea6-466f-83da-afc6dbd9d2a1"",""StepTypeName"":""WaitForPrimaryEvent"",""JsonStepConfiguration"":""{\""Id\"":\""e2dd57a7-3ea6-466f-83da-afc6dbd9d2a1\"",\""PrimaryEvent\"":{\""NextStepId\"":null,\""TimeoutStepId\"":null,\""ShouldCountdownTime\"":false,\""WaitEventType\"":0,\""WaitTime\"":\""01:00:00\"",\""TimeRemaining\"":\""01:00:00\"",\""Trigger\"":{\""EventType\"":\""deposit\"",\""Condition\"":\""transactioninfo.transactionstatus='Success'\""},\""SuccessActions\"":null,\""FailureActions\"":null}}"",""StepConfiguration"":""{\""Id\"":\""e2dd57a7-3ea6-466f-83da-afc6dbd9d2a1\"",\""PrimaryEvent\"":{\""NextStepId\"":null,\""TimeoutStepId\"":null,\""ShouldCountdownTime\"":false,\""WaitEventType\"":0,\""WaitTime\"":\""01:00:00\"",\""TimeRemaining\"":\""01:00:00\"",\""Trigger\"":{\""EventType\"":\""deposit\"",\""Condition\"":\""transactioninfo.transactionstatus='Success'\""},\""SuccessActions\"":null,\""FailureActions\"":null}}""}],""InitiatingTriggers"":[{""EventType"":""deposit"",""Condition"":null}],""TerminatingTriggers"":[{""EventType"":""logout"",""Condition"":null}]}"
            ;

        private static string _right =
                @"{""AudiencesIncluded"":""5"",""AudiencesExcluded"":"""",""Id"":""c623ce7f-4e7e-4cdc-b4b0-68cf2b26b778"",""Name"":""Journey 5"",""Version"":{""Major"":1,""Minor"":0},""StartDate"":""2017-07-25T10:54:14.115024+02:00"",""EndDate"":""2017-07-26T23:59:59"",""CanContinueAfterEndDate"":false,""NoEndDate"":false,""IsExternalTimingEnabled"":false,""MaxTimesUserCanJoinJourney"":-1,""MaxAllowedUsersOnJourney"":-1,""MaxTimeOnJourney"":""10.00:00:00"",""StartStepId"":""e2dd57a7-3ea6-466f-83da-afc6dbd9d2a1"",""Steps"":[{""Id"":""e2dd57a7-3ea6-466f-83da-afc6dbd9d2a1"",""StepTypeName"":""WaitForPrimaryEvent"",""JsonStepConfiguration"":""{\""Id\"":\""e2dd57a7-3ea6-466f-83da-afc6dbd9d2a1\"",\""PrimaryEvent\"":{\""NextStepId\"":null,\""TimeoutStepId\"":null,\""ShouldCountdownTime\"":false,\""WaitEventType\"":0,\""WaitTime\"":\""01:00:00\"",\""TimeRemaining\"":\""01:00:00\"",\""Trigger\"":{\""EventType\"":\""deposit\"",\""Condition\"":\""transactioninfo.transactionstatus='Success'\""},\""SuccessActions\"":null,\""FailureActions\"":null}}"",""StepConfiguration"":""{\""Id\"":\""e2dd57a7-3ea6-466f-83da-afc6dbd9d2a1\"",\""PrimaryEvent\"":{\""NextStepId\"":null,\""TimeoutStepId\"":null,\""ShouldCountdownTime\"":false,\""WaitEventType\"":0,\""WaitTime\"":\""01:00:00\"",\""TimeRemaining\"":\""01:00:00\"",\""Trigger\"":{\""EventType\"":\""deposit\"",\""Condition\"":\""transactioninfo.transactionstatus='Success'\""},\""SuccessActions\"":null,\""FailureActions\"":null}}""}],""InitiatingTriggers"":[{""EventType"":""deposit"",""Condition"":null}],""TerminatingTriggers"":[{""EventType"":""logout"",""Condition"":null}]}"
            ;


        public class ThingWithCollection
        {
            public ICollection<int> Subs { get; set; }
        }

        private DeepEqualityTester Create(object obj1, object obj2)
        {
            var sut = new DeepEqualityTester(obj1, obj2);
            sut.RecordErrors = true;
            return sut;
        }

        // TODO: obsfucate me, perhaps?
        public class PublishableJourneyDto {
            // Metadata properties
            public string Id { get; set; }
            public string Name { get; set; }

            public VersionDto Version { get; set; }

            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool CanContinueAfterEndDate { get; set; }
            public bool NoEndDate { get; set; }
            public bool IsExternalTimingEnabled { get; set; }
            public int MaxTimesUserCanJoinJourney { get; set; }
            public int MaxAllowedUsersOnJourney { get; set; }
            public TimeSpan MaxTimeOnJourney { get; set; }

            public string AudiencesIncluded;
            public string AudiencesExcluded;

            // Workflow properties
            public string StartStepId { get; set; }
            // these are lists only because RestSharp says "moo"
            public List<PublishableJourneyStepDto> Steps { get; set; }
            public List<PublishableEventTriggerDto> InitiatingTriggers { get; set; }
            public List<PublishableEventTriggerDto> TerminatingTriggers { get; set; }
        }

        public class VersionDto {
            public int Major { get; set; }
            public int Minor { get; set; }
        }

        public class PublishableJourneyStepDto {
            public string Id { get; set; }
            public string StepTypeName { get; set; }
            public string JsonStepConfiguration { get; set; }

            // TODO Added because Publish API contract has changed
            // TODO Removed JsonStepConfiguration once new API has been deployed everywhere ****NOTE that when doing this the correct N1QL scripts needs to be created to migrate data in Couchbase
            public string StepConfiguration {
                get { return JsonStepConfiguration; }
                set { 
                    // intentionally empty, will be implemented when JsonStepConfiguration is removed
                }
            }
        }
        public class PublishableEventTriggerDto {
            public string EventType { get; set; }
            public string Condition { get; set; }
        }

    }
}
