using System;
using System.Collections.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    public abstract class Builder<TBuilder, TSubject>
        where TBuilder : Builder<TBuilder, TSubject>
    {
        private readonly Action<TSubject>[] _actualizers = new Action<TSubject>[0];

        public static TBuilder Create()
        {
            return Activator.CreateInstance<TBuilder>();
        }

        public Builder()
        {
        }

        internal Builder(
            params Action<TSubject>[] actualizers
        )
        {
            _actualizers = actualizers;
        }

        public static TSubject BuildDefault()
        {
            return Create().Build();
        }

        public static TSubject BuildRandom()
        {
            return Create().Randomize().Build();
        }

        public abstract TBuilder Randomize();

        protected TSubject CurrentEntity;

        protected TBuilder With(
            Action<TSubject> mutator
        )
        {
            _mutators.Add(mutator);
            return this as TBuilder;
        }

        protected TBuilder With<TCast>(
            Action<TCast> mutator
        ) where TCast : TSubject
        {
            _mutators.Add(o => mutator((TCast) o));
            return this as TBuilder;
        }

        protected TBuilder WithRandomTimes(Action<TSubject> action)
        {
            var howMany = GetRandomInt(1, 4);
            for (var i = 0; i < howMany; i++)
            {
                With(action);
            }

            return this as TBuilder;
        }

        protected TSubject ApplyMutators(
            TSubject subject
        )
        {
            foreach (var mutator in _mutators)
            {
                mutator(subject);
            }

            return subject;
        }

        protected virtual TSubject ConstructEntity()
        {
            return Activator.CreateInstance<TSubject>();
        }

        private TSubject ConstructAndStoreEntity()
        {
            return CurrentEntity = ConstructEntity();
        }

        public virtual TSubject Build()
        {
            
            var result = ApplyMutators(ConstructAndStoreEntity());
            foreach (var actualizer in _actualizers)
            {
                actualizer(result);
            }

            CurrentEntity = default;
            return result;
        }

        private readonly List<Action<TSubject>> _mutators = new();
    }
}