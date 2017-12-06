using System;
using System.Data.Entity;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using PeanutButter.Utils.Entity;

namespace PeanutButter.TestUtils.Entity
{
    public class EntityPersistenceTestFixtureBase<TDbContext>
        : TestFixtureWithTempDb<TDbContext>  where TDbContext: DbContext
    {
        private const string CREATED = "Created";
        private const string LAST_MODIFIED = "LastModified";
        private const string ENABLED = "Enabled";

        private readonly string[] _ignoreFields = {CREATED, LAST_MODIFIED, ENABLED};
        protected bool LogEntitySql { get; set; }

        protected string[] DefaultIgnoreFieldsFor<T>()
        {
            return _ignoreFields.And(typeof(T).VirtualProperties());
        }

        protected void ShouldBeAbleToPersist<TBuilder, TEntity>(Func<TDbContext, IDbSet<TEntity>>  collectionNabber,
            Action<TDbContext, TEntity> beforePersisting = null,
            Action<TEntity, TEntity> customAssertions = null,
            params string[] ignoreProperties)
            where TBuilder: GenericBuilder<TBuilder, TEntity>, new()
            where TEntity: class
        {
            EntityPersistenceTester.CreateFor<TEntity>()
                .WithContext(conn => GetContext())
                .SuppressMissingMigratorMessage()
                .WithTempDbFactory(CreateNewTempDb)
                .WithCollection(collectionNabber)
                .BeforePersisting((ctx, entity) =>
                {
                    beforePersisting?.Invoke(ctx, entity);
                })
                .AfterPersisting(customAssertions)
                .AfterPersisting((ctx, e) =>
                {
                    RollBackIsolationTransaction();
                })
                .WithIgnoredProperties(ignoreProperties)
                .ShouldPersistAndRecall();
        }


        public void Type_ShouldInheritFromEntityBase<TEntity>()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (TEntity);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<EntityBase>();

            //---------------Test Result -----------------------
        }
    }
}