using System;
using System.Data.Entity;

namespace PeanutButter.TestUtils.Entity
{
    internal abstract class EntityPersistenceTestContextBase<TEntity> where TEntity: class
    {
        protected abstract IDbSet<TEntity> GetCollection();
        internal abstract void SetAfterPersisting(Action<TEntity, TEntity> runAfterPersisting);
        internal abstract void SetIgnoredProperties(params string[] propertyNames);
        internal abstract void PerformTest();
        internal abstract void SetEntityFactory(Func<TEntity> factory);
    }
}