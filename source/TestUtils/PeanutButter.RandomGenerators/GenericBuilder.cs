/* Generic Builder base class
 * author: Davyd McColl (davydm@gmail.com)
 * license: BSD
 * */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable InheritdocConsiderUsage
// ReSharper disable UsePatternMatching
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Local
#pragma warning disable 168


namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Base class for builders to produce instance of objects with a fluent
    /// builder-like syntax. Also includes utilities like randomizing property
    /// values.
    /// </summary>
    /// <typeparam name="TBuilder">Concrete type of the current builder, required to be able to return the builder from all With* methods</typeparam>
    /// <typeparam name="TEntity">Type of entity this builder builds</typeparam>
    public class GenericBuilder<TBuilder, TEntity> : GenericBuilderBase, IGenericBuilder, IBuilder<TEntity>
        where TBuilder : GenericBuilder<TBuilder, TEntity>
    {
        /// <summary>
        /// The delegate type for an action which takes one parameter
        /// of any type, by reference.
        /// </summary>
        /// <typeparam name="T1">Type of the parameter</typeparam>
        /// <param name="item">Reference to the item being passed in</param>
        public delegate void ActionRef<T1>(ref T1 item);

        private delegate void ActionRef<T1, in T2>(ref T1 item, T2 index);

        private static readonly List<ActionRef<TEntity>> DefaultPropMods = new List<ActionRef<TEntity>>();
        private readonly List<ActionRef<TEntity>> _propMods = new List<ActionRef<TEntity>>();
        private readonly List<ActionRef<TEntity>> _buildTimePropMods = new List<ActionRef<TEntity>>();
        private bool _currentlyBuilding;
        private static Type _constructingTypeBackingField = typeof(TEntity);
        private DateTimeKind _defaultDateTimeKind = DateTimeKind.Unspecified;

        private static Type ConstructingType
        {
            get => _constructingTypeBackingField;
            set
            {
                lock (LockObject)
                {
                    _constructingTypeBackingField = value;
                    _randomPropSettersField = null;
                    _entityPropInfoField = null;
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the builder; used to provide a fluent syntax
        /// </summary>
        /// <returns>New instance of the builder</returns>
        public static TBuilder Create()
        {
            return Activator.CreateInstance<TBuilder>();
        }

        /// <inheritdoc />
        public IGenericBuilder GenericWithRandomProps()
        {
            return _buildLevel > MaxRandomPropsLevel
                ? this
                : WithRandomProps();
        }

        /// <inheritdoc />
        public IGenericBuilder WithBuildLevel(int level)
        {
            _buildLevel = level;
            return this;
        }

        /// <inheritdoc />
        public object GenericBuild()
        {
            return Build();
        }

        /// <inheritdoc />
        public object GenericDeepBuild()
        {
            if (_buildLevel > MaxRandomPropsLevel)
            {
                return null;
            }

            var result = Build();
            var resultType = result.GetType();
            var complexProps = resultType
                .GetProperties()
                .Select(pi => new PropertyOrField(pi))
                .Union(resultType.GetFields().Select(fi => new PropertyOrField(fi)))
                .Where(pi => !Types.PrimitivesAndImmutables.Contains(pi.Type))
                .ToArray();
            complexProps.ForEach(
                p =>
                {
                    var propertyType = p.Type;
                    var value = TryBuildInstanceOf(propertyType);
                    p.SetValue(ref result, value);
                });
            return result;
        }

        private object TryBuildInstanceOf(Type propertyType)
        {
            try
            {
                if (propertyType.IsArray || propertyType.IsGenericOfIEnumerable())
                {
                    return MakeEmptyArrayOf(propertyType.GetCollectionItemType());
                }

                var builder = GenericBuilderLocator.GetGenericBuilderInstanceFor(propertyType);
                return builder.WithBuildLevel(_buildLevel + 1).GenericBuild();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Unable to build instance of {propertyType.PrettyName()}: {ex.Message}"
                );
                return null;
            }
        }

        private object MakeEmptyArrayOf(Type elementType)
        {
            var genericType = typeof(List<>);
            var specificType = genericType.MakeGenericType(elementType);
            var instance = Activator.CreateInstance(specificType);
            return instance.InvokeMethodWithResult("ToArray");
        }

        /// <summary>
        /// Builds a default instance of the entity
        /// </summary>
        /// <returns>New instance of the builder entity</returns>
        public static TEntity BuildDefault()
        {
            return Create().Build();
        }

        /// <summary>
        /// Convenience method: Creates a builder, sets random properties, returns a new instance of the entity
        /// </summary>
        /// <returns>New instance of TEntity with all randomizable properties randomized</returns>
        public static TEntity BuildRandom()
        {
            return Create().WithRandomProps().Build();
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Adds a default property setter, shared amongst all instances of this
        /// particular builder type
        /// </summary>
        /// <param name="action">
        /// Action to perform on the entity being built, will run before any
        /// actions specified on the instance
        /// </param>
        public static void WithDefaultProp(Action<TEntity> action)
        {
            DefaultPropMods.Add((ref TEntity e) => action(e));
        }

        /// <summary>
        /// Generic method to set a property on the entity.
        /// </summary>
        /// <param name="action">Action to run on the entity at build time</param>
        /// <returns>The current instance of the builder</returns>
        public TBuilder WithProp(Action<TEntity> action)
        {
            var collection = _currentlyBuilding
                ? _buildTimePropMods
                : _propMods;
            collection.Add((ref TEntity e) => action(e));
            return this as TBuilder;
        }

        /// <summary>
        /// Generic method to set a property on an entity
        /// when that entity is a struct type.
        /// </summary>
        /// <param name="action">Action to run on the entity</param>
        /// <returns>The current instance of the builder</returns>
        public TBuilder WithProp(ActionRef<TEntity> action)
        {
            var collection = _currentlyBuilding
                ? _buildTimePropMods
                : _propMods;
            collection.Add(action);
            return this as TBuilder;
        }


        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        /// <summary>
        /// Constructs a new instance of the entity. Mostly, an inheriter won't have to
        /// care, but if your entity has no parameterless constructor, you'll want to override
        /// this in your derived builder.
        /// </summary>
        /// <returns>New instance of TEntity, constructed from the parameterless constructor, when possible</returns>
        /// <exception cref="GenericBuilderInstanceCreationException"></exception>
        public virtual TEntity ConstructEntity()
        {
            var type = typeof(TEntity);
            try
            {
                CheckUnconstructable(type);
                return AttemptToConstructEntity();
            }
            catch (Exception ex)
            {
                CacheUnconstructable(type);
#if NETSTANDARD
#else
                Trace.WriteLine($"Unable to construct entity of type {type.Name}: {ex.Message}");
#endif
                throw CreateUnconstructableException();
            }
        }

        private GenericBuilderInstanceCreationException CreateUnconstructableException()
        {
            return new GenericBuilderInstanceCreationException(GetType(), typeof(TEntity));
        }

        private void CheckUnconstructable(Type type)
        {
            lock (Unconstructables)
            {
                if (Unconstructables.Contains(type))
                    throw CreateUnconstructableException();
            }
        }

        private static void CacheUnconstructable(Type type)
        {
            lock (Unconstructables)
            {
                Unconstructables.Add(type);
            }
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly HashSet<Type> Unconstructables = new HashSet<Type>();

        private TEntity AttemptToConstructEntity()
        {
            try
            {
                return ConstructInCurrentDomain<TEntity>(ConstructingType);
            }
            catch (MissingMethodException ex)
            {
                try
                {
                    var constructed = AttemptToConstructWithImplementingType<TEntity>();
                    ConstructingType = constructed.GetType();
                    return constructed;
                }
                catch (Exception)
                {
                    var result = FallbackConstructionStrategies.Aggregate(
                        default(TEntity),
                        (acc, cur) =>
                        {
                            try
                            {
                                return acc == null || acc.Equals(default(TEntity))
                                    ? cur()
                                    : acc;
                            }
                            catch
                            {
                                return acc;
                            }
                        });
                    if (result.Equals(default(TEntity)))
                        throw ex;
                    return result;
                }
            }
        }

        private static readonly Func<TEntity>[] FallbackConstructionStrategies =
        {
            AttemptToCreateDuckFor,
            AttemptToCreateSubstituteFor<TEntity>
        };

        private static TEntity AttemptToCreateDuckFor()
        {
            var asm = FindOrLoadDuckTyping<TEntity>();
            if (asm == null)
                throw new Exception("Can't find (or load) PeanutButter.DuckTyping");
            var typeMakerType = asm.GetTypes().FirstOrDefault(t => t.Name == "TypeMaker");
            if (typeMakerType == null)
                throw new Exception("Can't find TypeMaker type in PeanutButter.DuckTyping");
            var methodInfo = typeMakerType.GetMethods()
                .FirstOrDefault(
                    mi => mi.Name == "MakeTypeImplementing" &&
                          !mi.IsGenericMethod &&
                          HasOnlyTypeParameter(mi)
                );
            if (methodInfo == null)
                throw new Exception(
                    "Can't find MakeTypeImplementing method on TypeMaker with single parameter (Type to implement)");
            var instance = Activator.CreateInstance(typeMakerType);
            ConstructingType = (Type) methodInfo.Invoke(instance, new object[] {typeof(TEntity)});
            return (TEntity) Activator.CreateInstance(ConstructingType);
        }

        private static bool HasOnlyTypeParameter(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 1 &&
                   parameters[0].ParameterType == typeof(Type);
        }

        private static Assembly FindOrLoadDuckTyping<T>()
        {
            return FindOrLoadAssembly<T>("PeanutButter.DuckTyping", false);
        }

        private static T AttemptToCreateSubstituteFor<T>()
        {
            var loadedNsubstitute = FindOrLoadNSubstitute<T>();
            if (loadedNsubstitute == null)
                throw new Exception("Can't find (or load) NSubstitute )':");
            var subType = loadedNsubstitute.GetTypes().FirstOrDefault(t => t.Name == "Substitute");
            if (subType == null)
                throw new Exception("NSubstitute assembly loaded -- but no Substitute class? )':");
            var genericMethod = subType.GetMethods()
                .FirstOrDefault(m => m.Name == "For" && IsObjectParams(m.GetParameters()));
            if (genericMethod == null)
                throw new Exception("Can't find NSubstitute.Substitute.For method )':");
            var specificMethod = genericMethod.MakeGenericMethod(typeof(T));
            return (T) specificMethod.Invoke(null, new object[] {new object[] { }});
        }

        private static Assembly FindOrLoadNSubstitute<T>()
        {
            return FindOrLoadAssembly<T>("NSubstitute", false);
        }

        private static void AttemptToLoadNSubstitute<T>()
        {
            AttemptToLoadAssemblyAlongside<T>("NSubstitute.dll");
        }

        private static Assembly FindOrLoadAssembly<T>(string name, bool retrying)
        {
            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == name);
            if (loaded != null || retrying)
                return loaded;
            AttemptToLoadAssemblyAlongside<T>($"{name}.dll");
            return FindOrLoadAssembly<T>(name, true);
        }

        private static void AttemptToLoadAssemblyAlongside<T>(string fileName)
        {
            var codeBase = new Uri(typeof(T).Assembly.CodeBase).LocalPath;
            if (!File.Exists(codeBase))
                return;
            var folder = Path.GetDirectoryName(codeBase);
            var search = Path.Combine(folder ?? "", fileName);
            if (!File.Exists(search))
                return;
            try
            {
                Assembly.Load(File.ReadAllBytes(search));
            }
            catch
            {
                /* Nothing much to be done here anyway */
            }
        }

        private static bool IsObjectParams(ParameterInfo[] parameterInfos)
        {
            return parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(object[]);
        }

        private T AttemptToConstructWithImplementingType<T>()
        {
            try
            {
                return TryCreateConcreteInstanceFromSameAssemblyAs<T>();
            }
            catch
            {
                return TryCreateConcreteInstanceFromAnyAssembly<T>();
            }
        }

        private TInterface TryCreateConcreteInstanceFromSameAssemblyAs<TInterface>()
        {
            var assembly = typeof(TInterface).Assembly;
            var type = FindImplementingTypeFor<TInterface>(new[] {assembly});
            if (type == null)
                throw new TypeLoadException();
            return ConstructInCurrentDomain<TInterface>(type);
        }

        private TInterface TryCreateConcreteInstanceFromAnyAssembly<TInterface>()
        {
            var type = FindImplementingTypeFor<TInterface>(AppDomain.CurrentDomain.GetAssemblies());
            if (type == null)
                throw new TypeLoadException();
            return ConstructInCurrentDomain<TInterface>(type);
        }

        private TInterface ConstructInCurrentDomain<TInterface>(Type type)
        {
#if NETSTANDARD
            return (TInterface) Activator.CreateInstance(type);
#else
            var handle = Activator.CreateInstance(
                AppDomain.CurrentDomain,
                type.Assembly.FullName,
                // ReSharper disable once AssignNullToNotNullAttribute
                type.FullName,
                false,
                0,
                null,
                TryToMakeConstructorParametersFor(type),
                null,
                null);
            return (TInterface) handle.Unwrap();
#endif
        }

        private object[] TryToMakeConstructorParametersFor(Type type)
        {
            var parameters = type.GetConstructors()
                .Where(c => c.IsPublic)
                .Select(c => c.GetParameters())
                .ToArray();
            if (parameters.Any(p => p.Length == 0))
                return null;
            return parameters
                .OrderByDescending(p => p.Length)
                .Select(AttemptToMakeParameters)
                .FirstOrDefault(r => r.Success)
                ?.ParameterValues;
        }

        private class ParametersAttempt
        {
            public object[] ParameterValues => CreatedValues.ToArray();
            public readonly List<object> CreatedValues = new List<object>();
            public bool Success { get; set; } = true;
        }

        private ParametersAttempt AttemptToMakeParameters(
            ParameterInfo[] parameters
        )
        {
            return parameters.Aggregate(
                new ParametersAttempt(),
                (acc, cur) => acc.Success
                    ? TryAddValue(acc, cur)
                    : acc);
        }

        private ParametersAttempt TryAddValue(
            ParametersAttempt acc,
            ParameterInfo cur
        )
        {
            try
            {
                acc.CreatedValues.Add(
                    GetRandomValueFor(cur.ParameterType)
                );
            }
            catch
            {
                acc.Success = false;
            }

            return acc;
        }

        private object GetRandomValueFor(Type t)
        {
            if (Types.PrimitivesAndImmutables.Contains(t))
            {
                return GetRandomValue(t);
            }

            if (_buildLevel >= MaxRandomPropsLevel)
            {
                return GetDefaultValueFor(t);
            }

            var builderType = FindOrCreateDynamicBuilderTypeFor(t);
            var builder = (IGenericBuilder) Activator.CreateInstance(builderType);
            builder.WithBuildLevel(_buildLevel + 1);
            return builder.WithBuildLevel(_buildLevel + 1)
                .GenericWithRandomProps()
                .GenericBuild();
        }

        private object GetDefaultValueFor(Type correctType)
        {
            var method = GetType().GetMethod(nameof(GetDefaultFor), BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
                throw new InvalidOperationException(
                    $"Unable to find static, non-public method {nameof(GetDefaultFor)} on {GetType().PrettyName()}");
            return method
                .MakeGenericMethod(correctType)
                .Invoke(null, null);
        }

#pragma warning disable S1144 // Unused private types or members should be removed
        // ReSharper disable once UnusedMember.Local
        private static T GetDefaultFor<T>()
        {
            return default(T);
        }
#pragma warning restore S1144 // Unused private types or members should be removed


        private static Type FindImplementingTypeFor<TInterface>(IEnumerable<Assembly> assemblies)
        {
            var interfaceType = typeof(TInterface);
            return assemblies.SelectMany(
                    a =>
                    {
                        try
                        {
                            return a.GetExportedTypes();
                        }
                        catch
                        {
                            return new Type[] { };
                        }
                    })
                .FirstOrDefault(
                    t => interfaceType.IsAssignableFrom(t) &&
                         t.IsClass &&
                         !t.IsAbstract &&
                         t.HasDefaultConstructor());
        }

        /// <summary>
        /// Builds the instance of TEntity, applying all builder actions in
        /// order to provide the required entity
        /// </summary>
        /// <returns>An instance of TEntity with all builder actions run on it</returns>
        public virtual TEntity Build()
        {
            _buildTimePropMods.Clear();
            var dynamicCount = 0;
            using (new AutoResetter(() => _currentlyBuilding = true, () => _currentlyBuilding = false))
            {
                var entity = ConstructEntity();
                var actions = new Queue<ActionRef<TEntity>>(DefaultPropMods.Union(_propMods).ToArray());

                while (actions.Count > 0)
                {
                    var action = actions.Dequeue();
                    action(ref entity);
                    while (_buildTimePropMods.Any())
                    {
                        if (++dynamicCount > MaxRandomPropsLevel)
                        {
                            throw new InvalidOperationException(
                                $"{GetType().PrettyName()}::Build -> Too many property modifiers added by property modifiers. Check the sanity of this builder"
                            );
                        }

                        var newActions = _buildTimePropMods.ToArray();
                        _buildTimePropMods.Clear();
                        foreach (var a in newActions)
                        {
                            a(ref entity);
                        }
                    }
                }

                if (_defaultDateTimeKind != DateTimeKind.Unspecified)
                {
                    EntityPropInfo.ForEach(
                        pi =>
                        {
                            if (pi.Type != typeof(DateTime))
                                return;
                            var currentValue = (DateTime) (pi.GetValue(entity));
                            pi.SetValue(entity, currentValue.ToKind(_defaultDateTimeKind));
                        });
                }

                return entity;
            }
        }

        /// <summary>
        /// Sets the default DateTimeKind to be expected on DateTime properties
        /// randomly generated by this builder.
        /// </summary>
        /// <param name="dateTimeKind">Expected DateTimeKind. Setting Unspecified will result
        /// in the default DateTimeKind (Local)</param>
        /// <returns></returns>
        public virtual TBuilder WithDefaultDateTimeKind(DateTimeKind dateTimeKind)
        {
            _defaultDateTimeKind = dateTimeKind;
            return this as TBuilder;
        }
        
        /// <summary>
        /// Randomizes all properties on the instance of TEntity being built.
        /// This method will use methods from RandomValueGen and may generate
        /// new GenericBuilder types for generating more complex properties
        /// </summary>
        /// <returns>The current builder instance</returns>
        public virtual TBuilder WithRandomProps()
        {
            WithProp(SetRandomProps);
            return this as TBuilder;
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly object LockObject = new object();

#pragma warning disable S2743 // Static fields should not be used in generic types
        // ReSharper disable once StaticMemberInGenericType
        private static PropertyOrField[] _entityPropInfoField;

        private static PropertyOrField[] EntityPropInfo
        {
            get
            {
                lock (LockObject)
                {
                    return _entityPropInfoField ??
                           (_entityPropInfoField = GetAllPropertiesAndFieldsOfConstructingType());
                }
            }
        }

        private static PropertyOrField[] GetAllPropertiesAndFieldsOfConstructingType()
        {
            return ConstructingType.GetProperties().Select(pi => new PropertyOrField(pi))
                .Union(ConstructingType.GetFields().Select(fi => new PropertyOrField(fi)))
                .ToArray();
        }

        private static Dictionary<string, ActionRef<TEntity, int>> _randomPropSettersField;

        private static Dictionary<string, ActionRef<TEntity, int>> RandomPropSetters
        {
            get
            {
                var entityProps = EntityPropInfo;
                lock (LockObject)
                {
                    if (_randomPropSettersField != null)
                        return _randomPropSettersField;
                    _randomPropSettersField = new Dictionary<string, ActionRef<TEntity, int>>();
                    entityProps.ForEach(
                        prop =>
                        {
                            SetSetterForType(prop);
                        });

                    return _randomPropSettersField;
                }
            }
        }

        private static readonly Dictionary<Type, Func<PropertyOrField, ActionRef<TEntity, int>>> SimpleTypeSetters =
            new Dictionary<Type, Func<PropertyOrField, ActionRef<TEntity, int>>>()
            {
                {typeof(int), pi => ((ref TEntity e, int i) => pi.SetValue(ref e, GetRandomInt()))},
                {typeof(long), pi => ((ref TEntity e, int i) => pi.SetValue(ref e, GetRandomInt()))},
                {
                    typeof(float),
                    pi => ((ref TEntity e, int i) => pi.SetValue(
                        ref e,
                        Convert.ToSingle(GetRandomDouble(float.MinValue, float.MaxValue), null)))
                },
                {typeof(double), pi => ((ref TEntity e, int i) => pi.SetValue(ref e, GetRandomDouble()))},
                {typeof(decimal), pi => ((ref TEntity e, int i) => pi.SetValue(ref e, GetRandomDecimal()))},
                {typeof(DateTime), pi => ((ref TEntity e, int i) => pi.SetValue(ref e, GetRandomDate()))},
                {typeof(TimeSpan), pi => ((ref TEntity e, int i) => pi.SetValue(ref e, GetRandomTimeSpan()))},
                {typeof(Guid), pi => ((ref TEntity e, int i) => pi.SetValue(ref e, Guid.NewGuid()))},
                {typeof(string), CreateStringPropertyRandomSetterFor},
                {typeof(bool), CreateBooleanPropertyRandomSetterFor},
                {typeof(byte[]), pi => ((ref TEntity e, int i) => pi.SetValue(ref e, GetRandomBytes()))}
            };

#pragma warning restore S2743 // Static fields should not be used in generic types

        private static ActionRef<TEntity, int> CreateStringPropertyRandomSetterFor(PropertyOrField pi)
        {
            if (MayBeEmail(pi))
                return (ref TEntity e, int i) => pi.SetValue(ref e, GetRandomEmail());
            if (MayBeUrl(pi))
                return (ref TEntity e, int i) => pi.SetValue(ref e, GetRandomHttpUrl());
            if (MayBePhone(pi))
                return (ref TEntity e, int i) => pi.SetValue(ref e, GetRandomNumericString());
            return (ref TEntity e, int i) => pi.SetValue(ref e, GetRandomString());
        }

        private static bool MayBePhone(PropertyOrField pi)
        {
            return pi != null &&
                   (pi.Name.ContainsOneOf("phone", "mobile", "fax") ||
                    pi.Name.StartsWithOneOf("tel"));
        }

        private static bool MayBeUrl(PropertyOrField pi)
        {
            return pi != null &&
                   pi.Name.ContainsOneOf("url", "website");
        }

        private static bool MayBeEmail(PropertyOrField pi)
        {
            return pi != null && pi.Name.ToLower().Contains("email");
        }

        private static ActionRef<TEntity, int> CreateBooleanPropertyRandomSetterFor(PropertyOrField pi)
        {
            if (pi.Name == "Enabled")
                return (ref TEntity e, int i) => pi.SetValue(ref e, true);
            return (ref TEntity e, int i) => pi.SetValue(ref e, GetRandomBoolean());
        }

        private static bool IsCollectionType(
            PropertyOrField propertyInfo,
            Type type)
        {
            if (!type.IsCollection())
                return false;
            SetCollectionSetterFor(propertyInfo);
            return true;
        }

        private static void SetCollectionSetterFor(PropertyOrField propertyInfo)
        {
            RandomPropSetters[propertyInfo.Name] = (ref TEntity e, int i) =>
            {
                try
                {
                    var instance = CreateListContainerFor(propertyInfo);
                    if (propertyInfo.Type.IsArray)
                    {
                        instance = ConvertCollectionToArray(instance);
                    }

                    e.SetPropertyValue(propertyInfo.Name, instance);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"Unable to set Collection Setter for {propertyInfo.Name}: {ex.GetType().Name} : {ex.Message}");
                }
            };
        }

        private static object ConvertCollectionToArray(object instance)
        {
            var methodInfo = instance?.GetType().GetMethod("ToArray");
            if (methodInfo == null)
                throw new InvalidOperationException(
                    $"No ToArray() method found on {instance?.GetType()} (or perhaps instance is null)"
                );
            instance = methodInfo.Invoke(instance, new object[] { });
            return instance;
        }

        // ReSharper disable once VirtualMemberNeverOverridden.Global
        /// <summary>
        /// Attempts to fill collections with random data. May fail with stack-overflows
        /// on complex, cyclic-referencing objects. Not enabled by default on random builds,
        /// use with caution -- may lead to a stack overflow with collections which contain
        /// items whose types which have collections with items whose types... and so on. 
        /// </summary>
        /// <returns>The current instance of the builder</returns>
        public virtual TBuilder WithFilledCollections()
        {
            return WithProp(
                (ref TEntity o) =>
                {
                    // TODO: fix potential stack-overflows in cyclic classes by creating proper
                    //  cyclic references instead of gen1 -> gen2 -> genN (boom!)
                    var collectionProperties = EntityPropInfo
                        .Where(pi => IsCollectionType(pi, pi.Type));
                    foreach (var prop in collectionProperties)
                    {
                        FillCollection(o, prop);
                    }
                });
        }

        private void FillCollection(object entity, PropertyOrField pi)
        {
            var container = CreateListContainerFor(pi);
            FillContainer(container);
            if (pi.Type.IsArray)
                container = ConvertCollectionToArray(container);
            pi.SetValue(ref entity, container);
        }

        private static void FillContainer(object collectionInstance)
        {
            if (collectionInstance == null)
                return;
            var collectionType = collectionInstance.GetType();
            var innerType = collectionType.GetGenericArguments()[0];
            var method = collectionType.GetMethod("Add");
            if (method == null)
                throw new InvalidOperationException(
                    $"No 'Add()' method found on {collectionType.PrettyName()}"
                );
            var data = GetRandomCollection(() => GetRandomValue(innerType), 1);
            data.ForEach(item => method.Invoke(collectionInstance, new[] {item}));
        }

        private static object CreateListContainerFor(PropertyOrField propertyInfo)
        {
            var innerType = GetCollectionInnerTypeFor(propertyInfo);
            var listType = typeof(List<>);
            var specificType = listType.MakeGenericType(innerType);
            var instance = Activator.CreateInstance(specificType);
            return instance;
        }

        private static Type GetCollectionInnerTypeFor(PropertyOrField propertyInfo)
        {
            return propertyInfo.Type.IsGenericType
                ? propertyInfo.Type.GetGenericArguments()[0]
                : propertyInfo.Type.GetElementType();
        }

        private static void SetSetterForType(
            PropertyOrField prop,
            Type propertyType = null)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var setter in PropertySetterStrategies)
            {
                if (setter(prop, propertyType ?? prop.Type))
                    return;
            }
        }

        // whilst the collection itself does not reference a type parameter,
        //  HaveSetSimpleSetterFor does, so this collection must be per-generic-definition
        // ReSharper disable once StaticMemberInGenericType

        // SUPPRESSED ON PURPOSE (:
#pragma warning disable S2743 // Static fields should not be used in generic types
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Func<PropertyOrField, Type, bool>[] PropertySetterStrategies =
#pragma warning restore S2743 // Static fields should not be used in generic types
        {
            IsNotWritable,
            HaveSetSimpleSetterFor,
            IsEnumType,
            IsCollectionType,
            HaveSetNullableTypeSetterFor,
            SetupBuilderSetterFor
        };

        private static bool IsEnumType(
            PropertyOrField prop,
            Type propertyType)
        {
            if (!propertyType.IsEnum)
                return false;
            RandomPropSetters[prop.Name] = (ref TEntity entity, int idx) =>
            {
                prop.SetValue(ref entity, GetRandomEnum(propertyType));
            };
            return true;
        }

        private static bool HaveSetSimpleSetterFor(
            PropertyOrField prop,
            Type propertyType)
        {
            if (!SimpleTypeSetters.TryGetValue(propertyType, out var setterGenerator))
                return false;
            RandomPropSetters[prop.Name] = setterGenerator(prop);
            return true;
        }


        // TODO: delay this check until we have an instance: the generic builder may
        //  be created against a type which is implemented / overridden by another which
        //  provides write access on the property. I'm specifically thinking about
        //  builders doing funky stuff with interfaces...
#pragma warning disable S1172 // Unused method parameters should be removed
        private static bool IsNotWritable(
            PropertyOrField prop,
            Type propertyType)
#pragma warning restore S1172 // Unused method parameters should be removed
        {
            if (prop?.CanWrite ?? true)
                return false;
            // ReSharper disable once ConstantConditionalAccessQualifier
            Trace.WriteLine($"{prop?.DeclaringType?.Name}.{prop.Name} is not writable");
            return true;
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == NullableGeneric;
        }


        private static bool HaveSetNullableTypeSetterFor(
            PropertyOrField prop,
            Type propertyType)
        {
            if (!IsNullableType(propertyType))
                return false;
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            SetSetterForType(prop, underlyingType);
            return true;
        }

        private static bool SetupBuilderSetterFor(
            PropertyOrField prop,
            Type propertyType)
        {
            // FIXME: why am I sending through the type twice? I know I had a reason :/
            var builderType = TryFindUserBuilderFor(prop.Type) ??
                              FindOrCreateDynamicBuilderTypeFor(prop.Type);
            if (builderType == null)
                return false;
            RandomPropSetters[prop.Name] = (ref TEntity e, int depth) =>
            {
                if (TraversedTooManyTurtles(depth))
                    return;
                var dynamicBuilder = Activator.CreateInstance(builderType) as IGenericBuilder;
                if (dynamicBuilder == null)
                    return;
                dynamicBuilder
                    .WithBuildLevel(depth)
                    .GenericWithRandomProps();
                prop.SetValue(
                    ref e,
                    dynamicBuilder.GenericBuild());
            };
            return true;
        }

        private static bool TraversedTooManyTurtles(int i)
        {
            if (i > MaxRandomPropsLevel)
                return true;
            var stackTrace = new StackTrace();
            var frames = stackTrace.GetFrames();
            return HaveReenteredOwnRandomPropsTooManyTimesFor(frames);
        }

        private static bool HaveReenteredOwnRandomPropsTooManyTimesFor(StackFrame[] frames)
        {
            var level = frames.Aggregate(
                0,
                (acc, cur) =>
                {
                    var thisMethod = cur.GetMethod();
                    var thisType = thisMethod.DeclaringType;
                    if (thisType != null &&
                        thisType.IsGenericType &&
                        GenericBuilderBaseType.IsAssignableFrom(thisType) &&
                        thisMethod.Name == "SetRandomProps")
                    {
                        return acc + 1;
                    }

                    return acc;
                });
            return level >= MaxRandomPropsLevel;
        }

        private static Type FindOrCreateDynamicBuilderTypeFor(Type type)
        {
            if (DynamicBuilders.TryGetValue(type, out var builderType))
                return builderType;
            try
            {
                return GenerateDynamicBuilderFor(type);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(
                    $"Error defining dynamic builder for property of type: {type.Name}: " +
                    ex.Message);
                return null;
            }
        }

        private static Type TryFindUserBuilderFor(Type propertyType)
        {
            if (!UserBuilders.TryGetValue(propertyType, out var builderType))
            {
                var existingBuilder = GenericBuilderLocator.TryFindExistingBuilderFor(propertyType);
                if (existingBuilder == null)
                    return null;
                UserBuilders[propertyType] = existingBuilder;
                builderType = existingBuilder;
            }

            return builderType;
        }

        private static Type GenerateDynamicBuilderFor(Type type)
        {
            return ReuseOrGenerateDynamicBuilderFor(type);
        }

        private int _buildLevel;

        private void SetRandomProps(ref TEntity entity)
        {
            foreach (var prop in EntityPropInfo)
            {
                try
                {
                    var setter = GetRandomPropSetterFor(prop);
                    setter?.Invoke(ref entity, _buildLevel + 1);
                }
                catch (Exception ex)
                {
                    RandomPropSetters[prop.Name] = null;
                    Trace.WriteLine(
                        $"Unable to set random prop: {prop.DeclaringType?.Name}.{prop.Name} ({prop.Type.Name}) {ex.Message}"
                    );
                }
            }
        }

        private ActionRef<TEntity, int> GetRandomPropSetterFor(PropertyOrField prop)
        {
            if (RandomPropSetters.TryGetValue(prop.Name, out var result))
                return result;
            Trace.WriteLine(
                $"No random property setter available for {prop.DeclaringType}.{prop.Name} (perhaps make a dev request?)");
            RandomPropSetters[prop.Name] = null;
            return null;
        }
    }
}