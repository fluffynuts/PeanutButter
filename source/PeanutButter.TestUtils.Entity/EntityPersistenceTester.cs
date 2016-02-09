using System;
using System.Data.Common;
using System.Data.Entity;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.Entity
{
    public class EntityPersistenceTester<TEntity> where TEntity: class
    {
        private Func<TEntity> _entityFactory;
        private EntityPersistenceTestContextBase<TEntity> _testContext;

        public static EntityPersistenceTester<T> CreateFor<T>() where T : class
        {
            return new EntityPersistenceTester<T>();
        } 

        public EntityPersistenceTester(Func<TEntity> entityFactory = null)
        {
            _entityFactory = entityFactory ?? Activator.CreateInstance<TEntity>;
        } 

        public EntityPersistenceTester<TEntity> WithContext<TContext>(Func<DbConnection, TContext> contextFactory = null) where TContext: DbContext
        {
            _testContext = new EntityPersistenceTestContext<TContext, TEntity>(contextFactory);
            return this;
        }

        public EntityPersistenceTester<TEntity> WithCollection<TContext>(Func<TContext, IDbSet<TEntity>> collectionNabber) where TContext: DbContext
        {
            var specificContext = GetSpecificContext<TContext>();
            specificContext.SetCollectionNabber(collectionNabber);
            return this;
        }

        public EntityPersistenceTester<TEntity> BeforePersisting<TContext>(Action<TContext, TEntity> runBeforePersisting) where TContext: DbContext
        {
            var specificContext = GetSpecificContext<TContext>();
            specificContext.SetRunBeforePersisting(runBeforePersisting);
            return this;
        }

        public EntityPersistenceTester<TEntity> AfterPersisting(Action<TEntity, TEntity> runAfterPersisting)
        {
            _testContext.SetAfterPersisting(runAfterPersisting);
            return this;
        } 

        public EntityPersistenceTester<TEntity> WithIgnoredProperties(params string[] propertyNames)
        {
            _testContext.SetIgnoredProperties(propertyNames);
            return this;
        }

        public EntityPersistenceTester<TEntity> WithBuilder<TEntityBuilder>() where TEntityBuilder: GenericBuilder<TEntityBuilder, TEntity>
        {
            _entityFactory = BuildWithBuilder<TEntityBuilder>;
            return this;
        }

        private TEntity BuildWithBuilder<TEntityBuilder>() where TEntityBuilder: GenericBuilder<TEntityBuilder, TEntity>
        {
            //var builder = 
            return null;
        }

        public void ShouldPersistAndRecall()
        {
            _testContext.SetEntityFactory(_entityFactory);
            _testContext.PerformTest();
        }

        private EntityPersistenceTestContext<TContext, TEntity> GetSpecificContext<TContext>() where TContext : DbContext
        {
            var specificContext = _testContext as EntityPersistenceTestContext<TContext, TEntity>;
            if (specificContext == null)
                throw new ArgumentException("Collection nabber appears to operate on incorrect context");
            return specificContext;
        }
    }
}