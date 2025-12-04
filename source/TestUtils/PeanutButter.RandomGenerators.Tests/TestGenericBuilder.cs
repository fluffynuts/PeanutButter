using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NExpect;
using NUnit.Framework;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;
using static PeanutButter.Utils.PyLike;
using static PeanutButter.RandomGenerators.Tests.RandomTestCycles;
// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable RedundantBoolCompare

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable PossibleNullReferenceException
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.RandomGenerators.Tests;

[TestFixture]
public class TestGenericBuilder
{
    private class SimpleClass
    {
    }

    private class SimpleBuilder : GenericBuilder<SimpleBuilder, SimpleClass>
    {
    }

    [Test]
    public void Create_ReturnsANewInstanceOfTheBuilder()
    {
        //---------------Set up test pack-------------------

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var b1 = SimpleBuilder.Create();
        var b2 = SimpleBuilder.Create();

        //---------------Test Result -----------------------

        Expect(b1)
            .Not.To.Be.Null();
        Expect(b2)
            .Not.To.Be.Null();
        Expect(b1)
            .To.Be.An.Instance.Of<SimpleBuilder>();
        Expect(b2)
            .To.Be.An.Instance.Of<SimpleBuilder>();
        Expect(b1)
            .Not.To.Be(b2);
    }

    [Test]
    public void Build_ReturnsANewInstanceOfTheTargetClass()
    {
        //---------------Set up test pack-------------------
        var builder = SimpleBuilder.Create();
        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var simple1 = builder.Build();
        var simple2 = builder.Build();

        //---------------Test Result -----------------------
        Expect(simple1)
            .Not.To.Be.Null();
        Expect(simple2)
            .Not.To.Be.Null();
        Expect(simple1)
            .To.Be.An.Instance.Of<SimpleClass>();
        Expect(simple2)
            .To.Be.An.Instance.Of<SimpleClass>();
        Expect(simple1)
            .Not.To.Be(simple2);
    }

    public enum SomeValues
    {
        One,
        Two,
        Three,
        Four
    }

