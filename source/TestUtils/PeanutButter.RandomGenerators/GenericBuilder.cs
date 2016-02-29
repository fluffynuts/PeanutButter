/* Generic Builder base class
 * author: Davyd McColl (davydm@gmail.com)
 * license: BSD
 * */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;


namespace PeanutButter.RandomGenerators
{
    public interface IGenericBuilder
    {
        IGenericBuilder WithBuildLevel(int level);
        IGenericBuilder GenericWithRandomProps();
        object GenericBuild();
    }

    public class GenericBuilder<TBuilder, TEntity> : GenericBuilderBase, IGenericBuilder, IBuilder<TEntity> where TBuilder: GenericBuilder<TBuilder, TEntity>//, new()
    {
        private static readonly List<Action<TEntity>> DefaultPropMods = new List<Action<TEntity>>();
        private readonly List<Action<TEntity>> _propMods = new List<Action<TEntity>>();

        public static TBuilder Create()
        {
            return Activator.CreateInstance<TBuilder>();
        }

        public IGenericBuilder GenericWithRandomProps()
        {
            return _buildLevel > MaxRandomPropsLevel 
                        ? this 
                        : WithRandomProps();
        }

        public IGenericBuilder WithBuildLevel(int level)
        {
            _buildLevel = level;
            return this;
        }

        public object GenericBuild()
        {
            return Build();
        }

        public static TEntity BuildDefault()
        {
            return Create().Build();
        }

        public static TEntity BuildRandom()
        {
            return Create().WithRandomProps().Build();
        }

        // ReSharper disable once UnusedMember.Global
        public static void WithDefaultProp(Action<TEntity> action)
        {
            DefaultPropMods.Add(action);
        }

        public TBuilder WithProp(Action<TEntity> action)
        {
            _propMods.Add(action);
            return this as TBuilder;
        }

        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once VirtualMemberNeverOverriden.Global
        public virtual TEntity ConstructEntity()
        {
            try
            {
                return Activator.CreateInstance<TEntity>();
            }
            catch (Exception)
            {
                throw new GenericBuilderInstanceCreationException(GetType(), typeof (TBuilder));
            }
        }

        public virtual TEntity Build()
        {
            var entity = ConstructEntity();
            foreach (var action in DefaultPropMods.Union(_propMods))
            {
                action(entity);
            }
            return entity;
        }

