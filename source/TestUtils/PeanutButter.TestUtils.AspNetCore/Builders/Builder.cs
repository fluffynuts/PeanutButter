using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// Provides the base builder for AspNetCore fakes
/// </summary>
/// <typeparam name="TBuilder"></typeparam>
/// <typeparam name="TSubject"></typeparam>
public abstract class Builder<TBuilder, TSubject>
    where TBuilder : Builder<TBuilder, TSubject>, new()
{
    private readonly Action<TSubject>[] _actualizers;

    /// <summary>
    /// Returns a new instance of the builder
    /// </summary>
    /// <returns></returns>
    public static TBuilder Create()
    {
        return new TBuilder();
    }

    internal Builder(
        params Action<TSubject>[] actualizers
    )
    {
        _actualizers = actualizers;
    }

    /// <summary>
    /// Builds the default output artifact
    /// </summary>
    /// <returns></returns>
    public static TSubject BuildDefault()
    {
        return Create().Build();
    }

    // ReSharper disable once NotAccessedField.Global
    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// During build, CurrentEntity will be set to the currently-building entity.
    /// You may implement actualizer(s) in your derivative to pull this value in
    /// lazily to consumers
    /// </summary>
    protected TSubject CurrentEntity;

    /// <summary>
    /// Adds a mutator for the artifact
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    // ReSharper disable once MemberCanBeProtected.Global
    public TBuilder With(
        Action<TSubject> action
    )
    {
        return With(action, Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Adds an identified mutator for the artifact - if
    /// a mutator with the same identity already exists, it will be removed
    /// </summary>
    /// <param name="action"></param>
    /// <param name="identifier"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Adds a mutator for the artifact, cast to TCast
    /// </summary>
    /// <param name="action"></param>
    /// <typeparam name="TCast"></typeparam>
    /// <returns></returns>
    protected TBuilder With<TCast>(
        Action<TCast> action
    ) where TCast : TSubject
    {
        return With(action, Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Adds an identified mutator for the artifact, cast to TCast
    /// </summary>
    /// <param name="action"></param>
    /// <param name="identifier"></param>
    /// <typeparam name="TCast"></typeparam>
    /// <returns></returns>
    protected TBuilder With<TCast>(
        Action<TCast> action,
        string identifier
    ) where TCast : TSubject
    {
        var mutator = new Mutator<TSubject>(o => action((TCast) o), identifier);
        StoreMutator(mutator);
        return this as TBuilder;
    }

    /// <summary>
    /// Applies the given mutator a random (1-4) number of times
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    protected TBuilder WithRandomTimes(Action<TSubject> action)
    {
        var howMany = GetRandomInt(1, 4);
        for (var i = 0; i < howMany; i++)
        {
            With(action);
        }

        return this as TBuilder;
    }

    /// <summary>
    /// Applies the given mutator on a cast item a random (1-4) number of times
    /// </summary>
    /// <param name="action"></param>
    /// <typeparam name="TCast"></typeparam>
    /// <returns></returns>
    protected TBuilder WithRandomTimes<TCast>(Action<TCast> action) where TCast : TSubject
    {
        return WithRandomTimes(
            o => action((TCast) o)
        );
    }

    private TSubject ApplyMutators(
        TSubject subject
    )
    {
        foreach (var mutator in _mutators)
        {
            mutator.Action(subject);
        }

        return subject;
    }

    /// <summary>
    /// Override in a derivative builder to provide a custom implementation
    /// for TSubject
    /// </summary>
    /// <returns></returns>
    /// <exception cref="CustomConstructEntityRequired"></exception>
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

    /// <summary>
    /// Builds the subject artifact
    /// </summary>
    /// <returns></returns>
    public virtual TSubject Build()
    {
        foreach (var action in _precursors)
        {
            action();
        }

        var result = ApplyMutators(
            ConstructAndStoreEntity()
        );
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

    /// <summary>
    /// Print a traced warning if the condition is found to be false
    /// - useful to force actualization and print out if the actualization failed
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="message"></param>
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

    /// <summary>
    /// Print a traced warning if the condition is found to be false
    /// - useful to force actualization and print out if the actualization failed
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="message"></param>
    protected static void ErrorIf(
        bool condition,
        string message
    )
    {
        if (condition)
        {
            throw new BuilderException(message);
        }
    }

    private readonly List<Action> _precursors = new();

    /// <summary>
    /// Run a precursor before your entity is constructed - use when
    /// your entity requires constructor parameters you'd like to make
    /// user-tweakable via builder methods
    /// </summary>
    /// <param name="toRun"></param>
    protected TBuilder WithPreCursor(Action toRun)
    {
        _precursors.Add(toRun);
        return this as TBuilder;
    }
}

internal class BuilderException : Exception
{
    public BuilderException(string message) : base($"Error during build: {message}")
    {
    }
}