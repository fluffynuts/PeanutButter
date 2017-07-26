using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;

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
            var item1 = new {foo = 1};
            var item2 = new {foo = 2};
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
                Subs = new[] {1, 2,}
            };
            var item2 = new ThingWithCollection()
            {
                Subs = new[] {1, 2}
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
                Subs = new[] {2, 1,}
            };
            var item2 = new
            {
                Subs = new[] {1, 2}
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
                Subs = new[] {1, 2, 3}
            };
            var item2 = new ThingWithCollection()
            {
                Subs = new[] {1, 2}
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
                Subs = new[] {1, 2, 3}
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
                Subs = new[] {1, 2, 3}
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
        [Ignore(
            "WIP: for now, short-circuit is ok; I'd like to use this in PropertyAssert though, which attempts to be more explicit about failures")]
        public void AreDeepEqual_WhenReportingIsEnabled_ShouldNotShortCircuitTests()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------

            //--------------- Assert -----------------------
            Assert.Fail("Test Not Yet Implemented");
        }

        [Test]
        public void DeepEquals_ShouldNotDerpRoundTwo()
        {
            // Arrange
            var left = new ThingWithField() {StringField = GetRandomString()};
            var right = new ThingWithField() {StringField = GetAnother(left.StringField)};
            // Act
            var result = left.DeepEquals(right);
            // Assert
            Expect(result, Is.False);
        }

        [Test]
        public void DeepEquals_ShouldReturnTrueForCaseInWild()
        {
            // Arrange
            var original = GetRandom<JourneyDto>();
            var encoded = JourneyDtoWithEncodedGraphData.From(original);
            // Pre-Assert
            PropertyAssert.AreDeepEqual(original.AuthorData, encoded.AuthorData);
            PropertyAssert.AreDeepEqual(original.MetaData, encoded.MetaData);
            // Act
            var decoded = encoded.Decode();
            // Assert
            PropertyAssert.AreDeepEqual(decoded.AuthorData, original.AuthorData);
            PropertyAssert.AreDeepEqual(decoded.MetaData, original.MetaData);
            PropertyAssert.AreDeepEqual(decoded.GraphData, original.GraphData);
        }

        [Test]
        public void DeepEquals_WildCaseExactReplica()
        {
            // Arrange
            var journey = GetRandom<JourneyDto>();
            var encoded = new JourneyDtoWithEncodedGraphData();
            journey.CopyPropertiesTo(encoded);
            encoded.GraphDataEncoded = DataUriFor(journey.GraphData);
            encoded.GraphData = null;

            // Pre-assert

            // Act
            var result = encoded.Decode();

            // Assert
            PropertyAssert.AreDeepEqual(result, journey);

        }

        private string DataUriFor<T>(T input) {
            var json = JsonConvert.SerializeObject(input);
            var encoded = json.AsBytes().ToBase64();
            return $"data:application/json;base64,{encoded}";
        }

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

        public class ThingWithField
        {
            public string StringField;
        }

        // TODO: obsfucate me
        public class JourneyDto
        {
            public Guid Id { get; set; }

            public string Name { get; set; }

            public int MajorVersion { get; set; }
            public int MinorVersion { get; set; }

            public MetaDataDto MetaData { get; set; }
            public GraphDataDto GraphData { get; set; }
            public AuthorDto AuthorData { get; set; }

            public const int NameMaxLength = 100;
        }
        public class AuthorDto {
            public DateTime Created { get; set; }
            public DateTime LastModified { get; set; }
            public long OperatorId { get; set; }
            public long CreatedById { get; set; }
            public string CreatedBy { get; set; }
            public long LastModifiedById { get; set; }
            public string LastModifiedBy { get; set; }
        }

        public class GraphDataDto
        {
            // TODO: generalize down to DehydratedWidget
            //  as that type can support things which aren't wait events,
            //  such as, eg (at some point) decision diamonds
            public WaitEventWidget[] Widgets { get; set; }

            public DehydratedConnection[] Connections { get; set; }
        }

        public class DehydratedConnection
        {
            public string SourceId { get; set; }
            public string TargetId { get; set; }
        }
    }

    public class WaitEventWidget : DehydratedWidget
    {
        // -> backend WaitEvent
        public string EventType { get; set; }

        public Guid EventTypeId { get; set; }
        public EventCondition[] Conditions { get; set; }
        public ConditionalAction[] SuccessActions { get; set; }
    }

    public class EventCondition
    {
        // -> backend: Trigger
        public Option Condition { get; set; }

        public Option Operator { get; set; }
        public Option[] SelectedValues { get; set; }
    }

    public class Option
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

    public class ConditionalAction
    {
        // -> backend: StepAction
        public Guid Id { get; set; } // -> this conditional action's id

        public Guid ActionId { get; set; } // -> maps to the id of the source action
        public string ActionName { get; set; }
        public Dictionary<string, object[]> Settings { get; set; }
        public AdminEventGamingServerKeyValuePair[] AdminEvents { get; set; }
    }

    public class AdminEventGamingServerKeyValuePair
    {
        public NamedIdentifier Event { get; set; }
        public NamedIdentifier Server { get; set; }
    }

    public class NamedIdentifier
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DehydratedWidget
    {
        // -> used at client only
        public string Id { get; set; } // often a guid, but may be a static like "stateBegin"

        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string PresenterElement { get; set; }
    }

    public class MetaDataDto
    {
        public AudienceDto Audience { get; set; }
        public ScheduleDto Schedule { get; set; }
        public BudgetDto Budget { get; set; }
    }

    public class AudienceDto
    {
        public string[] IncludePlayers { get; set; }
        public string[] ExcludePlayers { get; set; }
    }

    public class ScheduleDto
    {
        public RecurrenceDto Recurrence { get; set; }
        public DateTime Start { get; set; }

        public DateTime End { get; set; }
        public bool NoEndDate { get; set; }
        public bool CanContinueAfterEndDate { get; set; }
        public TimezoneDto Timezone { get; set; }
    }

    public class RecurrenceDto
    {
        // TODO: complete this
        public RecurrenceFrequency Frequency { get; set; }

        public int Interval { get; set; }
    }

    public enum RecurrenceFrequency
    {
        Daily,
        Weekly,
        Monthly
    }

    public class TimezoneDto
    {
        public TimezoneType TimezoneType { get; set; }
        public string UtcOffset { get; set; }
    }

    public enum TimezoneType
    {
        Dynamic,
        UtcOffset
    }

    public class BudgetDto
    {
        public int? MaxPlayersAllowed { get; set; }
        public bool IsMaxPlayersAllowedUnlimited { get; set; }

        public int? PlayerReentryLimit { get; set; }
        public bool IsPlayerReentryLimitUnlimited { get; set; }

        public int PlayerDurationDaysLimit { get; set; }

        public bool ResetPlayerLimits { get; set; }
        public bool ResetRuleLimits { get; set; }

        public const int PlayerDurationDaysLimitMinimum = 1;
        public const int PlayerDurationDaysLimitMaximum = 90;
    }
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class JourneyDtoWithEncodedGraphData : TestDeepEqualityTester.JourneyDto {
        // ReSharper disable once MemberCanBePrivate.Global
        public string GraphDataEncoded { get; set; }

        private readonly object _lock = new object();

        public TestDeepEqualityTester.JourneyDto Decode() {
            var result = GetStartingJourney();
            if (GraphDataEncoded == null)
                return result;
            lock (_lock) {
                result.GraphData = JsonConvert.DeserializeObject<TestDeepEqualityTester.GraphDataDto>(
                    Convert.FromBase64String(GetDataPartOf(GraphDataEncoded))
                        .ToUTF8String());
            }
            return result;
        }

        private readonly Regex _dataUriRegex = new Regex(@"^(data:application/json;base64,)(?<data>.*)");

        private string GetDataPartOf(string graphDataEncoded) {
            var match = _dataUriRegex.Match(graphDataEncoded);
            var dataGroup = match.Groups["data"];
            if (!dataGroup.Success) {
                throw new InvalidDataException($"Expected to find a base64-encoded data uri, but got:\n${graphDataEncoded}");
            }

            return dataGroup.Value;
        }

        private static readonly PropertyInfo[] _journeyProps =
            typeof(TestDeepEqualityTester.JourneyDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        private static readonly PropertyInfo[] _myProps =
            typeof(JourneyDtoWithEncodedGraphData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        private TestDeepEqualityTester.JourneyDto GetStartingJourney() {
            var journey = new TestDeepEqualityTester.JourneyDto();
            foreach (var propertyInfo in _journeyProps) {
                var match = _myProps.First(pi => pi.Name == propertyInfo.Name);
                var myVal = match.GetValue(this);
                propertyInfo.SetValue(journey, myVal);
            }
            return journey;
        }

        public static JourneyDtoWithEncodedGraphData From(TestDeepEqualityTester.JourneyDto journey) {
            var result = new JourneyDtoWithEncodedGraphData();
            foreach (var propertyInfo in _journeyProps) {
                var match = _myProps.First(pi => pi.Name == propertyInfo.Name);
                var srcVal = match.GetValue(journey);
                propertyInfo.SetValue(result, srcVal);
            }
            result.GraphDataEncoded = 
                $"data:application/json;base64,{Convert.ToBase64String(JsonConvert.SerializeObject(journey.GraphData).AsBytes())}";
            return result;
        }
    }
    public class JourneyDtoTestDataBuilder : GenericBuilder<JourneyDtoTestDataBuilder, TestDeepEqualityTester.JourneyDto> {
        public JourneyDtoTestDataBuilder WithCreatedDate(DateTime date) {
            return WithProp(dto => dto.AuthorData.Created = date);
        }

        public JourneyDtoTestDataBuilder WithCreatedById(int id) {
            return
                WithProp(dto => dto.AuthorData = dto.AuthorData ?? GetRandom<TestDeepEqualityTester.AuthorDto>())
                    .WithProp(dto => dto.AuthorData.CreatedById = id);
        }

        public JourneyDtoTestDataBuilder WithName(string name) {
            return WithProp(o => o.Name = name);
        }

        public JourneyDtoTestDataBuilder WithInvalidProps() {
            return WithRandomProps()
                .WithName("")
                .WithProp(dto => dto.MetaData.Audience.IncludePlayers = new string[0])
                .WithProp(dto => dto.MetaData.Schedule.End = DateTime.MinValue)
                .WithProp(dto => dto.MetaData.Schedule.Start = DateTime.MaxValue)
                .WithProp(dto => dto.MetaData.Budget.IsMaxPlayersAllowedUnlimited = false)
                .WithProp(dto => dto.MetaData.Budget.MaxPlayersAllowed = int.MinValue)
                .WithProp(dto => dto.MetaData.Budget.IsPlayerReentryLimitUnlimited = false)
                .WithProp(dto => dto.MetaData.Budget.PlayerReentryLimit = int.MinValue)
                .WithProp(dto => dto.MetaData.Budget.PlayerDurationDaysLimit = int.MinValue);
        }

        public JourneyDtoTestDataBuilder WithAllValidProps() {
            return WithRandomProps()
                .WithName(GetRandomString(5, 15))
                .WithProp(dto => dto.MetaData.Audience.IncludePlayers = new[] { GetRandomString(5, 11) })
                .WithProp(dto => dto.MetaData.Schedule.Start = DateTime.Now)
                .WithProp(dto => dto.MetaData.Schedule.End = DateTime.Now.AddDays(10))
                .WithProp(dto => dto.MetaData.Budget.IsMaxPlayersAllowedUnlimited = false)
                .WithProp(dto => dto.MetaData.Budget.MaxPlayersAllowed = 10)
                .WithProp(dto => dto.MetaData.Budget.IsPlayerReentryLimitUnlimited = false)
                .WithProp(dto => dto.MetaData.Budget.PlayerReentryLimit = 1)
                .WithProp(dto => dto.MetaData.Budget.PlayerDurationDaysLimit = 5);
        }

        public JourneyDtoTestDataBuilder WithNullGraphData() {
            return WithProp(o => o.GraphData = null);
        }

        public JourneyDtoTestDataBuilder WithNullWidgets() {
            return WithProp(o => o.GraphData.Widgets = null);
        }

        public JourneyDtoTestDataBuilder WithEmptyWidgets() {
            return WithProp(o => o.GraphData.Widgets = new WaitEventWidget[0]);
        }

        public JourneyDtoTestDataBuilder WithNullConnections() {
            return WithProp(o => o.GraphData.Connections = null);
        }

        public JourneyDtoTestDataBuilder WithEmptyConnections() {
            return WithProp(o => o.GraphData.Connections = new TestDeepEqualityTester.DehydratedConnection[0]);
        }

        public JourneyDtoTestDataBuilder WithNoInitiatingTrigger() {
            return WithProp(o => o.GraphData.Connections =
                o.GraphData.Connections
                    .EmptyIfNull()
                    .Where(c => c.SourceId != "stateBegin")
                    .ToArray()
            );
        }

        public JourneyDtoTestDataBuilder WithUnconfiguredAction() {
            return WithProp(o => {
                var widget = o.GraphData.Widgets.Second();
                widget.SuccessActions.First().Settings["Moo"] = GetRandomBoolean() ? null : new object[] { };
            });
        }

        public JourneyDtoTestDataBuilder WithUnconfiguredCondition() {
            return WithProp(o => {
                var widget = o.GraphData.Widgets.Second();
                widget.Conditions.First().SelectedValues = GetRandomBoolean() ? null : new Option[0];
            });
        }

        public JourneyDtoTestDataBuilder WithMultipleEventsButNoActions() {
            return WithProp(o => {
                o.GraphData.Widgets.ForEach(w => {
                    w.SuccessActions = null;
                });
            });
        }
        public class EventConditionBuilder : GenericBuilder<EventConditionBuilder, EventCondition> {
            public override EventConditionBuilder WithRandomProps() {
                return base.WithRandomProps()
                    .WithProp(condition => condition.Condition = RandomValueGen.GetRandom<Option>())
                    .WithProp(condition => condition.Operator = RandomValueGen.GetRandom<Option>())
                    .WithProp(condition => condition.SelectedValues = RandomValueGen.GetRandomCollection<Option>(1).ToArray());
            }
        }

        public class ConditionalActionBuilder : GenericBuilder<ConditionalActionBuilder, ConditionalAction> {
            public override ConditionalActionBuilder WithRandomProps() {
                return WithProp(o => o.ActionId = Guid.NewGuid())
                    .WithProp(o => o.ActionName = RandomValueGen.GetRandomString(4))
                    .WithProp(o => o.Id = Guid.NewGuid())
                    .WithProp(o => o.Settings = new Dictionary<string, object[]>() {
                        [RandomValueGen.GetRandomString(2, 4)] = RandomValueGen.GetRandomCollection<int>(2).Cast<object>().ToArray()
                    });
            }
        }

        public class WaitEventWidgetDtoBuilder : GenericBuilder<WaitEventWidgetDtoBuilder, WaitEventWidget> {
            public override WaitEventWidgetDtoBuilder WithRandomProps() {
                return base.WithRandomProps()
                    .WithGuidId()
                    .WithRandomConditions()
                    .WithRandomSuccessActions();
            }

            public WaitEventWidgetDtoBuilder WithGuidId() {
                return WithProp(o => o.Id = $"{Guid.NewGuid()}");
            }
            private readonly string _depositId = Guid.NewGuid().ToString();
            public WaitEventWidgetDtoBuilder AsDeposit() {
                return WithGuidId()
                    .WithEventTypeId(_depositId)
                    .WithClientEventType("Deposit");  // this is what the client gets; it's only interesting to humans -- we'll use the id
            }

            public WaitEventWidgetDtoBuilder WithClientEventType(string clientSideEventType) {
                return WithProp(o => o.EventType = clientSideEventType);
            }

            public WaitEventWidgetDtoBuilder WithEventTypeId(string guid) {
                return WithProp(o => o.EventTypeId = Guid.Parse(guid));
            }

            public WaitEventWidgetDtoBuilder WithRandomSuccessActions() {
                return WithProp(o =>
                    WithSuccessActions(GetRandomCollection<ConditionalAction>(1, 3).ToArray())
                );
            }

            public WaitEventWidgetDtoBuilder WithId(string id) {
                return WithProp(o => o.Id = id);
            }

            public WaitEventWidgetDtoBuilder WithSuccessActions(params ConditionalAction[] actions) {
                return WithProp(o =>
                    o.SuccessActions = o.SuccessActions.EmptyIfNull().And(actions)
                );
            }

            public WaitEventWidgetDtoBuilder WithRandomConditions() {
                return WithProp(o =>
                    WithConditions(GetRandomCollection<EventCondition>(1, 3).ToArray())
                );
            }

            public WaitEventWidgetDtoBuilder WithConditions(params EventCondition[] conditions) {
                return WithProp(o => o.Conditions = o.Conditions.EmptyIfNull().And(conditions));
            }
        }

        public class GraphDataDtoTestDataBuilder : GenericBuilder<GraphDataDtoTestDataBuilder, TestDeepEqualityTester.GraphDataDto> {
            private static readonly WaitEventWidget StartState = new WaitEventWidget() {
                Id = "stateBegin",
                EventTypeId = Guid.Empty,
                PresenterElement = "pjc-state-begin"
            };

            public override GraphDataDtoTestDataBuilder WithRandomProps() {
                return base.WithRandomProps()
                    .WithStartState()
                    .WithProp(o => {
                        var waitEventWidgets = GetRandomCollection<WaitEventWidget>(2, 4).ToArray();
                        WithWidgets(waitEventWidgets);
                    })
                    .WithLinearProgression();
            }

            private GraphDataDtoTestDataBuilder WithLinearProgression() {
                return WithProp(o => {
                    var start = o.Widgets.FirstOrDefault(w => w.Id == "stateBegin");
                    if (start == null)
                        return;
                    var others = new Queue<WaitEventWidget>(o.Widgets.Except(new[] {start}).ToArray());
                    var last = start;
                    while (others.Count > 0) {
                        var next = others.Dequeue();
                        o.Connections = o.Connections.EmptyIfNull().And(ConnectSuccess(last, next));
                        last = next;
                    }
                });

                TestDeepEqualityTester.DehydratedConnection ConnectSuccess(DehydratedWidget fromWidget, DehydratedWidget toWidget) {
                    return new TestDeepEqualityTester.DehydratedConnection() {
                        SourceId = fromWidget.Id == "stateBegin" ? fromWidget.Id : $"success.{fromWidget.Id}",
                        TargetId = $"wait-event.{toWidget.Id}"
                    };
                }
            }

            public GraphDataDtoTestDataBuilder WithWidgets(params WaitEventWidget[] connections) {
                return WithProp(o =>
                    o.Widgets = o.Widgets.EmptyIfNull().And(connections)
                );
            }

            public GraphDataDtoTestDataBuilder WithConnections(params TestDeepEqualityTester.DehydratedConnection[] connections) {
                return WithProp(o => o.Connections = o.Connections.EmptyIfNull().And(connections));
            }

            public GraphDataDtoTestDataBuilder WithStartState() {
                return WithProp(o => o.Widgets = o.Widgets.And(StartState));
            }
        }

    }


}
