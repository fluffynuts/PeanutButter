using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.TempDb;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using PeanutButter.Utils.Entity;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestEntityPersistenceTester
    {
        [Test]
        public void CreateFor_ShouldReturnEntityPersistenceTester()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = EntityPersistenceTester.CreateFor<COMBlockListReason>();

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<EntityPersistenceTesterFor<COMBlockListReason>>(result);
        }

        [Test]
        public void Tester_WithAllPriorProvidedStuffs_ShouldWorkTheSame()
        {
            //---------------Set up test pack-------------------
            var beforePersistingCalled = false;
            var afterPersistingCalled = false;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            EntityPersistenceTester.CreateFor<COMBlockListReason>()
                .WithContext<CommunicatorContext>()
                .WithCollection(ctx => ctx.BlockListReasons)
                .WithBuilder<COMBlockListReasonBuilder>()
                .WithIgnoredProperties("COMBlockListReasonID")  // not actually required
                .WithDbMigrator(connectionString => new DbSchemaImporter(connectionString, TestResources.dbscript))
                .BeforePersisting((ctx, entity) =>
                {
                    beforePersistingCalled = true;
                    Assert.IsNotNull(ctx);
                    Assert.IsInstanceOf<CommunicatorContext>(ctx);
                    Assert.IsNotNull(entity);
                    Assert.IsInstanceOf<COMBlockListReason>(entity);
                })
                .AfterPersisting((before, after) =>
                {
                    afterPersistingCalled = true;
                    Assert.IsNotNull(before);
                    Assert.IsNotNull(after);
                    Assert.AreNotEqual(before, after);
                    before.GetType().ShouldBeAssignableFrom<COMBlockListReason>();
                })
                .ShouldPersistAndRecall();

            //---------------Test Result -----------------------
            Assert.IsTrue(beforePersistingCalled);
            Assert.IsTrue(afterPersistingCalled);
        }

        [Test]
        public void Tester_WithoutBeforePersisting_ShouldWorkTheSame()
        {
            //---------------Set up test pack-------------------
            var afterPersistingCalled = false;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            EntityPersistenceTester.CreateFor<COMBlockListReason>()
                .WithContext<CommunicatorContext>()
                .WithBuilder<COMBlockListReasonBuilder>()
                .WithDbMigrator(connectionString => new DbSchemaImporter(connectionString, TestResources.dbscript))
                .AfterPersisting((before, after) =>
                {
                    afterPersistingCalled = true;
                    Assert.IsNotNull(before);
                    Assert.IsNotNull(after);
                    Assert.AreNotEqual(before, after);
                    before.GetType().ShouldBeAssignableFrom<COMBlockListReason>();
                })
                .ShouldPersistAndRecall();

            //---------------Test Result -----------------------
            Assert.IsTrue(afterPersistingCalled);
        }

        [Test]
        public void Tester_WithoutAfterPersisting_ShouldWorkTheSame()
        {
            //---------------Set up test pack-------------------
            var beforePersistingCalled = false;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            EntityPersistenceTester.CreateFor<COMBlockListReason>()
                .WithContext<CommunicatorContext>()
                .WithCollection(ctx => ctx.BlockListReasons)
                .WithBuilder<COMBlockListReasonBuilder>()
                .WithDbMigrator(connectionString => new DbSchemaImporter(connectionString, TestResources.dbscript))
                .BeforePersisting((ctx, entity) =>
                {
                    beforePersistingCalled = true;
                    Assert.IsNotNull(ctx);
                    Assert.IsInstanceOf<CommunicatorContext>(ctx);
                    Assert.IsNotNull(entity);
                    Assert.IsInstanceOf<COMBlockListReason>(entity);
                })
                .ShouldPersistAndRecall();

            //---------------Test Result -----------------------
            Assert.IsTrue(beforePersistingCalled);
        }

        [Test]
        public void Tester_WithoutAnyUserSetupOrValidation_ShouldWorkTheSame()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            EntityPersistenceTester.CreateFor<COMBlockListReason>()
                .WithContext<CommunicatorContext>()
                .WithCollection(ctx => ctx.BlockListReasons)
                .WithBuilder<COMBlockListReasonBuilder>()
                .WithDbMigrator(connectionString => new DbSchemaImporter(connectionString, TestResources.dbscript))
                .ShouldPersistAndRecall();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Tester_ShouldBeAbleToWorkWithoutACollectionNabber()
        {
            //---------------Set up test pack-------------------
            var beforePersistingCalled = false;
            var afterPersistingCalled = false;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            EntityPersistenceTester.CreateFor<COMBlockListReason>()
                .WithContext<CommunicatorContext>()
                .WithBuilder<COMBlockListReasonBuilder>()
                .WithDbMigrator(connectionString => new DbSchemaImporter(connectionString, TestResources.dbscript))
                .BeforePersisting((ctx, entity) =>
                {
                    beforePersistingCalled = true;
                    Assert.IsNotNull(ctx);
                    Assert.IsInstanceOf<CommunicatorContext>(ctx);
                    Assert.IsNotNull(entity);
                    Assert.IsInstanceOf<COMBlockListReason>(entity);
                })
                .AfterPersisting((before, after) =>
                {
                    afterPersistingCalled = true;
                    Assert.IsNotNull(before);
                    Assert.IsNotNull(after);
                    Assert.AreNotEqual(before, after);
                    before.GetType().ShouldBeAssignableFrom<COMBlockListReason>();
                })
                .ShouldPersistAndRecall();

            //---------------Test Result -----------------------
            Assert.IsTrue(beforePersistingCalled);
            Assert.IsTrue(afterPersistingCalled);
        }

        [Test]
        public void Tester_ShouldBeAbleToWorkWithoutSpecifyingABuilder()
        {
            //---------------Set up test pack-------------------
            var beforePersistingCalled = false;
            var afterPersistingCalled = false;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            EntityPersistenceTester.CreateFor<COMBlockListReason>()
                .WithContext<CommunicatorContext>()
                .WithDbMigrator(connectionString => new DbSchemaImporter(connectionString, TestResources.dbscript))
                .BeforePersisting((ctx, entity) =>
                {
                    beforePersistingCalled = true;
                    Assert.IsNotNull(ctx);
                    Assert.IsInstanceOf<CommunicatorContext>(ctx);
                    Assert.IsNotNull(entity);
                    Assert.IsInstanceOf<COMBlockListReason>(entity);
                })
                .AfterPersisting((before, after) =>
                {
                    afterPersistingCalled = true;
                    Assert.IsNotNull(before);
                    Assert.IsNotNull(after);
                    Assert.AreNotEqual(before, after);
                    before.GetType().ShouldBeAssignableFrom<COMBlockListReason>();
                })
                .ShouldPersistAndRecall();

            //---------------Test Result -----------------------
            Assert.IsTrue(beforePersistingCalled);
            Assert.IsTrue(afterPersistingCalled);
        }

        [Test]
        public void Tester_ShouldBeAbleToBeGivenAFuncToCreateTheTempDb()
        {
            //---------------Set up test pack-------------------
            var beforePersistingCalled = false;
            var afterPersistingCalled = false;
            var tempDb = new TempDbWithCallInformation();
            using (new AutoResetter(() => { }, () => tempDb.ActualDispose()))
            {
                Assert.AreEqual(0, tempDb.DisposeCalls);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                EntityPersistenceTester.CreateFor<COMBlockListReason>()
                    .WithContext<CommunicatorContext>()
                    .WithTempDbFactory(() => tempDb)
                    .WithDbMigrator(connectionString => new DbSchemaImporter(connectionString, TestResources.dbscript))
                    .BeforePersisting((ctx, entity) =>
                    {
                        beforePersistingCalled = true;
                        Assert.IsNotNull(ctx);
                        Assert.IsInstanceOf<CommunicatorContext>(ctx);
                        Assert.IsNotNull(entity);
                        Assert.IsInstanceOf<COMBlockListReason>(entity);
                    })
                    .AfterPersisting((before, after) =>
                    {
                        afterPersistingCalled = true;
                        Assert.IsNotNull(before);
                        Assert.IsNotNull(after);
                        Assert.AreNotEqual(before, after);
                        before.GetType().ShouldBeAssignableFrom<COMBlockListReason>();
                    })
                    .ShouldPersistAndRecall();

                //---------------Test Result -----------------------
                Assert.IsTrue(beforePersistingCalled);
                Assert.IsTrue(afterPersistingCalled);
                Assert.AreEqual(1, tempDb.DisposeCalls);

                // test that the provided tempdb was actually used;
                AssertHasTable(tempDb, "COMBlockListReason");
                AssertTableIsNotEmpty(tempDb, "COMBlockListReason");
            }
        }

        [Test]
        public void Tester_ShouldBeAbleToShareATempDb()
        {
            //---------------Set up test pack-------------------
            var tempDb = new TempDbWithCallInformation();
            using (new AutoResetter(() => { }, () => tempDb.Dispose()))
            {

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                EntityPersistenceTester.CreateFor<COMBlockListReason>()
                    .WithContext<CommunicatorContext>()
                    .WithSharedDatabase(tempDb)
                    .WithDbMigrator(connectionString => new DbSchemaImporter(connectionString, TestResources.dbscript))
                    .ShouldPersistAndRecall();
                Assert.AreEqual(0, tempDb.DisposeCalls);
                using (var ctx = new CommunicatorContext(tempDb.CreateConnection()))
                {
                    ctx.BlockListReasons.Clear(); // required to allow the persistence test to complete
                    ctx.SaveChangesWithErrorReporting();
                }
                EntityPersistenceTester.CreateFor<COMBlockListReason>()
                    .WithContext<CommunicatorContext>()
                    .WithSharedDatabase(tempDb)
                    .WithDbMigrator(connectionString => new DbSchemaImporter(connectionString, TestResources.dbscript))
                    .ShouldPersistAndRecall();

                //---------------Test Result -----------------------
                Assert.AreEqual(0, tempDb.DisposeCalls);
            }
        }

        [Test]
        public void SuppressMissingMigratorMessage_ShouldSuppressMissingMigratorMessage()
        {
            //---------------Set up test pack-------------------
            string logMessage = null;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            EntityPersistenceTester.CreateFor<COMBlockListReason>()
                .WithContext<CommunicatorContext>()
                .WithLogAction(s => logMessage = s)
                .SuppressMissingMigratorMessage()
                .ShouldPersistAndRecall();

            //---------------Test Result -----------------------
            Assert.IsNull(logMessage);
        }
        [Test]
        public void WhenNoMigratorAndNoSuppressMissingMigratorMessage_ShouldWarnAboutMigrator()
        {
            //---------------Set up test pack-------------------
            string logMessage = null;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            EntityPersistenceTester.CreateFor<COMBlockListReason>()
                .WithContext<CommunicatorContext>()
                .WithLogAction(s => logMessage = s)
                .ShouldPersistAndRecall();

            //---------------Test Result -----------------------
            Assert.IsNotNull(logMessage);
            StringAssert.Contains("warning", logMessage.ToLower());
            StringAssert.Contains("entityframework will perform migrations", logMessage.ToLower());
            StringAssert.Contains("to suppress this message", logMessage.ToLower());
        }

        [Test]
        public void ShortestPossibleUsefulUsage_CoversSimplestCases()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            EntityPersistenceTester.CreateFor<COMBlockList>()
                .WithContext<CommunicatorContext>()
                .WithDbMigrator(s => new DbSchemaImporter(s, TestResources.dbscript))
                .ShouldPersistAndRecall();

            //---------------Test Result -----------------------
        }

        [Test]
        public void WithEntityFrameworkLogger_ShouldLogUsingProvidedAction()
        {
            //---------------Set up test pack-------------------
            var logLines = new List<string>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            EntityPersistenceTester.CreateFor<COMBlockList>()
                .WithContext<CommunicatorContext>()
                .WithEntityFrameworkLogger(logLines.Add)
                .WithDbMigrator(s => new DbSchemaImporter(s, TestResources.dbscript))
                .ShouldPersistAndRecall();

            //---------------Test Result -----------------------
            CollectionAssert.IsNotEmpty(logLines);
            var total = string.Join("\n", logLines).ToLower();
            StringAssert.Contains("insert", total);
            StringAssert.Contains("select", total);
            StringAssert.Contains("comblocklist", total);
        }

        [Test]
        public void ActingOnEntitiesWithTracking()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            EntityPersistenceTester.CreateFor<SomeEntityWithDecimalValue>()
                .WithContext<EntityPersistenceContext>()
                .WithDbMigrator(s => new DbSchemaImporter(s, TestResources.entitypersistence))
                .ShouldPersistAndRecall();

            //---------------Test Result -----------------------
        }


        [Test]
        public void UsingDefaultDeltaOf2Ms_ShouldAllowTestItemDateTimePropertyValuesToDifferByThatDelta()
        {
            // Pre-amble: I've noticed that, periodically, a test against an entity
            //  with a DateTime field will fail: the persisted entity will come back 
            //  with the field different by one (rarely two) milliseconds. Mostly, 
            //  in production, timestamps are only used to seconds precision, so mostly,
            //  we don't actually care. The point of this test is to fail on the few that do
            //  when we're not catering for the delta. The plan is:
            //  1) add a WithAllowedDateTimeDelta method which takes a timespan
            //          - so the consumer can specify exactly how fine to make the
            //              allowance
            //  2) default to using an allowed delta which covers the observed test flops (so
            //      probably 2 ms)
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var exceptions = new List<Exception>();
            Parallel.For(0, 5, (i, state) =>
            {
                // at least one of these should hit the issue
                try
                {
                    EntityPersistenceTester.CreateFor<SomeEntityWithDateTimeValue>()
                        .WithContext<ContextForDateTimeDeltaTesting>()
                        .SuppressMissingMigratorMessage()
                        .ShouldPersistAndRecall();
                }
                catch (Exception ex)
                {
                    state.Stop();
                    exceptions.Add(ex);
                }
            });

            //---------------Test Result -----------------------
            if (exceptions.Any())
                exceptions.ForEach(e => Console.WriteLine(e.Message));
            CollectionAssert.IsEmpty(exceptions);
        }

        [Test]
        public void WithAllowedDateTimeDelta_GivenDelta_WhenDeltaIsExceeded_ShouldThrow()
        {
            // Pre-amble: I've noticed that, periodically, a test against an entity
            //  with a DateTime field will fail: the persisted entity will come back 
            //  with the field different by one (rarely two) milliseconds. Mostly, 
            //  in production, timestamps are only used to seconds precision, so mostly,
            //  we don't actually care. The point of this test is to fail on the few that do
            //  when we're not catering for the delta. The plan is:
            //  1) add a WithAllowedDateTimeDelta method which takes a timespan
            //          - so the consumer can specify exactly how fine to make the
            //              allowance
            //  2) default to using an allowed delta which covers the observed test flops (so
            //      probably 2 ms)
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var exceptions = new List<Exception>();
            Parallel.For(0, 15, (i, state) =>
            {
                // at least one of these should hit the issue
                try
                {
                    EntityPersistenceTester.CreateFor<SomeEntityWithDateTimeValue>()
                        .WithContext<ContextForDateTimeDeltaTesting>()
                        .WithAllowedDateTimePropertyDelta(new TimeSpan(0,0,0,0,1))  // there should be at least one 2ms delta
                        .SuppressMissingMigratorMessage()
                        .ShouldPersistAndRecall();
                }
                catch (Exception ex)
                {
                    state.Stop();
                    exceptions.Add(ex);
                }
            });

            //---------------Test Result -----------------------
            CollectionAssert.IsNotEmpty(exceptions);
        }

        public class SomeEntityWithDateTimeValue
        {
            public int SomeEntityWithDateTimeValueId { get; set; }
            public DateTime TimeStamp { get; set; }
            public DateTime? NullableTimeStamp { get; set; }
        }

        public class ContextForDateTimeDeltaTesting: DbContext
        {
            public IDbSet<SomeEntityWithDateTimeValue> StuffAndThings { get; set; }
         
            public ContextForDateTimeDeltaTesting(DbConnection connection): base(connection, false)
            {
            }
        }


        public class SomeEntityWithDecimalValue: EntityBase
        {
            public int SomeEntityWithDecimalValueId { get; set; }
            public decimal DecimalValue { get; set; }
            public decimal? NullableDecimalValue { get; set; }
        }

        public class EntityPersistenceContext: DbContextWithAutomaticTrackingFields
        {
            private IDbSet<SomeEntityWithDecimalValue> StuffAndThings { get; set; }

            public EntityPersistenceContext(string nameOrConnectionString) : base(nameOrConnectionString)
            {
            }

            public EntityPersistenceContext(DbConnection connection, bool contextOwnsConnection) : base(connection, contextOwnsConnection)
            {
            }

            public EntityPersistenceContext(DbConnection connection) : base(connection)
            {
            }
        }


        private void AssertTableIsNotEmpty(ITempDB tempDb, string tableName)
        {
            AssertCanRead(tempDb, $"select * from {tableName};");
        }

        private class TempDbWithCallInformation: TempDBLocalDb
        {
            public int DisposeCalls { get; private set; }
            public int CreateConnectionCalls { get; private set; }

            public override void Dispose()
            {
                DisposeCalls++;
            }

            public void ActualDispose()
            {
                base.Dispose();
            }

            public override DbConnection CreateConnection()
            {
                CreateConnectionCalls++;
                return base.CreateConnection();
            }
        }

        private void AssertHasTable(ITempDB tempDb, string tableName)
        {
            AssertCanRead(tempDb, $"select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{tableName}';");
        }

        private static void AssertCanRead(ITempDB tempDb, string sql)
        {
            using (var conn = tempDb.CreateConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                }
            }
        }
    }

}
