using System;
using System.Collections.Generic;
using PeanutButter.Utils.Dictionaries;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif

/// <summary>
/// Provides basic extensions for using a mocked
/// store against any object, levering off of
/// PeanutButter's metadata logic
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    static class MockStoreExtensions
{
    /// <summary>
    /// Finds or creates a validating store with integer primary key
    /// and TEntity entries (ie IDictionary&lt;int, TEntity&gt;, but
    /// not allowing invalid PK values (ie ids must be > 0)). The
    /// store is associated with the owning object for future use.
    /// </summary>
    /// <param name="owner"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static IDictionary<int, TEntity> FindOrAddMockStoreFor<TEntity>(
        this object owner
    )
    {
        return owner.FindOrAddMockStore<Store<TEntity>>();
    }

    /// <summary>
    /// Finds or creates a store of the given type TStore,
    /// associated with the owner object. TStore must
    /// be a type which has a parameterless constructor.
    /// </summary>
    /// <param name="owner"></param>
    /// <typeparam name="TStore"></typeparam>
    /// <returns></returns>
    public static TStore FindOrAddMockStore<TStore>(
        this object owner
    ) where TStore : new()
    {
        return owner.FindOrAddMockStore<TStore>(() => new());
    }

    /// <summary>
    /// Finds or creates a store of the given type TStore,
    /// associated with the owner object. If the store
    /// doesn't already exist, it is created with the
    /// provided factory.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="factory"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static TResult FindOrAddMockStore<TResult>(
        this object owner,
        Func<TResult> factory
    )
    {
        var key = $"{KEY_PREFIX}:{typeof(TResult).PrettyName()}";
        return owner.TryGetMetadata<TResult>(key, out var existing)
            ? existing
            : CreateStore();

        TResult CreateStore()
        {
            if (owner.TryGetMetadata<object>(key, out var obj))
            {
                throw new InvalidOperationException(
                    $"{owner} already has a store of type {obj.GetType()}"
                );
            }

            var store = factory();
            owner.SetMetadata(key, store);
            return store;
        }
    }

    private const string KEY_PREFIX = "__mock_store__";

    private class Store<T> : ValidatingDictionary<int, T>
    {
        public Store() : base(Validate)
        {
        }

        private static void Validate(
            IDictionary<int, T> store,
            int key,
            T entity,
            Mutation mutation
        )
        {
            if (key < 1)
            {
                throw new ArgumentException(
                    "Fake stores may only be populated with items that have a natural-number (ie > 0) key (id)",
                    nameof(key)
                );
            }
        }
    }
}