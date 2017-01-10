using System;
using System.Data.Common;
using System.Data.Entity;

namespace PeanutButter.TestUtils.Entity
{
    public class EntityPersistenceTesterFor<TEntity> where TEntity: class
    {
        private readonly Func<TEntity> _entityFactory;

        public EntityPersistenceTesterFor(Func<TEntity> entityFactory = null)
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