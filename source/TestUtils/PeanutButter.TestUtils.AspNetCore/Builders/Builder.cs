using System;
using System.Collections.Generic;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    public abstract class Builder<TBuilder, TSubject>
        where TBuilder : Builder<TBuilder, TSubject>
    {
        protected TSubject CurrentEntity;
    
        protected TBuilder With(
            Action<TSubject> mutator
        )
        {
            _mutators.Add(mutator);
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
            return CurrentEntity = Activator.CreateInstance<TSubject>();
        }

        public virtual TSubject Build()
        {
            var result = ApplyMutators(ConstructEntity());
            CurrentEntity = default;
            return result;
        }

        private readonly List<Action<TSubject>> _mutators = new();
    }
}