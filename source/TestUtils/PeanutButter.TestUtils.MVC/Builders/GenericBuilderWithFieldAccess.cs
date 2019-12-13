using System;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.MVC.Builders
{
    public abstract class GenericBuilderWithFieldAccess<TBuilder, TEntity>
        : GenericBuilder<TBuilder, TEntity>
        where TBuilder : GenericBuilder<TBuilder, TEntity>
    {
        protected TBuilder WithField(Action<TBuilder> action)
        {
            action(this as TBuilder);
            return this as TBuilder;
        }
    }
}