using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        // ReSharper disable once NotAccessedField.Global
        // ReSharper disable once MemberCanBePrivate.Global
        protected TSubject CurrentEntity;

        protected TBuilder With(
            Action<TSubject> action
        )
        {
            return With(action, Guid.NewGuid().ToString());
        }

        protected TBuilder With(
            Action<TSubject> action,
            string identifier
        )
        {
            var mutator = new Mutator<TSubject>(action, identifier);
            StoreMutator(mutator);
            return this as TBuilder;
        }

        private void StoreMutator(Mutator<TSubject> mutator)
        {
            var existing = _mutators.Where(o => o.Identifier == mutator.Identifier)
                .ToArray();
            foreach (var item in existing)
            {
                _mutators.Remove(item);
            }
            _mutators.Add(mutator);
            
        }

        protected TBuilder With<TCast>(
            Action<TCast> action
        ) where TCast : TSubject
        {
            return With(action, Guid.NewGuid().ToString());
        }

        protected TBuilder With<TCast>(
            Action<TCast> action,
            string identifier
        ) where TCast: TSubject
        {
            var mutator = new Mutator<TSubject>(o => action((TCast) o), identifier);
            StoreMutator(mutator);
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
                mutator.Action(subject);
            }

            return subject;
        }

        protected virtual TSubject ConstructEntity()
        {
            try
            {
                return Activator.CreateInstance<TSubject>();
            }
            catch
            {
                throw new CustomConstructEntityRequired(
                    GetType()
                );
            }
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

        private readonly List<Mutator<TSubject>> _mutators = new();

        private class Mutator<T>
        {
            public Action<T> Action { get; }
            public string Identifier { get; }

            public Mutator(
                Action<T> action,
                string identifier
            )
            {
                Action = action;
                Identifier = identifier;
            }
        }

        protected static void WarnIf(
            bool condition,
            string message
        )
        {
#if DEBUG
            if (condition)
            {
                Trace.WriteLine(message);
            }
#endif
        }
    }

    internal class CustomConstructEntityRequired
        : Exception
    {
        public CustomConstructEntityRequired(
            Type outerType
        ) : base(
            $@"Please implement {outerType.Name}.ConstructEntity"
        )
        {
        }
    }
}