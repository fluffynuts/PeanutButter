using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable StaticMemberInGenericType

namespace PeanutButter.Utils
{
    /// <summary>
    /// Defines an action to run on a reference to a value type
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam name="T1"></typeparam>
    public delegate void ActionRef<T1>(ref T1 item);
    
    /// <summary>
    /// Most basic interface to implement to be considered a builder
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity this builder should build</typeparam>
    /// <typeparam name="TBuilder"></typeparam>
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IBuilder<TBuilder, TEntity>
    {
        /// <summary>
        /// Builds a new instance of TSubject
        /// </summary>
        /// <returns>New instance of TSubject</returns>
        TEntity Build();
        
        /// <summary>
        /// Queues a transform to be run at build time on entities
        /// you wish to build; use this on reference types
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        TBuilder WithProp(Action<TEntity> transform);
        
        /// <summary>
        /// Queues a transform to be run at build time on entities
        /// you wish to build; use this on value types
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        TBuilder WithProp(ActionRef<TEntity> transform);
    }
    
    /// <summary>
    /// Provides a base class with simple builder functionality
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class Builder<TBuilder, TEntity>: IBuilder<TBuilder, TEntity>
        where TBuilder : Builder<TBuilder, TEntity>, IBuilder<TBuilder, TEntity>, new()
    {
        private static readonly Type EntityType = typeof(TEntity);

        private static readonly bool IsInterfaceType = EntityType.IsInterface;

        private static readonly bool HasParameterlessConstructor =
            EntityType.GetConstructors()
                .Any(ctor => ctor.GetParameters().Length == 0);

        private readonly List<ActionRef<TEntity>> _transforms
            = new List<ActionRef<TEntity>>();

        /// <summary>
        /// Creates an instance of this builder; used for fluent
        /// code like:
        /// SomeBuilder.Create()
        ///   .WithStuff()
        ///   .Build();
        /// </summary>
        public static TBuilder Create()
        {
            return new TBuilder();
        }

        /// <summary>
        /// Builds a default instance of TEntity
        /// based on your builder's ConstructEntity and Build methods
        /// (ie convenience wrapper around YourBuilder.Create().Build())
        /// </summary>
        /// <returns></returns>
        public static TEntity BuildDefault()
        {
            return Create().Build();
        }

        /// <summary>
        /// Allows the derivative class to implement a custom
        /// strategy for entity construction; for example:
        /// - providing constructor parameters
        /// - returning an implementation for an interface
        /// </summary>
        /// <returns></returns>
        protected virtual TEntity ConstructEntity()
        {
            if (IsInterfaceType)
            {
                return ThrowUnconstructable($"{EntityType} is an interface");
            }

            try
            {
                if (HasParameterlessConstructor)
                {
                    return Activator.CreateInstance<TEntity>();
                }

                if (!EntityType.IsClass)
                {
                    return default(TEntity);
                }

                return ThrowUnconstructable(
                    $"{EntityType} has no parameterless constructor and is not a class type"
                );
            }
            catch (Exception ex)
            {
                return ThrowUnconstructable(
                    $"Unable to create instance of {EntityType}: {ex.Message}",
                    ex
                );
            }
        }

        private TEntity ThrowUnconstructable(
            string info,
            Exception ex = null)
        {
            throw new NotImplementedException(
                $"{info}\nPlease override {nameof(ConstructEntity)} in {typeof(TBuilder)} to handle construction of {EntityType}",
                ex
            );
        }

        /// <summary>
        /// Generic property mutator to apply to built entity
        /// of pass-by-value type (eg a struct)
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public TBuilder WithProp(ActionRef<TEntity> transform)
        {
            _transforms.Add(transform);
            return this as TBuilder;
        }

        /// <summary>
        /// Generic property mutator to apply to built entity
        /// of pass-by-ref type (eg class)
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public TBuilder WithProp(Action<TEntity> transform)
        {
            _transforms.Add((ref TEntity item) => transform(item));
            return this as TBuilder;
        }

        /// <summary>
        /// Attempts to:
        /// - construct the entity
        /// - apply all transforms
        /// - return the entity
        /// </summary>
        /// <returns></returns>
        public virtual TEntity Build()
        {
            lock (this)
            {
                var snapshot = _transforms.ToArray();
                _transforms.Clear();
                var entity = ConstructEntity();
                var initialTransforms = snapshot.Concat(
                    _transforms
                ).ToArray();
                _transforms.Clear();
                RunTransforms(ref entity, initialTransforms);
                _transforms.AddRange(snapshot);
                return entity;
            }
        }

        private void RunTransforms(
            ref TEntity entity,
            ActionRef<TEntity>[] transforms,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            int depth = 0)
        {
            if (depth > 15)
            {
                throw new InvalidOperationException(
                    $"One or more of the transforms on {EntityType} is too re-entrant. Stopping now to avoid stack-overflow."
                );
            }

            var queue = new Queue<ActionRef<TEntity>>(transforms);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                current(ref entity);
                var generated = _transforms.ToArray();
                _transforms.Clear();
                RunTransforms(ref entity, generated, depth + 1);
            }
        }
    }
}