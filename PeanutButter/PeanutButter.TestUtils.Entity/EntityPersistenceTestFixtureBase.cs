using System;
using System.Data.Entity;
using System.Linq;
using EntityUtilities;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Entity
{
    public class EntityPersistenceTestFixtureBase<TDbContext>
        : TestFixtureWithTempDb<TDbContext>  where TDbContext: DbContext
    {
        private const string CREATED = "Created";
        private const string LAST_MODIFIED = "LastModified";
        private const string ENABLED = "Enabled";

        protected readonly string[] _ignoreFields = {CREATED, LAST_MODIFIED, ENABLED};
        protected bool _logEntitySql = false;

        protected string[] DefaultIgnoreFieldsFor<T>()
        {
            return _ignoreFields.And(typeof(T).VirtualProperties());
        }

        public void ShouldBeAbleToPersist<TBuilder, TEntity>(Func<TDbContext, IDbSet<TEntity>>  collectionNabber, 
            Action<TDbContext, TEntity> beforePersisting = null, 
            Action<TEntity, TEntity> customAssertions = null,
            params string[] ignoreProperties) 
            where TBuilder: GenericBuilder<TBuilder, TEntity>, new()
            where TEntity: class
        {
            //---------------Set up test pack-------------------
            var sut = (new TBuilder()).WithRandomProps().Build();
            using (var ctx = GetContext(_logEntitySql))
            {
                if (ctx is DbContextWithAutomaticTrackingFields)
                {
                    var entity = sut as EntityBase;
                    if (entity != null)
                        entity.Created = default(DateTime);
                }
                var beforeTest = DateTime.Now;
                //---------------Assert Precondition----------------
                if (beforePersisting != null)
                {
                    beforePersisting(ctx, sut);
                }
                Assert.IsFalse(collectionNabber(ctx).Any());
                //---------------Execute Test ----------------------
                collectionNabber(ctx).Add(sut);
                DbContextExtensions.SaveChangesWithErrorReporting(ctx);
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
            using (var ctx = GetContext())
            {
                var persisted = collectionNabber(ctx).First();
                var entityType = typeof (TEntity);
                var idProp = entityType.GetProperties().FirstOrDefault(pi => pi.Name == entityType.Name + "Id");
                if (!ignoreProperties.Contains(idProp.Name))
                    Assert.AreNotEqual((object) 0, idProp.GetValue(persisted));
                if (customAssertions != null)
                {
                    customAssertions(sut, persisted);
                }
                var decimalProps = typeof (TEntity)
                    .GetProperties()
                    .Where(pi => pi.PropertyType == typeof (decimal));
                var ignore = new[]
                {
                    LAST_MODIFIED,
                    CREATED,
                    ENABLED
                }.Union(ignoreProperties).Union(decimalProps.Select(pi => pi.Name)).ToArray();
                PropertyAssert.AllPropertiesAreEqual(persisted, sut, ignore);
                foreach (var pi in decimalProps)
                {
                    var sutValue = (decimal)pi.GetValue(sut);
                    var persistedValue = (decimal) pi.GetValue(persisted);
                    persistedValue.ShouldMatch(sutValue);
                }
            }
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