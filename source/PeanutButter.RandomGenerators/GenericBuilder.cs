/* Generic Builder base class
 * author: Davyd McColl (davydm@gmail.com)
 * license: BSD
 * */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

    public class GenericBuilder<TConcrete, TEntity> : GenericBuilderStaticHouser, IGenericBuilder, IBuilder<TEntity> where TConcrete: GenericBuilder<TConcrete, TEntity>//, new()
    {
        private static readonly List<Action<TEntity>> DefaultPropMods = new List<Action<TEntity>>();
        private readonly List<Action<TEntity>> _propMods = new List<Action<TEntity>>();

        public static TConcrete Create()
        {
            return Activator.CreateInstance<TConcrete>();
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

        public TConcrete WithProp(Action<TEntity> action)
        {
            _propMods.Add(action);
            return this as TConcrete;
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
                throw new GenericBuilderInstanceCreationException(GetType(), typeof (TConcrete));
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

        public virtual TConcrete WithRandomProps()
        {
            WithProp(SetRandomProps);
            return this as TConcrete;
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
            return type.IsArray ||
                   (type.IsGenericType &&
                    CollectionGenerics.Contains(type.GetGenericTypeDefinition()));
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
            IsCollectionType,
            HaveSetNullableTypeSetterFor,
            SetupBuilderSetterFor
        };

        private static bool HaveSetSimpleSetterFor(PropertyInfo prop, Type propertyType)
        {
            Func<PropertyInfo, Action<TEntity, int>> setterGenerator;
            if (!SimpleTypeSetters.TryGetValue(propertyType, out setterGenerator))
                return false;
            RandomPropSetters[prop.Name] = setterGenerator(prop);
            return true;
        }

        private static bool IsNotWritable(PropertyInfo prop, Type propertyType)
        {
            return !prop.CanWrite;
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
            _randomPropSettersField[prop.Name] = (e, i) =>
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
                Debug.WriteLine($"Error defining dynamic builder for property of type: {propInfo.PropertyType.Name}: " + ex.Message);
                return null;
            }
        }

        private static Type TryFindUserBuilderFor(Type propertyType)
        {
            Type builderType;
            if (!UserBuilders.TryGetValue(propertyType, out builderType))
            {
                var existingBuilder = TryFindExistingBuilderFor(propertyType);
                if (existingBuilder == null) return null;
                UserBuilders[propertyType] = existingBuilder;
                builderType = existingBuilder;
            }
            return builderType;
        }

        private static Type TryFindExistingBuilderFor(Type propertyType)
        {
            // TODO: scour other assemblies for a possible builder (FUTURE, as required)
            return TryFindBuilderInCurrentAssemblyFor(propertyType)
                   ?? TryFindBuilderInAnyOtherAssemblyInAppDomainFor(propertyType);
        }

        private static Type[] TryGetExportedTypesFrom(Assembly asm)
        {
            try
            {
                return asm.GetExportedTypes();
            }
            catch
            {
                return new Type[] {};
            }
        }

        private static Type TryFindBuilderInAnyOtherAssemblyInAppDomainFor(Type propertyType)
        {
            try
            {
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a != propertyType.Assembly && !a.IsDynamic)
                    .SelectMany(TryGetExportedTypesFrom)
                    .Where(t => t.IsBuilderFor(propertyType))
                    .ToArray();
                if (!types.Any())
                    return null;
                return types.Length == 1 
                        ? types.First() 
                        : FindClosestNamespaceMatchFor(propertyType, types);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error whilst searching for user builder for type '" + propertyType.PrettyName() + "' in all loaded assemblies: " + ex.Message);
                return null;
            }
        }

        private static Type FindClosestNamespaceMatchFor(Type propertyType, IEnumerable<Type> types)
        {
            if (propertyType?.Namespace == null)    // R# is convinced this might happen :/
                return null;
            var seekNamespace = propertyType.Namespace.Split('.');
            return types.Aggregate((Type) null, (acc, cur) =>
            {
                if (acc?.Namespace == null || cur.Namespace == null)
                    return cur;
                var accParts = acc.Namespace.Split('.');
                var curParts = cur.Namespace.Split('.');
                var accMatchIndex = seekNamespace.MatchIndexFor(accParts);
                var curMatchIndex = seekNamespace.MatchIndexFor(curParts);
                return accMatchIndex < curMatchIndex ? acc : cur;
            });
        }

        private static Type TryFindBuilderInCurrentAssemblyFor(Type propType)
        {
            try
            {
                return propType.Assembly.GetTypes()
                    .FirstOrDefault(t => t.IsBuilderFor(propType));
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error whilst searching for user builder for type '" + propType.PrettyName() + "' in type's assembly: " + ex.Message);
                return null;
            }
        }

        private static Type GenerateDynamicBuilderFor(PropertyInfo prop)
        {
            var t = typeof(GenericBuilder<,>);
            var moduleName = string.Join("_", "DynamicEntityBuilders", prop.PropertyType.Name);
            var modBuilder = DynamicAssemblyBuilder.DefineDynamicModule(moduleName);

            var typeBuilder = modBuilder.DefineType(prop.PropertyType + "Builder", TypeAttributes.Public | TypeAttributes.Class);
            // Typebuilder is a sub class of Type
            typeBuilder.SetParent(t.MakeGenericType(typeBuilder, prop.PropertyType));
            var dynamicBuilderType = typeBuilder.CreateType();
            DynamicBuilders[prop.PropertyType] = dynamicBuilderType;
            return dynamicBuilderType;
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
                    Debug.WriteLine($"Unable to set random prop: {ex.Message}");
                }
            }
        }
    }
}
