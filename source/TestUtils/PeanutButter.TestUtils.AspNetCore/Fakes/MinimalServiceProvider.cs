using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Provides a minimal implementation of IServiceProvider
/// - can register types and factories for types
/// - some level of auto-registration is available where constructor
///     parameters are either empty or can also be resolved:
///     - single implementations for interfaces
///     - concrete types
/// - possibly the simplest DI "framework" in the world...
/// </summary>
public interface IMinimalServiceProvider : IServiceProvider
{
    /// <summary>
    /// Resolve the service TService
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    TService Resolve<TService>();

    /// <summary>
    /// Register a transient factory for the service
    /// </summary>
    /// <param name="factory"></param>
    /// <typeparam name="TService"></typeparam>
    void Register<TService>(Func<object> factory);

    /// <summary>
    /// Register a transient type-map for a service
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    void Register<TService, TImplementation>()
        where TImplementation : TService;

    /// <summary>
    /// Register an instance for a service request
    /// </summary>
    /// <param name="service"></param>
    /// <typeparam name="TService"></typeparam>
    void RegisterInstance<TService>(TService service);

    /// <summary>
    /// Register a singleton type-map for a service
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    void RegisterSingleton<TService, TImplementation>() where TImplementation : TService;

    /// <summary>
    /// Register a singleton factory for a service
    /// </summary>
    /// <param name="factory"></param>
    /// <typeparam name="TService"></typeparam>
    void RegisterSingleton<TService>(Func<TService> factory);
}

internal class DefaultJsonOptions : IOptions<JsonOptions>
{
    public JsonOptions Value { get; }
        = new JsonOptions();
}

/// <summary>
/// Provides a very minimal service provider
/// </summary>
public class MinimalServiceProvider : IFake, IMinimalServiceProvider
{
    /// <summary>
    /// 
    /// </summary>
    public MinimalServiceProvider()
    {
        // To do WriteJsonAsync, the writer needs to know about json options...
        RegisterInstance<IOptions<JsonOptions>>(new DefaultJsonOptions());
        RegisterInstance<IActionResultExecutor<ContentResult>>(
            new FakeContentResultExecutor()
        );
        RegisterInstance<IActionResultExecutor<ObjectResult>>(
            new FakeObjectResultExecutor()
        );
        var viewResultExecutor = new FakeViewResultExecutor();
        RegisterInstance<IActionResultExecutor<PartialViewResult>>(
            viewResultExecutor
        );
        RegisterInstance<IActionResultExecutor<ViewResult>>(
            viewResultExecutor
        );
    }

    /// <inheritdoc />
    public object GetService(Type serviceType)
    {
        return TryGetService(serviceType)
            ?? throw new NotImplementedException(
                $"No factory registered or could be generated for {serviceType}"
            );
    }

    /// <summary>
    /// Attempt to resolve the service, returning null if we can't
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public object TryGetService(Type serviceType)
    {
        if (_factories.TryGetValue(serviceType, out var factory))
        {
            return factory();
        }

        if (TryRegisterAutoFactory(serviceType, out factory))
        {
            return factory();
        }

        return null;
    }

    /// <inheritdoc />
    public T Resolve<T>()
    {
        return (T) GetService(typeof(T));
    }

    /// <inheritdoc />
    public void Register<TService>(Func<object> factory)
    {
        _factories[typeof(TService)] = factory;
    }

    /// <inheritdoc />
    public void Register<TService, TImplementation>()
        where TImplementation : TService
    {
        Register<TService>(DefaultBuilderFor(typeof(TImplementation)));
    }

    /// <inheritdoc />
    public void RegisterInstance<TService>(TService service)
    {
        RegisterSingleton(() => service);
    }

    /// <inheritdoc />
    public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService
    {
        // ReSharper disable once ConvertClosureToMethodGroup
        RegisterSingleton<TService>(() => Resolve<TImplementation>());
    }

    /// <inheritdoc />
    public void RegisterSingleton<TService>(Func<TService> factory)
    {
        var resolved = false;
        TService resolvedValue = default;
        Register<TService>(
            () =>
            {
                if (resolved)
                {
                    return resolvedValue;
                }

                resolvedValue = factory();
                resolved = true;
                return resolvedValue;
            }
        );
    }

