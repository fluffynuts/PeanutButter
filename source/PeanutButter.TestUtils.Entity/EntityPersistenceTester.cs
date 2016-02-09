using System;
using System.Data.Common;
using System.Data.Entity;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.Entity
{
    public class EntityPersistenceTester
    {
        public static EntityPersistenceTester<T> CreateFor<T>() where T : class
        {
            return new EntityPersistenceTester<T>();
        } 
    }

    public class EntityPersistenceTester<TEntity> where TEntity: class
    {
        private readonly Func<TEntity> _entityFactory;

        public EntityPersistenceTester(Func<TEntity> entityFactory = null)
        {
            _entityFactory = entityFactory;
        }

        public EntityPersistenceFluentState<TContext, TEntity> WithContext<TContext>(Func<DbConnection, TContext> contextFactory = null) where TContext: DbContext
        {
            return new EntityPersistenceFluentState<TContext, TEntity>(_entityFactory, contextFactory)
                            .WithEntityFactory(_entityFactory);
        }

    }
}