        public virtual TBuilder WithRandomProps()
        {
            WithProp(SetRandomProps);
            return this as TBuilder;
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly object LockObject = new object();
        // ReSharper disable once StaticMemberInGenericType
        private static PropertyInfo[] _entityPropInfoField;
        private static PropertyInfo[] EntityPropInfo
        {
            get
            {
                lock (LockObject)
                {
                    return _entityPropInfoField ?? (_entityPropInfoField = typeof (TEntity).GetProperties());
                }
            }
        }

        private static Dictionary<string, Action<TEntity, int>> _randomPropSettersField;
        private static Dictionary<string, Action<TEntity, int>> RandomPropSetters
        {
            get
            {
                var entityProps = EntityPropInfo;
                lock (LockObject)
                {
                    if (_randomPropSettersField != null) return _randomPropSettersField;
                    _randomPropSettersField = new Dictionary<string, Action<TEntity, int>>();
                    foreach (var prop in entityProps)
                    {
                        SetSetterForType(prop);
                    }
                    return _randomPropSettersField;
                }
            }
        }

        private static readonly Dictionary<Type, Func<PropertyInfo, Action<TEntity, int>>> SimpleTypeSetters =
            new Dictionary<Type, Func<PropertyInfo, Action<TEntity, int>>>()
            {
                { typeof (int), pi => ((e, i) => pi.SetValue(e, RandomValueGen.GetRandomInt(), null))},
                { typeof (long), pi => ((e, i) => pi.SetValue(e, RandomValueGen.GetRandomInt(), null))},
                { typeof (float), pi => ((e, i) => pi.SetValue(e, Convert.ToSingle(RandomValueGen.GetRandomDouble(float.MinValue, float.MaxValue), null))) },
                { typeof (double), pi => ((e, i) => pi.SetValue(e, RandomValueGen.GetRandomDouble(), null))},
                { typeof (decimal), pi => ((e, i) => pi.SetValue(e, RandomValueGen.GetRandomDecimal(), null))},
                { typeof(DateTime), pi => ((e, i) => pi.SetValue(e, RandomValueGen.GetRandomDate(), null))},
                { typeof(Guid), pi => ((e, i) => pi.SetValue(e, Guid.NewGuid(), null)) },
                { typeof(string), CreateStringPropertyRandomSetterFor },
                { typeof(bool), CreateBooleanPropertyRandomSetterFor },
                { typeof(byte[]), pi => ((e, i) => pi.SetValue(e, RandomValueGen.GetRandomBytes(), null)) }
            };

        private static Action<TEntity, int> CreateStringPropertyRandomSetterFor(PropertyInfo pi)
        {
            if (MayBeEmail(pi))
                return (e, i) => pi.SetValue(e, RandomValueGen.GetRandomEmail(), null);
            if (MayBeUrl(pi))
                return (e, i) => pi.SetValue(e, RandomValueGen.GetRandomHttpUrl(), null);
            if (MayBePhone(pi))
                return (e, i) => pi.SetValue(e, RandomValueGen.GetRandomNumericString(), null);
            return (e, i) => pi.SetValue(e, RandomValueGen.GetRandomString(), null);
        }

        private static bool MayBePhone(PropertyInfo pi)
        {
            return pi != null &&
                   (pi.Name.ContainsOneOf("phone", "mobile", "fax") ||
                    pi.Name.StartsWithOneOf("tel"));
        }

        private static bool MayBeUrl(PropertyInfo pi)
        {
            return pi != null &&
                   pi.Name.ContainsOneOf("url", "website");
        }

        private static bool MayBeEmail(PropertyInfo pi)
        {
            return pi != null && pi.Name.ToLower().Contains("email");
        }

        private static Action<TEntity, int> CreateBooleanPropertyRandomSetterFor(PropertyInfo pi)
        {
            if (pi.Name == "Enabled")
                return (e, i) => pi.SetValue(e, true, null);
            return (e, i) => pi.SetValue(e, RandomValueGen.GetRandomBoolean(), null);
        }

        private static bool IsCollectionType(PropertyInfo propertyInfo, Type type)
        {
            //return isCollectionType;
            // TODO: figure out the stack overflow introduced by cyclic references when enabling this
            //  code below
            if (type.IsNotCollection())
                return false;
            SetCollectionSetterFor(propertyInfo, type);
            return true;
        }

        private static void SetCollectionSetterFor(PropertyInfo propertyInfo, Type type)
        {
            RandomPropSetters[propertyInfo.Name] = (e, i) => 
            {
                try
                {
                    var instance = CreateListContainerFor(propertyInfo);
                    if (propertyInfo.PropertyType.IsArray)
                    {
                        instance = ConvertCollectionToArray(instance);
                    }
                    e.SetPropertyValue(propertyInfo.Name, instance);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to set Collection Setter for {propertyInfo.Name}: {ex.GetType().Name} : {ex.Message}");
                }
            };
        }

        private static object ConvertCollectionToArray(object instance)
        {
            var methodInfo = instance.GetType().GetMethod("ToArray");
            instance = methodInfo.Invoke(instance, new object[] {});
            return instance;
        }

        public virtual TBuilder WithFilledCollections()
        {
            return WithProp(o =>
            {
                // TODO: figure out why filling collections can cause an SO with cyclic classes
                var collectionProperties = typeof(TEntity).GetProperties()
                                                .Where(pi => IsCollectionType(pi, pi.PropertyType));
                collectionProperties.ForEach(prop => FillCollection(o, prop));
            });
        }

        private void FillCollection(object entity, PropertyInfo pi)
        {
            var container = CreateListContainerFor(pi);
            FillContainer(container);
            if (pi.PropertyType.IsArray)
                container = ConvertCollectionToArray(container);
            pi.SetValue(entity, container);
        }

        private static void FillContainer(object collectionInstance)
        {
            var innerType = collectionInstance.GetType().GetGenericArguments()[0];
            var method = collectionInstance.GetType().GetMethod("Add");
            var data = RandomValueGen.GetRandomCollection(() => RandomValueGen.GetRandomValue(innerType), 1);
            data.ForEach(item => method.Invoke(collectionInstance, new object[] { item }));
        }

        private static object CreateListContainerFor(PropertyInfo propertyInfo)
        {
            var innerType = GetCollectionInnerTypeFor(propertyInfo);
            var listType = typeof (List<>);
            var specificType = listType.MakeGenericType(innerType);
            var instance = Activator.CreateInstance(specificType);
            return instance;
        }

        private static Type GetCollectionInnerTypeFor(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.IsGenericType
                    ? propertyInfo.PropertyType.GetGenericArguments()[0]
                    : propertyInfo.PropertyType.GetElementType();
        }

        private static void SetSetterForType(PropertyInfo prop, Type propertyType = null)
        {
            PropertySetterStrategies.Aggregate(false,
                (accumulator, currentFunc) => accumulator || currentFunc(prop, propertyType ?? prop.PropertyType));
        }

        // whilst the collection itself does not reference a type parameter,
        //  HaveSetSimpleSetterFor does, so this collection must be per-generic-definition
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Func<PropertyInfo, Type, bool>[] PropertySetterStrategies =
        {
            IsNotWritable,
            HaveSetSimpleSetterFor,
            IsEnumType,
            IsCollectionType,
            HaveSetNullableTypeSetterFor,
            SetupBuilderSetterFor
        };

        private static bool IsEnumType(PropertyInfo prop, Type propertyType)
        {
            if (!propertyType.IsEnum)
                return false;
            RandomPropSetters[prop.Name] = (entity, idx) =>
            {
                prop.SetValue(entity, RandomValueGen.GetRandomEnum(propertyType));
            };
            return true;
        }

        private static bool HaveSetSimpleSetterFor(PropertyInfo prop, Type propertyType)
        {
            Func<PropertyInfo, Action<TEntity, int>> setterGenerator;
            if (!SimpleTypeSetters.TryGetValue(propertyType, out setterGenerator))
                return false;
            RandomPropSetters[prop.Name] = setterGenerator(prop);
            return true;
        }

        private static bool SetterResultFor(Func<bool> finalFunc, string failMessage)
        {
            var result = finalFunc();
            if (!result)
                Trace.WriteLine(failMessage);
            return result;
        }

        // TODO: delay this check until we have an instance: the generic builder may
        //  be created against a type which is implemented / overridden by another which
        //  provides write access on the property. I'm specifically thinking about
        //  builders doing funky stuff with interfaces...
        private static bool IsNotWritable(PropertyInfo prop, Type propertyType)
        {
            return SetterResultFor(() => !prop.CanWrite, $"{prop.Name} is not writable");
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType &&
                    type.GetGenericTypeDefinition() == NullableGeneric;
        }


        private static bool HaveSetNullableTypeSetterFor(PropertyInfo prop, Type propertyType)
        {
            if (!IsNullableType(propertyType))
                return false;
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            SetSetterForType(prop, underlyingType);
            return true;
        }

        private static bool SetupBuilderSetterFor(PropertyInfo prop, Type propertyType)
        {
            var builderType = TryFindUserBuilderFor(prop.PropertyType)
                              ?? FindOrCreateDynamicBuilderFor(prop);
            if (builderType == null)
                return false;
            RandomPropSetters[prop.Name] = (e, i) =>
            {
                if (TraversedTooManyTurtles(i)) return;
                var dynamicBuilder = Activator.CreateInstance(builderType) as IGenericBuilder;
                if (dynamicBuilder == null)
                    return;
                prop.SetValue(e, dynamicBuilder.WithBuildLevel(i).GenericWithRandomProps().GenericBuild(), null);
            };
            return true;
        }

        private static bool TraversedTooManyTurtles(int i)
        {
            var stackTrace = new StackTrace();
            var frames = stackTrace.GetFrames();
            if (HaveReenteredOwnRandomPropsTooManyTimesFor(frames))
                return true;
            return i > MaxRandomPropsLevel;
        }

        private static bool HaveReenteredOwnRandomPropsTooManyTimesFor(StackFrame[] frames)
        {
            var level = frames.Aggregate(0, (acc, cur) =>
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

        private static Type FindOrCreateDynamicBuilderFor(PropertyInfo propInfo)
        {
            Type builderType;
            if (DynamicBuilders.TryGetValue(propInfo.PropertyType, out builderType))
                return builderType;
            try
            {
                return GenerateDynamicBuilderFor(propInfo);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error defining dynamic builder for property of type: {propInfo.PropertyType.Name}: " + ex.Message);
                return null;
            }
        }

        private static Type GenerateDynamicBuilderFor(PropertyInfo prop)
        {
            return FindOrGenerateDynamicBuilderFor(prop.PropertyType);
        }

        private static Type TryFindUserBuilderFor(Type propertyType)
        {
            Type builderType;
            if (!UserBuilders.TryGetValue(propertyType, out builderType))
            {
                var existingBuilder = GenericBuilderLocator.TryFindExistingBuilderFor(propertyType);
                if (existingBuilder == null) return null;
                UserBuilders[propertyType] = existingBuilder;
                builderType = existingBuilder;
            }
            return builderType;
        }

        private int _buildLevel;
        private void SetRandomProps(TEntity entity)
        {
            foreach (var prop in EntityPropInfo)
            {
                try
                {
                    RandomPropSetters[prop.Name](entity, _buildLevel + 1);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Unable to set random prop: {ex.Message}");
                }
            }
        }
    }

}