    private bool TryRegisterAutoFactory(Type serviceType, out Func<object> factory)
    {
        factory = default;
        if (serviceType is null)
        {
            return false;
        }

        if (!serviceType.IsAbstract && !serviceType.IsInterface)
        {
            factory = DefaultBuilderFor(serviceType);
            return true;
        }

        if (serviceType.IsInterface && TryFindSingleImplementationOf(serviceType, out var implementationType))
        {
            factory = DefaultBuilderFor(implementationType);
            return true;
        }

        return false;
    }

    private bool TryFindSingleImplementationOf(Type serviceType, out Type impl)
    {
        impl = null;
        if (!serviceType.IsInterface)
        {
            return false;
        }

        RefreshSeenTypes();
        var possible = new List<Type>();
        foreach (var t in AllTypes)
        {
            var interfaces = AllInterfacesFor(t);
            if (interfaces.Contains(serviceType) &&
                !t.IsAbstract)
            {
                possible.Add(t);
            }
        }

        var hasSingleImplementation = possible.Count == 1;
        if (hasSingleImplementation)
        {
            impl = possible[0];
        }

        // try to find one with no parameters
        var parameterless = possible.FirstOrDefault(
            t => t.GetConstructors().Any(
                c => c.IsPublic && c.GetParameters().Length == 0
            )
        );
        if (parameterless is not null)
        {
            impl = parameterless;
            return true;
        }

        var constructable = possible.FirstOrDefault(
            CanResolveAllConstructorParameters
        );

        if (constructable is not null)
        {
            impl = constructable;
            return true;
        }

        return false;
    }

    private bool CanResolveAllConstructorParameters(Type arg)
    {
        foreach (var constructor in arg.GetConstructors())
        {
            if (CanResolveAllParameters(constructor))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanResolveAllParameters(ConstructorInfo constructor)
    {
        foreach (var p in constructor.GetParameters())
        {
            if (TryGetService(p.ParameterType) is null)
            {
                return false;
            }
        }

        return true;
    }

    private static HashSet<Type> AllInterfacesFor(Type t)
    {
        if (InterfaceMap.TryGetValue(t, out var result))
        {
            return result;
        }

        result = new HashSet<Type>(t.GetInterfaces());
        InterfaceMap.TryAdd(t, result);
        return result;
    }

    private void RefreshSeenTypes()
    {
        lock (SeenAssemblies)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (SeenAssemblies.Contains(asm))
                {
                    continue;
                }

                SeenAssemblies.Add(asm);
                try
                {
                    AllTypes.AddRange(asm.GetExportedTypes());
                }
                catch
                {
                    // suppress
                }
            }
        }
    }

    private static readonly HashSet<Assembly> SeenAssemblies = new();
    private static readonly List<Type> AllTypes = new();
    private static readonly ConcurrentDictionary<Type, HashSet<Type>> InterfaceMap = new();

    private Func<object> DefaultBuilderFor(Type t)
    {
        return () => Activator.CreateInstance(t, ResolveConstructorParametersFor(t));
    }

    private object[] ResolveConstructorParametersFor(Type type)
    {
        var constructorInfo = type.GetConstructors()
            .Where(c => c.IsPublic)
            .Select(c => (constructor: c, parameters: c.GetParameters()))
            .OrderByDescending(o => o.parameters.Length)
            .ToArray();

        var unresolved = new List<List<Type>>();
        foreach (var info in constructorInfo)
        {
            var thisUnresolved = new List<Type>();
            unresolved.Add(thisUnresolved);
            var parameters = new List<object>();
            var allResolved = true;
            foreach (var pt in info.parameters.Select(p => p.ParameterType))
            {
                try
                {
                    parameters.Add(GetService(pt));
                }
                catch
                {
                    thisUnresolved.Add(pt);
                    allResolved = false;
                }
            }

            if (allResolved)
            {
                return parameters.ToArray();
            }
        }

        throw new ArgumentException(
            $@"Unable to resolve constructor parameters for {type}. Manually resolve one of the following constructors:
{string.Join("\n", unresolved.Select(l => string.Join(",", l)))}
",
            nameof(type)
        );
    }

    private readonly ConcurrentDictionary<Type, Func<object>> _factories = new();
}