    private class NotAsSimpleClass
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool Flag { get; set; }
        public DateTime Created { get; set; }
        public decimal Cost { get; set; }
        public double DoubleValue { get; set; }
        public float FloatValue { get; set; }
        public Guid GuidValue { get; set; }
        public decimal? NullableDecimalValue { get; set; }
        public byte[] ByteArrayValue { get; set; }
        public SomeValues EnumValue { get; set; }
    }

    private class NotAsSimpleBuilder
        : GenericBuilder<NotAsSimpleBuilder, NotAsSimpleClass>
    {
    }

    [Test]
    public void BuildDefault_ReturnsBlankObject()
    {
        //---------------Set up test pack-------------------
        var blank = new NotAsSimpleClass();

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var obj = NotAsSimpleBuilder.BuildDefault();

        //---------------Test Result -----------------------
        Expect(obj)
            .Not.To.Be.Null();
        Expect(obj)
            .To.Be.An.Instance.Of<NotAsSimpleClass>();
        Expect(obj)
            .To.Deep.Equal(blank);
    }

    [Test]
    public void WithRandomProps_SetsRandomValuesForAllProperties()
    {
        //---------------Set up test pack-------------------

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var randomItems = new List<NotAsSimpleClass>();
        for (var i = 0;
             i < NORMAL_RANDOM_TEST_CYCLES;
             i++)
        {
            randomItems.Add(
                NotAsSimpleBuilder.Create()
                    .WithRandomProps()
                    .Build()
            );
        }

        //---------------Test Result -----------------------
        // look for variance
        VarianceAssert.IsVariant<NotAsSimpleClass, string>(
            randomItems,
            "Name"
        );
        VarianceAssert.IsVariant<NotAsSimpleClass, int>(
            randomItems,
            "Value"
        );
        VarianceAssert.IsVariant<NotAsSimpleClass, bool>(
            randomItems,
            "Flag"
        );
        VarianceAssert.IsVariant<NotAsSimpleClass, DateTime>(
            randomItems,
            "Created"
        );
        VarianceAssert.IsVariant<NotAsSimpleClass, decimal>(
            randomItems,
            "Cost"
        );
        VarianceAssert.IsVariant<NotAsSimpleClass, double>(
            randomItems,
            "DoubleValue"
        );
        VarianceAssert.IsVariant<NotAsSimpleClass, float>(
            randomItems,
            "FloatValue"
        );
        VarianceAssert.IsVariant<NotAsSimpleClass, Guid>(
            randomItems,
            "GuidValue"
        );
        VarianceAssert.IsVariant<NotAsSimpleClass, decimal?>(
            randomItems,
            "NullableDecimalValue"
        );
        VarianceAssert.IsVariant<NotAsSimpleClass, byte[]>(
            randomItems,
            "ByteArrayValue"
        );
        VarianceAssert.IsVariant<NotAsSimpleClass, SomeValues>(
            randomItems,
            "EnumValue"
        );
    }

    public class TestCleverRandomStrings
    {
        public string Email { get; set; }
        public string EmailAddress { get; set; }
        public string Url { get; set; }
        public string Website { get; set; }
        public string Phone { get; set; }
        public string Tel { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }
    }

    public class TestCleverRandomStringsBuilder
        : GenericBuilder<TestCleverRandomStringsBuilder,
            TestCleverRandomStrings
        >
    {
    }

    [Test]
    public void BuildRandom_ShouldAttemptToMakeUsefulStringValues()
    {
        //---------------Set up test pack-------------------
        var sut = TestCleverRandomStringsBuilder.BuildRandom();

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        Expect(sut.EmailAddress)
            .To.LookLikeEmailAddress();
        Expect(sut.Email)
            .To.LookLikeEmailAddress();
        Expect(sut.Website)
            .To.LookLikeUrl();
        Expect(sut.Url)
            .To.LookLikeUrl();
        Expect(sut.Fax)
            .To.Be.AllNumeric();
        Expect(sut.Mobile)
            .To.Be.AllNumeric();
        Expect(sut.Tel)
            .To.Be.AllNumeric();
        Expect(sut.Phone)
            .To.Be.AllNumeric();
        //---------------Test Result -----------------------
    }

    public class TestBooleans
    {
        public bool Enabled { get; set; }
        public bool SomeOtherBoolean { get; set; }
    }

    public class TestBooleansBuilder
        : GenericBuilder<TestBooleansBuilder, TestBooleans>
    {
    }

    [Test]
    public void BuildRandom_ShouldSetBooleanEnabledPropertyToTrue()
    {
        // special rule which makes dealing with entities which have an Enabled flag
        //  a little less tedious for the user
        //---------------Set up test pack-------------------
        var items = new List<TestBooleans>();

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        for (var i = 0;
             i < 1000;
             i++)
        {
            items.Add(TestBooleansBuilder.BuildRandom());
        }

        //---------------Test Result -----------------------
        Expect(items)
            .To.Contain.All
            .Matched.By(o => o.Enabled == true);
        VarianceAssert.IsVariant<TestBooleans, bool>(
            items,
            "SomeOtherBoolean"
        );
    }

    private class BuilderInspector
        : GenericBuilder<BuilderInspector, SimpleClass>
    {
        public static string[] Calls => CallsField.ToArray();

        public static void Clear()
        {
            CallsField.Clear();
        }

        private static readonly List<string> CallsField =
            new List<string>();

        public override BuilderInspector WithRandomProps()
        {
            CallsField.Add("WithRandomProps");
            return base.WithRandomProps();
        }

        public override SimpleClass Build()
        {
            CallsField.Add("Build");
            return base.Build();
        }
    }

    [Test]
    public void BuildRandom_CallsWithRandomPropsThenBuildAndReturnsResult()
    {
        //---------------Set up test pack-------------------
        BuilderInspector.Clear();
        Expect(BuilderInspector.Calls)
            .To.Be.Empty();

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        BuilderInspector.BuildRandom();

        //---------------Test Result -----------------------
        Expect(BuilderInspector.Calls)
            .To.Equal(
                new[]
                {
                    "WithRandomProps",
                    "Build"
                }
            );
    }

    public class ComplexMember1
    {
        public string Name { get; set; }
    }

    public class ComplexMember2
    {
        public int Value { get; set; }
    }

    public class ClassWithComplexMembers
    {
        public ComplexMember1 ComplexMember1 { get; set; }
        public virtual ComplexMember2 ComplexMember2 { get; set; }
    }

    private class ClassWithComplexMembersBuilder
        : GenericBuilder<ClassWithComplexMembersBuilder,
            ClassWithComplexMembers>
    {
    }

    [Test]
    public void BuildDefault_SetsComplexMembersToNullValue()
    {
        //---------------Set up test pack-------------------

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var obj = ClassWithComplexMembersBuilder.BuildDefault();

        //---------------Test Result -----------------------
        Expect(obj.ComplexMember1)
            .To.Be.Null();
        Expect(obj.ComplexMember2)
            .To.Be.Null();
    }

    [Test]
    public void
        WithRandomProps_SetsRandomPropertiesForComplexMembersAndTheirProps()
    {
        //---------------Set up test pack-------------------

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var randomItems = new List<ClassWithComplexMembers>();
        for (var i = 0;
             i < NORMAL_RANDOM_TEST_CYCLES;
             i++)
        {
            var randomItem = ClassWithComplexMembersBuilder.BuildRandom();
            Expect(randomItem.ComplexMember1)
                .Not.To.Be.Null();
            Expect(randomItem.ComplexMember2)
                .Not.To.Be.Null();
            randomItems.Add(randomItem);
        }

        //---------------Test Result -----------------------
        Expect(randomItems.Count)
            .To.Equal(
                NORMAL_RANDOM_TEST_CYCLES
            );
        VarianceAssert.IsVariant<ClassWithComplexMembers, ComplexMember1>(
            randomItems,
            "ComplexMember1"
        );
        VarianceAssert.IsVariant<ClassWithComplexMembers, ComplexMember2>(
            randomItems,
            "ComplexMember2"
        );
        var complexMembers1 = randomItems.Select(i => i.ComplexMember1);
        VarianceAssert.IsVariant<ComplexMember1, string>(
            complexMembers1,
            "Name"
        );
        var complexMembers2 = randomItems.Select(i => i.ComplexMember2);
        VarianceAssert.IsVariant<ComplexMember2, int>(
            complexMembers2,
            "Value"
        );
    }

    [Test]
    public void
        WhenUsingExistingBuildersWhichWouldCauseStackOverflow_ShouldAttemptToProtectAgainstStackOverflow()
    {
        //---------------Set up test pack-------------------

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        Expect(ParentWithBuilderBuilder.BuildRandom)
            .Not.To.Throw();
        //---------------Test Result -----------------------
    }

    public class ParentWithBuilder
    {
        public List<ChildWithBuilder> Children { get; set; }

        public ParentWithBuilder()
        {
            Children = new List<ChildWithBuilder>();
        }
    }

    public class ChildWithBuilder
    {
        public ParentWithBuilder Parent { get; set; }
    }

    public class ParentWithBuilderBuilder
        : GenericBuilder<ParentWithBuilderBuilder, ParentWithBuilder>
    {
        public override ParentWithBuilderBuilder WithRandomProps()
        {
            return base.WithRandomProps()
                .WithChild(ChildWithBuilderBuilder.BuildRandom());
        }

        private ParentWithBuilderBuilder WithChild(
            ChildWithBuilder child
        )
        {
            return WithProp(o => o.Children.Add(child));
        }
    }

    public class ChildWithBuilderBuilder
        : GenericBuilder<ChildWithBuilderBuilder, ChildWithBuilder>
    {
    }

    public class Parent
    {
        public Child Child { get; set; }
    }

    public class Child
    {
        public int Id { get; set; }
    }

    public class ParentBuilder : GenericBuilder<ParentBuilder, Parent>
    {
    }

    public class ChildBuilder : GenericBuilder<ChildBuilder, Child>
    {
        public override ChildBuilder WithRandomProps()
        {
            return base.WithRandomProps()
                .WithProp(o => o.Id = 1337);
        }
    }

    [Test]
    public void
        WithRandomProps_ShouldReuseKnownBuildersFromSameAssemblyAsType()
    {
        //---------------Set up test pack-------------------

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var result = ParentBuilder.BuildRandom();

        //---------------Test Result -----------------------
        Expect(result.Child.Id).To.Equal(1337);
    }

    public class EmailBuilder : GenericBuilder<EmailBuilder, Email>
    {
        public override EmailBuilder WithRandomProps()
        {
            return base.WithRandomProps()
                .WithProp(o => o.Subject = "local is lekker");
        }
    }

    public class EmailRecipientBuilder
        : GenericBuilder<EmailRecipientBuilder, EmailRecipient>
    {
    }

    public class EmailRecipient
    {
        public int EmailRecipientId { get; set; }
        public int EmailId { get; set; }
        public string Recipient { get; set; }
        public bool IsPrimaryRecipient { get; set; }
        public bool IsCC { get; set; }
        public bool IsBCC { get; set; }
        public virtual Email Email { get; set; }
    }

    public class Email
    {
        public Email()
        {
            EmailAttachments = new List<EmailAttachment>();
            EmailRecipients = new List<EmailRecipient>();
        }

        public int EmailId { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SendAt { get; set; }
        public int SendAttempts { get; set; }
        public bool Sent { get; set; }
        public string LastError { get; set; }
        public virtual IList<EmailAttachment> EmailAttachments { get; set; }
        public virtual IList<EmailRecipient> EmailRecipients { get; set; }
    }

    public class EmailAttachment
    {
        public int EmailAttachmentId { get; set; }
        public int EmailId { get; set; }
        public string Name { get; set; }
        public bool Inline { get; set; }
        public string ContentID { get; set; }
        public string MIMEType { get; set; }
        public byte[] Data { get; set; }
        public virtual Email Email { get; set; }
    }

    [Test]
    public void
        WithRandomProps_ShouldReuseKnownBuildersFromAllLoadedAssembliesWhenNoBuilderInTypesAssembly()
    {
        //---------------Set up test pack-------------------

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var result = EmailRecipientBuilder.BuildRandom();

        //---------------Test Result -----------------------
        Expect(result.Email.Subject).To.Equal("local is lekker");
    }

    [TestCase(
        "foo",
        "foo",
        0
    )]
    [TestCase(
        "bar",
        "foo",
        -1
    )]
    [TestCase(
        "foo",
        "bar",
        1
    )]
    [TestCase(
        "foo.bar",
        "foo.bar",
        0
    )]
    [TestCase(
        "foo.bar",
        "foo.bar.tests",
        -1
    )]
    [TestCase(
        "foo.bar",
        "foo.bar.tests.part2",
        -2
    )]
    [TestCase(
        "foo.bar.tests",
        "foo.bar",
        1
    )]
    [TestCase(
        "foo.bar.tests.part2",
        "foo.bar",
        2
    )]
    public void MatchIndexFor_GivenArrays_ShouldReturnExpectedResult(
        string left,
        string right,
        int expected
    )
    {
        //---------------Set up test pack-------------------
        var leftParts = left.Split('.');
        var rightParts = right.Split('.');

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var result = leftParts.MatchIndexFor(rightParts);

        //---------------Test Result -----------------------
        Expect(result).To.Equal(expected);
    }

    // real-world usage, has inner exception about defining duplicate dynamic module
    public class FakeMessagePlatformSender
    {
        public string Address { get; set; }
        public string ReplyTo { get; set; }
    }

    public class FakeMessagePlatformRecipient
    {
        public string RecipientType { get; set; }
        public string RecipientIdentity { get; set; }
        public string Address { get; set; }
    }

    public class FakeMessagePlatformData
    {
        public int ClientID { get; set; }
        public IEnumerable<FakeMessagePlatformOption> Options { get; set; }
    }

    public class FakeMessageData
    {
        public FakeMessagePlatformSender Sender { get; set; }

        public IEnumerable<FakeMessagePlatformRecipient> Recipients { get; set; }

        public FakeMessagePlatformData Message { get; set; }
        public IEnumerable<string> Protocols { get; set; }
    }

    public class FakeMessagePlatformOption
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public class FakeMessagePlatformOptionBuilder
        : GenericBuilder<FakeMessagePlatformOptionBuilder,
            FakeMessagePlatformOption>
    {
    }

    public class FakeMessageDataBuilder
        : GenericBuilder<FakeMessageDataBuilder, FakeMessageData>
    {
        public override FakeMessageDataBuilder WithRandomProps()
        {
            return base.WithRandomProps()
                .WithRandomOptions()
                .WithRandomRecipients()
                .WithRandomProtocols();
        }

        private FakeMessageDataBuilder WithRandomOptions()
        {
            return WithProp(
                o => o.Message.Options =
                    GetRandomCollection(
                        FakeMessagePlatformOptionBuilder.BuildRandom,
                        2
                    )
            );
        }

        public FakeMessageDataBuilder AsOneProtocolToOneRecipient()
        {
            return WithRandomProtocols(
                    1,
                    1
                )
                .WithRandomRecipients(
                    1,
                    1
                );
        }

        public FakeMessageDataBuilder WithRandomProtocols(
            int min = 1,
            int max = 10
        )
        {
            return WithProp(
                o => o.Protocols =
                    GetRandomCollection(
                        () => GetRandomString(),
                        min,
                        max
                    )
            );
        }

        public FakeMessageDataBuilder WithRandomRecipients(
            int min = 1,
            int max = 10
        )
        {
            return WithProp(
                o => o.Recipients =
                    GetRandomCollection(
                        FakeMessagePlatformRecipientBuilder
                            .BuildRandom,
                        min,
                        max
                    )
            );
        }

        public FakeMessageDataBuilder WithOption(
            string name,
            string value
        )
        {
            return WithProp(
                o => o.Message.Options = o.Message.Options
                    .EmptyIfNull()
                    .And(
                        new FakeMessagePlatformOption()
                        {
                            Name = name,
                            Value = value
                        }
                    )
            );
        }
    }

    public class FakeMessagePlatformRecipientBuilder
        : GenericBuilder<FakeMessagePlatformRecipientBuilder,
            FakeMessagePlatformRecipient>
    {
    }

    [Test]
    public void ShouldNotThrow()
    {
        //---------------Set up test pack-------------------

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        Expect(
                () => FakeMessageDataBuilder.Create()
                    .WithRandomProps()
                    .WithOption(
                        "message",
                        "hello world"
                    )
                    .WithOption(
                        "user",
                        "bob saget"
                    )
                    .WithOption(
                        "AnotherOption",
                        "wibble socks"
                    )
                    .AsOneProtocolToOneRecipient()
                    .Build()
            )
            .Not.To.Throw();

        //---------------Test Result -----------------------
    }

    //-- end real-world error example

    public class SomePOCOWithCollection
    {
        public virtual ICollection<string> Strings { get; set; }
    }

    public class SomePOCOWithCollectionBuilder
        : GenericBuilder<SomePOCOWithCollectionBuilder,
            SomePOCOWithCollection>
    {
    }

    [Test]
    public void ICollectionShouldBeCreatedEmptyAndNotNull()
    {
        //---------------Set up test pack-------------------
        var sut = SomePOCOWithCollectionBuilder.BuildRandom();
        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var result = sut.Strings;

        //---------------Test Result -----------------------
        Expect(result)
            .To.Be.Empty();
    }

    public class SomePOCOWithArray
    {
        public virtual string[] Strings { get; set; }
    }

    public class SomePOCOWithArrayBuilder
        : GenericBuilder<SomePOCOWithArrayBuilder, SomePOCOWithArray>
    {
    }

    [Test]
    public void ArrayShouldBeCreatedEmptyAndNotNull()
    {
        //---------------Set up test pack-------------------
        var sut = SomePOCOWithArrayBuilder.BuildRandom();
        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var result = sut.Strings;

        //---------------Test Result -----------------------
        Expect(result)
            .To.Be.Empty();
    }

    public class SomePOCOWithList
    {
        public virtual List<string> Strings { get; set; }
    }

    public class SomePOCOWithListBuilder
        : GenericBuilder<SomePOCOWithListBuilder, SomePOCOWithList>
    {
    }

    [Test]
    public void ListShouldBeCreatedEmptyAndNotNull()
    {
        //---------------Set up test pack-------------------
        var sut = SomePOCOWithListBuilder.BuildRandom();
        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var result = sut.Strings;

        //---------------Test Result -----------------------
        Expect(result)
            .To.Be.Empty();
    }

    [Test]
    public void WhenUsing_WithFilledCollections_ShouldPutDataInCollections()
    {
        //---------------Set up test pack-------------------
        var sut = SomePOCOWithListBuilder.Create()
            .WithRandomProps()
            .WithFilledCollections()
            .Build();

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var result = sut.Strings;

        //---------------Test Result -----------------------
        Expect(result)
            .Not.To.Be.Empty();
    }

    public interface IInterfaceWithNoImplementation
    {
        int Id { get; set; }
        string Name { get; set; }
    }

    public class SomeInterfaceWithNoImplementationBuilder
        : GenericBuilder<SomeInterfaceWithNoImplementationBuilder,
            IInterfaceWithNoImplementation>
    {
    }

    [Test]
    public void
        Build_WhenUsingAgainstNotImplementedInterface_WhenNSubstituteInLoadedAssemblies_ShouldConstruct()
    {
        //---------------Set up test pack-------------------
        IInterfaceWithNoImplementation result = null;

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        Assert.DoesNotThrow(
            () =>
                result = SomeInterfaceWithNoImplementationBuilder
                    .BuildRandom()
        );

        //---------------Test Result -----------------------
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Name)
            .Not.To.Be.Null();
    }

    public interface IImplementedInterface
    {
        int Id { get; set; }
        string Name { get; set; }
    }

    public class ImplementingClassWithConstructorParams
        : IImplementedInterface
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ImplementingClassWithConstructorParams(
            int id,
            string name
        )
        {
            Id = id;
            Name = name;
        }
    }

    public class SomeImplementingClass : IImplementedInterface
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SomeImplementingClassBuilder
        : GenericBuilder<SomeImplementingClassBuilder, IImplementedInterface
        >
    {
    }

    [Test]
    public void
        Build_WhenUsingAgainstInteface_WithSingleImplementationWithParameterLessConstructor_ShouldUseIt()
    {
        //---------------Set up test pack-------------------
        var interfaceType = typeof(IImplementedInterface);
        var concrete = typeof(SomeImplementingClass);
        var types = GetType()
            .Assembly
            .ExportedTypes
            .Where(t => interfaceType.IsAssignableFrom(t))
            .ToArray();
        Expect(types)
            .To.Contain(concrete);
        Expect(concrete.IsClass)
            .To.Be.True();
        Expect(concrete.IsAbstract)
            .To.Be.False();
        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var result = SomeImplementingClassBuilder.BuildRandom();

        //---------------Test Result -----------------------
        Expect(result)
            .Not.To.Be.Null()
            .And
            .To.Be.An.Instance.Of<SomeImplementingClass>();
    }

    public interface ITokenValidationResponse
    {
        string Issuer { get; } // Who issued this token (uri)

        string
            Audience { get; } // Uri to audience (with idsvr, this points at resources)

        DateTime Expires { get; } // Token is not valid after this datetime

        DateTime
            NotBefore { get; } // Token is not valid before this datetime

        string
            ClientId { get; } // Id of the connecting Client for which this token was provided

        string[] Scopes { get; } // Scopes requested & loaded into the token

        string
            Subject { get; } // User identifier (ie: UserId for Principal)

        DateTime AuthorizedAt { get; }

        string
            UserName { get; } // only present if profile scope was requested

        string Email { get; } // only present if email scope was requested

        bool EmailVerified { get; } // only present if email scope was requested

        string[] Roles { get; } // only present if roles scope was requested
    }

    public class TokenValidationResponse : ITokenValidationResponse
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public DateTime Expires { get; set; }
        public DateTime NotBefore { get; set; }
        public string ClientId { get; set; }
        public string[] Scopes { get; set; }
        public string Subject { get; set; }
        public DateTime AuthorizedAt { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string[] Roles { get; set; }
    }

    public class LocalTraceListener : TraceListener
    {
        public string AllMessages { get; private set; } = "";

        public override void Write(
            string message
        )
        {
            AllMessages += message;
        }

        public override void WriteLine(
            string message
        )
        {
            AllMessages += message + "\n";
        }
    }

    [Test]
    public void Randomize_ShouldNotTraceLogWhenPropertiesAreWritable()
    {
        //--------------- Arrange -------------------
        var listener = new LocalTraceListener();
        using (new AutoResetter(
                   InstallListener(listener),
                   RemoveListener(listener)
               ))
        {
            //--------------- Assume ----------------
            Expect(listener.AllMessages)
                .To.Equal("");

            //--------------- Act ----------------------
            var random = GetRandom<TokenValidationResponse>();

            //--------------- Assert -----------------------
            Expect(random)
                .Not.To.Be.Null();
            Expect(listener.AllMessages)
                .Not.To.Contain("not writable");
        }
    }

    private Action InstallListener(
        LocalTraceListener listener
    )
    {
        return () => Trace.Listeners.Add(listener);
    }

    private Action RemoveListener(
        LocalTraceListener listener
    )
    {
        return () => Trace.Listeners.Remove(listener);
    }

    public class Node
    {
        public Node Child { get; set; }
        public Node[] Siblings { get; set; }
        public IEnumerable<Node> Enemies { get; set; }
    }

    public class NodeBuilder : GenericBuilder<NodeBuilder, Node>
    {
    }


    [Test]
    public void
        GenericBuild_ShouldReturnObjectWithDefaultProps_IncludingNullsForComplexTypes()
    {
        // Arrange
        // Pre-Assert
        // Act
        var result = NodeBuilder.Create()
            .GenericBuild() as Node;
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Child)
            .To.Be.Null();
        Expect(result.Siblings)
            .To.Be.Null();
    }

    [Test]
    public void
        GenericDeepBuild_ShouldReturnObjectWithAllComplexObjectsFilled()
    {
        // Arrange
        // Pre-Assert
        // Act
        var result = NodeBuilder.Create()
            .GenericDeepBuild() as Node;
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Child)
            .Not.To.Be.Null();
        Expect(result.Siblings)
            .Not.To.Be.Null();
        Expect(result.Enemies)
            .Not.To.Be.Null();
    }

    public struct SomeStruct
    {
        public int Id;
        public string Name;
    }

    public class SomeStructBuilder
        : GenericBuilder<SomeStructBuilder, SomeStruct>
    {
    }

    [Test]
    public void WithRandomProps_ShouldPopulateFieldsWithRandomValues()
    {
        // Arrange
        // Pre-assert
        // Act
        var result = Range(
                0,
                10
            )
            .Select(
                _ => SomeStructBuilder.Create()
                    .WithRandomProps()
                    .Build()
            )
            .ToArray();
        // Assert
        Expect(result.All(o => o.Equals(default(SomeStruct))))
            .To.Be.False();
    }

    public class SomeClass
    {
        public SomeStruct Moo { get; }

        public SomeClass(
            SomeStruct moo
        )
        {
            Moo = moo;
        }
    }

    [Test]
    public void ShouldConstructWithRandomStructParameter()
    {
        // Arrange
        // Pre-assert
        // Act
        var result = Range(
                0,
                10
            )
            .Select(
                _ => GetRandom<SomeClass>()
            )
            .ToArray();

        // Assert
        Expect(result.All(o => o.Moo.Equals(default(SomeStruct))))
            .To.Be.False();
    }

    public class SomeClassWithDateProp
    {
        public DateTime DateTime { get; set; }
    }

    public class SomeClassWithDatePropBuilder
        : GenericBuilder<SomeClassWithDatePropBuilder, SomeClassWithDateProp
        >
    {
    }

    [TestCase(
        DateTimeKind.Local,
        DateTimeKind.Local
    )]
    [TestCase(
        DateTimeKind.Utc,
        DateTimeKind.Utc
    )]
    [TestCase(
        DateTimeKind.Unspecified,
        DateTimeKind.Local
    )]
    public void ShouldSetDefaultDateTimeKindFromMethod(
        DateTimeKind set,
        DateTimeKind expected
    )
    {
        // Arrange
        // Pre-assert
        // Act
        var result = SomeClassWithDatePropBuilder.Create()
            .WithDefaultDateTimeKind(set)
            .WithRandomProps()
            .Build();
        // Assert
        Expect(result.DateTime.Kind)
            .To.Equal(expected);
    }

    [TestFixture]
    public class Decorations : TestGenericBuilder
    {
        [Test]
        [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
        public void RequiringIntegerPropertyToBeNonZero()
        {
            // Arrange
            // Pre-assert
            // Act
            var result = GetRandom<Poco>();
            // Assert
            Expect(result.Wheels)
                .Not.To.Equal(0);
            Expect(result.NonZeroNumber)
                .Not.To.Equal(0);
        }

        [Test]
        [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
        public void RequireNonZeroId_ShouldSetIdField()
        {
            // Arrange
            // Pre-assert
            // Act
            var result = GetRandom<Poco>();
            // Assert
            Expect(result.Id)
                .Not.To.Equal(0);
        }

        [Test]
        public void RequireUniqueId_ShouldSetUniqueId()
        {
            // Arrange
            var generated = new List<int>();
            RunCycles(
                () =>
                {
                    // Pre-assert
                    // Act
                    var result = GetRandom<Poco2>();
                    // Assert
                    generated.Add(result.Id);
                }
            );
            Expect(generated).To.Have.Unique.Items();
        }

        [Test]
        public void RequireUniqueId_ShouldNeverSetZero()
        {
            // Arrange
            var generated = new List<int>();
            RunCycles(
                () =>
                {
                    // Pre-assert
                    // Act
                    var result = GetRandom<Poco2>();
                    // Assert
                    generated.Add(result.Id);
                }
            );
            Expect(generated)
                .Not.To.Contain(0);
        }

        [Test]
        [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
        public void Randomizer_ShouldUseProvidedRandomizerForNamedProperty()
        {
            // Arrange
            // Act
            var result = GetRandom<Poco>();
            // Assert
            Expect(result.NegativeNumber)
                .To.Be.Less.Than(0);
        }

        [Test]
        [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
        public void NoRandomize_ShouldIgnoreSettingProperty()
        {
            // Arrange
            // Act
            var result = GetRandom<Poco>();
            // Assert
            Expect(result.IgnoreMe)
                .To.Be.Null();
            Expect(result.IgnoreMeToo)
                .To.Be.Null();
        }

        [TestFixture]
        public class InheritingDecorators
        {
            [TestFixture]
            public class DerivedBuilderHasNoAttributes
            {
                [Test]
                public void ShouldInheritAll()
                {
                    // Arrange
                    // Act
                    var result = UndecoratedBuilder.Create()
                        .WithRandomProps()
                        .Build();
                    // Assert
                    Expect(result.Id)
                        .To.Equal(1);
                    Expect(result.Name)
                        .To.Equal("base");
                }
            }

            [TestFixture]
            public class DerivedBuilderOverridesAnAttribute
            {
                [Test]
                public void ShouldApplyDerivedAttributes()
                {
                    // Arrange
                    // Act
                    var result1 = DecoratedBuilder.Create()
                        .WithRandomProps()
                        .Build();
                    var result2 = DecoratedBuilder2.Create()
                        .WithRandomProps()
                        .Build();
                    // Assert
                    Expect(result1.Id)
                        .To.Equal(2);
                    Expect(result1.Name)
                        .To.Equal("decorated");
                    Expect(result2.Id)
                        .To.Equal(2);
                    Expect(result2.Name)
                        .To.Equal("decorated2");
                }
            }

            [TestFixture]
            public class UsingInheritanceToCreateBuildersForConcreteAndInterfaces
            {
                [Test]
                public void ShouldBeAbleToShareBuilderLogicFromBaseBuilder()
                {
                    // Arrange
                    // Act
                    var item1 = GetRandom<ItemWithInterface>();
                    var item2 = GetRandom<IItemWithInterface>();
                    // Assert
                    Expect(item1.Id)
                        .To.Equal(24);
                    Expect(item2.Id)
                        .To.Equal(42);
                }

                public interface IItemWithInterface
                {
                    int Id { get; set; }
                    string Name { get; set; }
                }

                public class ItemWithInterface
                    : IItemWithInterface
                {
                    public int Id { get; set; }
                    public string Name { get; set; }
                }

                [SetId(42)]
                public abstract class BuilderBase<T> : GenericBuilder<BuilderBase<T>, T>
                    where T : IItemWithInterface
                {
                }

                [SetId(24)]
                public class ItemBuilder : BuilderBase<ItemWithInterface>
                {
                }

                public class ItemInterfaceBuilder : BuilderBase<IItemWithInterface>
                {
                }
            }

            [SetId(1)]
            [SetName("base")]
            public class BaseBuilder : GenericBuilder<BaseBuilder, Item>
            {
            }

            public class UndecoratedBuilder : BaseBuilder
            {
                public new static UndecoratedBuilder Create()
                {
                    return new UndecoratedBuilder();
                }
            }

            [SetId(2)]
            [SetName("decorated")]
            public class DecoratedBuilder : BaseBuilder
            {
                public new static DecoratedBuilder Create()
                {
                    return new DecoratedBuilder();
                }
            }

            [SetName("decorated2")]
            public class DecoratedBuilder2 : DecoratedBuilder
            {
                public new static DecoratedBuilder2 Create()
                {
                    return new DecoratedBuilder2();
                }
            }

            public class Item
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            public class SetIdAttribute : RandomizerAttribute
            {
                private readonly int _value;

                public SetIdAttribute(
                    int value
                ) : base("Id")
                {
                    _value = value;
                }

                public override void SetRandomValue(
                    PropertyOrField propInfo,
                    ref object target
                )
                {
                    propInfo.SetValue(
                        target,
                        _value
                    );
                }
            }

            public class SetNameAttribute : RandomizerAttribute
            {
                private readonly string _value;

                public SetNameAttribute(
                    string value
                ) : base("Name")
                {
                    _value = value;
                }

                public override void SetRandomValue(
                    PropertyOrField propInfo,
                    ref object target
                )
                {
                    propInfo.SetValue(
                        target,
                        _value
                    );
                }
            }
        }

        private void RunCycles(
            Action toRun
        )
        {
            for (var i = 0;
                 i < NORMAL_RANDOM_TEST_CYCLES;
                 i++)
                toRun();
        }

        [RequireUniqueId]
        public class Poco2
        {
            public int Id { get; set; }
        }

        [RequireUniqueId]
        public class Poco2Builder : GenericBuilder<Poco2Builder, Poco2>
        {
        }

        public class Poco
        {
            public int Id { get; set; }
            public int Wheels { get; set; }
            public string Name { get; set; }
            public int NegativeNumber { get; set; }
            public int NonZeroNumber { get; set; }

            public Poco IgnoreMe { get; set; }
            public Poco IgnoreMeToo { get; set; }
        }

        [RequireNonZero(
            nameof(Poco.Wheels),
            nameof(Poco.NonZeroNumber)
        )]
        [RequireNonZeroId]
        [RandomizeNegative(nameof(Poco.NegativeNumber))]
        [NoRandomize(
            nameof(Poco.IgnoreMe),
            nameof(Poco.IgnoreMeToo)
        )]
        public class PocoBuilder : GenericBuilder<PocoBuilder, Poco>
        {
        }

        public class RandomizeNegativeAttribute : RandomizerAttribute
        {
            public RandomizeNegativeAttribute(
                string propertyName
            )
                : base(propertyName)
            {
            }

            public override void SetRandomValue(
                PropertyOrField propInfo,
                ref object target
            )
            {
                var value = GetRandomInt(
                    -10,
                    -1
                );
                propInfo.SetValue(
                    target,
                    value
                );
            }
        }

        public class RandomizeHighPositiveAttribute : RandomizerAttribute
        {
            public RandomizeHighPositiveAttribute(
                string propertyName
            )
                : base(propertyName)
            {
            }

            public override void SetRandomValue(
                PropertyOrField propInfo,
                ref object target
            )
            {
                var value = GetRandomInt(
                    100,
                    1024
                );
                propInfo.SetValue(
                    target,
                    value
                );
            }
        }

        public class Poco3
        {
            public int Id { get; set; }
        }

        [RandomizeNegative("MooCakes")]
        public class Poco3Builder : NegativeIdBuilder<Poco3Builder, Poco3>
        {
        }

        [RandomizeNegative("Id")]
        public class NegativeIdBuilder<TBuilder, TEntity>
            : GenericBuilder<TBuilder, TEntity>
            where TBuilder : GenericBuilder<TBuilder, TEntity>
        {
        }

        [Test]
        public void ShouldSearchEntireInheritanceForAttributes()
        {
            // Arrange
            // Act
            var result = GetRandom<Poco3>();
            // Assert
            Expect(result.Id)
                .To.Be.Less.Than(0);
        }

        public class Poco4
        {
            public int Id { get; set; }
        }

        [RandomizeHighPositive("Id")]
        public class Poco4Builder : NegativeIdBuilder<Poco4Builder, Poco4>
        {
        }

        [Test]
        public void ShouldRunRandomizersInOrderFromLowestAncestorToHighestAncestor()
        {
            // Arrange
            // Act
            var result = GetRandom<Poco4>();
            // Assert
            Expect(result.Id).To.Be
                .Greater.Than.Or.Equal.To(100)
                .And
                .Less.Than.Or.Equal.To(1024);
        }
    }

    [TestFixture]
    public class WhenImplicitlyUsedForRandomObjectGeneration
    {
        public class HasDelegateProp
        {
            public delegate void SomeDelegate();

            public SomeDelegate SomeDelegateHandler { get; set; }
        }

        [Test]
        public void ShouldNotBreakOnDelegateProperty()
        {
            // Arrange
            // Act
            Expect(GetRandom<HasDelegateProp>)
                .Not.To.Throw();
            // Assert
        }

        [Test]
        public void ShouldNotBreakOnDelegatePropertyInvocation()
        {
            // Arrange
            // Act
            var result = GetRandom<HasDelegateProp>();
            // Assert
            Expect(() => result.SomeDelegateHandler())
                .Not.To.Throw();
        }

        public class HasActionProp
        {
            public Action SomeAction { get; set; }
        }

        [Test]
        public void ShouldNotBreakOnActionProperty()
        {
            // Arrange
            // Act
            Expect(GetRandom<HasActionProp>)
                .Not.To.Throw();
            // Assert
        }

        [Test]
        public void ShouldNotBreakOnActionPropertyInvocation()
        {
            // Arrange
            // Act
            var result = GetRandom<HasActionProp>();
            // Assert
            Expect(() => result.SomeAction())
                .Not.To.Throw();
        }

        public class HasActionProp2
        {
            public Action<int> MethodThatRequireInt { get; set; }
        }

        [Test]
        public void ShouldNotBreakOnActionOfIntPropertyInvokation()
        {
            // Arrange
            var generated = GetRandom<HasActionProp2>();
            // Act
            // Assert
            generated.MethodThatRequireInt(312);
        }

        public class HasFuncProp<T>
        {
            public Func<T> SomeFunc { get; set; }
        }

        [Test]
        public void ShouldNotThrowOnFuncProperty()
        {
            // Arrange
            // Act
            Expect(GetRandom<HasFuncProp<int>>)
                .Not.To.Throw();
            // Assert
        }

        [Test]
        public void ShouldNotThrowOnFuncPropertyInvocation()
        {
            // Arrange
            // Act
            var result = GetRandom<HasFuncProp<int>>();
            // Assert
            Expect(result.SomeFunc)
                .Not.To.Throw();
        }

        public class HasFuncProp2<TIn, TOut>
        {
            public Func<TIn, TOut> SomeFunc { get; set; }
        }

        [Test]
        public void ShouldReturnDefaultOutputValueFromGeneratedFuncProperty()
        {
            // Arrange
            var generated = GetRandom<HasFuncProp2<string, int>>();
            // Act
            var result = generated.SomeFunc(GetRandomString());
            // Assert
            Expect(result)
                .To.Equal(0);
        }

        [Test]
        public void ShouldThrowOnGenericDefinitionType()
        {
            // Arrange
            // Act
            Action generation = () => GetRandom(typeof(Action<>));
            // Assert
            Expect(generation)
                .To.Throw().With.Message.Containing("A generic type definition can't be generated: ");
        }
    }

    [TestFixture]
    public class RequireUniqueForEveryone
    {
        [Test]
        public void ShouldHonorAttribute()
        {
            // Arrange
            var collection = new List<Poco>();
            // Act
            for (var i = 0; i < 100; i++)
            {
                collection.Add(GetRandom<Poco>());
            }
            // Assert
            var result = collection.Select(o => o.Id).ToArray();
            Expect(result)
                .To.Be.Distinct();
        }

        [RequireUnique<int>("Id")]
        public class PocoBuilder : GenericBuilder<PocoBuilder, Poco>
        {
        }

        public class Poco
        {
            public int Id { get; set; }
        }
    }
}