using System;
using System.Data.Common;
using System.Data.Entity;
using PeanutButter.RandomGenerators;
using PeanutButter.TempDb.LocalDb;

namespace PeanutButter.TestUtils.Entity
{
    internal class EntityPersistenceTestContext<TContext, TEntity>: EntityPersistenceTestContextBase<TEntity> where TContext: DbContext
        where TEntity : class
    {
        private Func<DbConnection, TContext> _contextFactory;
        private Func<TContext, IDbSet<TEntity>> _collectionNabberFromContext;
        private TContext _context;
        private TempDBLocalDb _tempDb;
        private Action<TContext, TEntity> _runBeforePersisting;
        private Action<TEntity, TEntity> _runAfterPersisting;
        private string[] _ignoredProperties;
        private Func<TEntity> _entityFactory;
        public TContext Context => _context;

        public EntityPersistenceTestContext(Func<DbConnection, TContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? CreateContext;
            _entityFactory = DefaultEntityFactory;
        }

        private TEntity DefaultEntityFactory()
        {
            var entityType = typeof (TEntity);
            var builderType = GenericBuilderLocator.TryFindExistingBuilderFor(entityType)
                              ?? GenericBuilderLocator.FindOrGenerateDynamicBuilderFor(entityType);
            var builder = Activator.CreateInstance(builderType) as IGenericBuilder;
            return builder.GenericWithRandomProps().GenericBuild() as TEntity;
        }

        private TContext CreateContext(DbConnection dbConnection)
        {
            return (TContext) Activator.CreateInstance(typeof (TContext), dbConnection);
        }

        internal void SetCollectionNabber(Func<TContext, IDbSet<TEntity>> nabber)
        {
            _collectionNabberFromContext = nabber;
        }

        internal void SetRunBeforePersisting(Action<TContext, TEntity> toRun)
        {
            _runBeforePersisting = toRun;
        }

        protected override IDbSet<TEntity> GetCollection()
        {
            return _collectionNabberFromContext(GetContext());
        }

        internal override void SetAfterPersisting(Action<TEntity, TEntity> runAfterPersisting)
        {
            _runAfterPersisting = runAfterPersisting;
        }

        internal override void SetIgnoredProperties(params string[] propertyNames)
        {
            _ignoredProperties = propertyNames;
        }

        internal override void PerformTest()
        {
            ////---------------Set up test pack-------------------
            //var sut = (new TBuilder()).WithRandomProps().Build();
            //using (var ctx = GetContext(LogEntitySql))
            //{
            //    if (ctx is DbContextWithAutomaticTrackingFields)
            //    {
            //        var entity = sut as EntityBase;
            //        if (entity != null)
            //            entity.Created = default(DateTime);
            //    }
            //    var beforeTest = DateTime.Now;
            //    //---------------Assert Precondition----------------
            //    if (beforePersisting != null)
            //    {
            //        beforePersisting(ctx, sut);
            //    }
            //    Assert.IsFalse(collectionNabber(ctx).Any(), "Some entities already exist. Please clear out your context before running this test");
            //    //---------------Execute Test ----------------------
            //    collectionNabber(ctx).Add(sut);
            //    DbContextExtensions.SaveChangesWithErrorReporting(ctx);
            //    var afterTest = DateTime.Now;
            //    //---------------Test Result -----------------------
            //    if (ctx is DbContextWithAutomaticTrackingFields)
            //    {
            //        var entity = sut as EntityBase;
            //        if (entity != null)
            //        {
            //            Assert.That((object) entity.Created, Is.GreaterThanOrEqualTo(beforeTest));
            //            Assert.That((object) entity.Created, Is.LessThanOrEqualTo(afterTest));
            //            Assert.IsNull(entity.LastModified);
            //            Assert.IsTrue(entity.Enabled);

            //            // modify to test LastModified
            //            beforeTest = DateTime.Now;
            //            entity.LastModified = DateTime.MinValue;
            //            ctx.SaveChangesWithErrorReporting();
            //            afterTest = DateTime.Now;
            //            Assert.That((object) entity.LastModified, Is.GreaterThanOrEqualTo(beforeTest));
            //            Assert.That((object) entity.LastModified, Is.LessThanOrEqualTo(afterTest));
            //        }
            //    }
            //}
            //using (var ctx = GetContext())
            //{
            //    var persisted = collectionNabber(ctx).FirstOrDefault();
            //    Assert.IsNotNull(persisted, "No entity of type '" + typeof(TEntity).FullName + "' found in context after saving!");
            //    var entityType = typeof (TEntity);
            //    var idProp = entityType.GetProperties().FirstOrDefault(pi => pi.Name.ToLower() == entityType.Name.ToLower() + "id");
            //    if (idProp != null && !ignoreProperties.Contains(idProp.Name))
            //        Assert.AreNotEqual((object) 0, idProp.GetValue(persisted));
            //    if (customAssertions != null)
            //    {
            //        customAssertions(sut, persisted);
            //    }
            //    var decimalProps = typeof (TEntity)
            //        .GetProperties()
            //        .Where(pi => pi.PropertyType == typeof (decimal));
            //    var ignore = new[]
            //    {
            //        LAST_MODIFIED,
            //        CREATED,
            //        ENABLED
            //    }.Union(ignoreProperties).Union(decimalProps.Select(pi => pi.Name)).ToArray();
            //    PropertyAssert.AllPropertiesAreEqual(persisted, sut, ignore);
            //    foreach (var pi in decimalProps)
            //    {
            //        var sutValue = (decimal)pi.GetValue(sut);
            //        var persistedValue = (decimal) pi.GetValue(persisted);
            //        persistedValue.ShouldMatch(sutValue);
            //    }
            //}

        }

        internal override void SetEntityFactory(Func<TEntity> factory)
        {
            _entityFactory = factory;
        }

        private TContext GetContext()
        {
            return _context ?? (_context = CreateContext(GetConnection()));
        }

        private DbConnection GetConnection()
        {
            var tempDb = GetTempDb();
            return tempDb.CreateConnection();
        }

        private TempDBLocalDb GetTempDb()
        {
            return _tempDb ?? (_tempDb = new TempDBLocalDb());
        }
    }
}