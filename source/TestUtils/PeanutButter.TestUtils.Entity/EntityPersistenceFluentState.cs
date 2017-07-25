using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.FluentMigrator;
using PeanutButter.RandomGenerators;
using PeanutButter.TempDb;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using PeanutButter.Utils.Entity;

namespace PeanutButter.TestUtils.Entity
{
    public class EntityPersistenceFluentStateBase
    {
        protected static List<ITempDB> SharedDatabasesWhichHaveBeenMigrated { get; } = new List<ITempDB>();
        protected static readonly object SharedMigrationsLock = new object();
    }

    public class EntityPersistenceFluentState<TContext, TEntity> : EntityPersistenceFluentStateBase
        where TContext : DbContext
        where TEntity : class
    {
        private readonly Func<DbConnection, TContext> _contextFactory;
        private Func<TContext, IDbSet<TEntity>> _collectionNabberFromContext;
        private ITempDB _tempDb;
        private readonly List<Action<TContext, TEntity>> _runBeforePersisting = new List<Action<TContext, TEntity>>();
        private readonly List<Action<TEntity, TEntity>> _runAfterPersisting = new List<Action<TEntity, TEntity>>();
        private const string CREATED = "Created";
        private const string LAST_MODIFIED = "LastModified";
        private const string ENABLED = "Enabled";
        private readonly string[] _ignoreEntityBaseFields = {CREATED, LAST_MODIFIED, ENABLED};
        private string[] _ignoredProperties;
        private Func<TEntity> _entityFactory;
        private Action<string> _contextLogAction;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly IEnumerable<PropertyInfo> DecimalProps;
        private static readonly IEnumerable<PropertyInfo> DateTimeProps;
        private Func<string, IDBMigrationsRunner> _migrationsRunnerFactory;
        private Func<ITempDB> _tempDbFactoryFunction;
        private ITempDB _sharedDatabase;
        private Action<string> _logAction;
        private bool _suppressMigrationsWarning = false;
        private TimeSpan _allowedDateTimeDelta;

        // ReSharper disable once StaticMemberInGenericType

        public EntityPersistenceFluentState(Func<TEntity> entityFactory, Func<DbConnection, TContext> contextFactory = null)
        {
            _entityFactory = entityFactory;
            _contextFactory = contextFactory ?? CreateContext;
            _entityFactory = BuildWithBuilder;
            _logAction = Console.WriteLine;
            _allowedDateTimeDelta = EntityPersistenceFluentStateConstants.FiftyMilliseconds;
        }

        static EntityPersistenceFluentState()
        {
            var allProperties = typeof(TEntity)
                .GetProperties();
            DecimalProps = allProperties
                .Where(pi => pi.PropertyType == typeof(decimal) || pi.PropertyType == typeof(decimal?));
            DateTimeProps = allProperties
                .Where(pi => pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(DateTime?));
        }


        public EntityPersistenceFluentState<TContext, TEntity> WithDbMigrator(Func<string, IDBMigrationsRunner> migrationsRunnerFactory)
        {
            _migrationsRunnerFactory = migrationsRunnerFactory;
            return this;
        }

        public EntityPersistenceFluentState<TContext, TEntity> WithCollection(Func<TContext, IDbSet<TEntity>> nabber)
        {
            _collectionNabberFromContext = nabber;
            return this;
        }

        public EntityPersistenceFluentState<TContext, TEntity> BeforePersisting(Action<TContext, TEntity> toRun)
        {
            _runBeforePersisting.Add(toRun);
            return this;
        }

        public EntityPersistenceFluentState<TContext, TEntity> AfterPersisting(Action<TEntity, TEntity> runAfterPersisting)
        {
            _runAfterPersisting.Add(runAfterPersisting);
            return this;
        }

        public EntityPersistenceFluentState<TContext, TEntity> WithIgnoredProperties(params string[] propertyNames)
        {
            _ignoredProperties = propertyNames;
            return this;
        }

        public EntityPersistenceFluentState<TContext, TEntity> WithEntityFactory(Func<TEntity> factoryFunc)
        {
            _entityFactory = factoryFunc ?? BuildWithBuilder;
            return this;
        }

        public EntityPersistenceFluentState<TContext, TEntity> WithEntityFrameworkLogger(Action<string> logAction)
        {
            _contextLogAction = logAction;
            return this;
        }

        public EntityPersistenceFluentState<TContext, TEntity> WithLogAction(Action<string> logAction)
        {
            _logAction = logAction;
            return this;
        }

        public EntityPersistenceFluentState<TContext, TEntity> WithAllowedDateTimePropertyDelta(TimeSpan timeSpan)
        {
            _allowedDateTimeDelta = timeSpan;
            return this;
        }

        private IDbSet<TEntity> GetCollection(TContext context)
        {
            if (_collectionNabberFromContext != null)
                return _collectionNabberFromContext(context);
            return context.Set<TEntity>();
        }

        private TContext CreateContext(DbConnection dbConnection)
        {
            var context = (TContext) Activator.CreateInstance(typeof(TContext), dbConnection);
            if (_contextLogAction != null)
                context.Database.Log = _contextLogAction;
            return context;
        }

        private TEntity BuildWithBuilder()
        {
            var entityType = typeof(TEntity);
            var builderType = GenericBuilderLocator.TryFindExistingBuilderFor(entityType)
                              ?? GenericBuilderLocator.FindOrGenerateDynamicBuilderFor(entityType);
            Assert.IsNotNull(builderType, $"Can't find or create a builder for {entityType.Name}");
            var builder = Activator.CreateInstance(builderType) as IGenericBuilder;
            Assert.IsNotNull(builder, $"Located builder {builderType.Name} does not implement IGenericBuilder");
            var entity = builder.GenericWithRandomProps().GenericBuild() as TEntity;
            Assert.IsNotNull(entity, $"located builder {builderType.Name} for {entityType.Name} builds NULL or invalid entity");
            return entity;
        }

        public EntityPersistenceFluentState<TContext, TEntity> WithBuilder<TEntityBuilder>()
            where TEntityBuilder : GenericBuilder<TEntityBuilder, TEntity>
        {
            _entityFactory = BuildWithBuilder;
            return this;
        }

        public void ShouldPersistAndRecall()
        {
            //---------------Set up test pack-------------------
            var sut = _entityFactory();
            var toIgnore = new List<string>(_ignoredProperties
                .EmptyIfNull()
                .Union(typeof(TEntity).VirtualProperties()));
            AttemptToPersistWith(sut, toIgnore);
            ValidatePersistenceWith(toIgnore, sut);
            if (_tempDb == _sharedDatabase)
                return;
            _tempDb.Dispose();
        }

        private void ValidatePersistenceWith(List<string> toIgnore, TEntity sut)
        {
            using (var ctx = GetContext())
            {
                var persisted = GetPersistedEntityFrom(ctx);
                Assert.IsNotNull(persisted, "No entity of type '" + typeof(TEntity).FullName + "' found in context after saving!");
                var entityType = typeof(TEntity);
                var idProp = entityType.GetProperties().FirstOrDefault(pi => pi.Name.ToLower() == entityType.Name.ToLower() + "id");
                if (idProp != null && !toIgnore.EmptyIfNull().Contains(idProp.Name))
                    Assert.AreNotEqual(0, idProp.GetValue(persisted));


                var ignoreAndCrankyProperties = toIgnore.Union(
                    DecimalProps.Union(DateTimeProps).Select(pi => pi.Name)
                ).ToArray();
                PropertyAssert.AreDeepEqual(persisted, sut, ignoreAndCrankyProperties);
                TestDecimalPropertiesOn(sut, persisted);
                TestDateTimePropertiesOn(sut, persisted);

                _runAfterPersisting.ForEach(a => a.Invoke(sut, persisted));
            }
        }

        private void TestDateTimePropertiesOn(TEntity sut, TEntity persisted)
        {
            var allowed = Math.Abs(_allowedDateTimeDelta.TotalMilliseconds);
            DateTimeProps.ForEach(pi =>
            {
                var beforeValue = (DateTime?) pi.GetValue(sut);
                var afterValue = (DateTime?) pi.GetValue(persisted);
                if (beforeValue.HasValue && afterValue.HasValue)
                {
                    var delta = Math.Abs((beforeValue.Value - afterValue.Value).TotalMilliseconds);
                    Assert.That(delta, Is.LessThanOrEqualTo(allowed),
                        $"Property mismatch: expected {pi.Name} to persist and recall with an accuracy within {allowed} ms");
                    return;
                }
                if (beforeValue.HasValue != afterValue.HasValue)
                {
                    Assert.Fail($"Property mismatch for {pi.Name}: expected {beforeValue} but got {afterValue}");
                }
            });
        }

        private static void TestDecimalPropertiesOn(TEntity sut, TEntity persisted)
        {
            foreach (var pi in DecimalProps)
            {
                var beforeValue = (decimal?) pi.GetValue(sut);
                var afterValue = (decimal?) pi.GetValue(persisted);
                if (beforeValue.HasValue && afterValue.HasValue)
                {
                    afterValue.Value.ShouldMatch(beforeValue.Value);
                    continue;
                }
                if (beforeValue.HasValue != afterValue.HasValue)
                    Assert.Fail($"Property mismatch for {pi.Name}: expected {beforeValue} but got {afterValue}");
            }
        }

        private TEntity GetPersistedEntityFrom(TContext ctx)
        {
            return GetCollection(ctx).FirstOrDefault();
        }

        private void AttemptToPersistWith(TEntity sut, List<string> toIgnore)
        {
            using (var ctx = GetContext())
            {
                if (ctx is DbContextWithAutomaticTrackingFields)
                {
                    var entity = sut as EntityBase;
                    if (entity != null)
                    {
                        entity.Created = default(DateTime);
                        toIgnore.AddRange(_ignoreEntityBaseFields);
                    }
                }
                var beforeTest = DateTime.Now;
                //---------------Assert Precondition----------------
                _runBeforePersisting.ForEach(a => a.Invoke(ctx, sut));
                Assert.IsFalse(SomeEntitiesAlreadyExistIn(ctx), "Some entities already exist. Please clear out your context before running this test");
                //---------------Execute Test ----------------------
                GetCollection(ctx).Add(sut);
                ctx.SaveChangesWithErrorReporting();
                var afterTest = DateTime.Now;
                //---------------Test Result -----------------------
                if (ctx is DbContextWithAutomaticTrackingFields)
                {
                    var entity = sut as EntityBase;
                    if (entity != null)
                    {
                        Assert.That((object) entity.Created, Is.GreaterThanOrEqualTo(beforeTest));
                        Assert.That((object) entity.Created, Is.LessThanOrEqualTo(afterTest));
                        Assert.IsNull(entity.LastModified);
                        Assert.IsTrue(entity.Enabled);

                        // modify to test LastModified
                        beforeTest = DateTime.Now;
                        entity.LastModified = DateTime.MinValue;
                        ctx.SaveChangesWithErrorReporting();
                        afterTest = DateTime.Now;
                        Assert.That((object) entity.LastModified, Is.GreaterThanOrEqualTo(beforeTest));
                        Assert.That((object) entity.LastModified, Is.LessThanOrEqualTo(afterTest));
                    }
                }
            }
        }

        private bool SomeEntitiesAlreadyExistIn(TContext ctx)
        {
            return GetCollection(ctx).Any();
        }

        private TContext GetContext()
        {
            return _contextFactory(GetConnection());
        }

        private DbConnection GetConnection()
        {
            var tempDb = GetTempDb();
            return tempDb.CreateConnection();
        }

        private ITempDB GetTempDb()
        {
            return _tempDb ?? (_tempDb = GetSharedDatabase() ?? CreateTempDb());
        }


        private ITempDB GetSharedDatabase()
        {
            if (_sharedDatabase == null)
                return null;
            lock (SharedMigrationsLock)
            {
                if (SharedDatabasesWhichHaveBeenMigrated.Contains(_sharedDatabase))
                    return _sharedDatabase;
                SharedDatabasesWhichHaveBeenMigrated.Add(_sharedDatabase);
                return MigrateUpOn(_sharedDatabase);
            }
        }


        private ITempDB CreateTempDb()
        {
            var db = _tempDbFactoryFunction?.Invoke() ?? new TempDBLocalDb();
            return MigrateUpOn(db);
        }

        private ITempDB MigrateUpOn(ITempDB db)
        {
            if (_migrationsRunnerFactory == null)
            {
                if (!_suppressMigrationsWarning)
                    _logAction(
                        @"WARNING: running tests without specified DBMigrationsRunner. 

EntityFramework will perform migrations, which probably won't test what you want to. 

To suppress this message, include .SuppressMissingMigratorMessage() in your fluent call chain.");
                return db;
            }
            var runner = _migrationsRunnerFactory(db.ConnectionString);
            runner.MigrateToLatest();
            return db;
        }

        public EntityPersistenceFluentState<TContext, TEntity> WithTempDbFactory(Func<ITempDB> factoryFunction)
        {
            _tempDbFactoryFunction = factoryFunction;
            return this;
        }

        public EntityPersistenceFluentState<TContext, TEntity> WithSharedDatabase(ITempDB tempDb)
        {
            _sharedDatabase = tempDb;
            return this;
        }

        public EntityPersistenceFluentState<TContext, TEntity> SuppressMissingMigratorMessage()
        {
            _suppressMigrationsWarning = true;
            return this;
        }
    }
}