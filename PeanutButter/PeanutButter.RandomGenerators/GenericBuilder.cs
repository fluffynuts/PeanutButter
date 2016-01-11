/* Generic Builder base class
 * author: Davyd McColl (davydm@gmail.com)
 * license: BSD
 * */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PeanutButter.TestUtils.Generic;


namespace PeanutButter.RandomGenerators
{
    public interface IGenericBuilder
    {
        IGenericBuilder WithBuildLevel(int level);
        IGenericBuilder GenericWithRandomProps();
        object GenericBuild();
    }
    public class GenericBuilder<TConcrete, TEntity> : IGenericBuilder, IBuilder<TEntity> where TConcrete: GenericBuilder<TConcrete, TEntity>, new()
    {
        public static int MAX_RANDOM_PROPS_LEVEL = 10;
        private static List<Action<TEntity>> _defaultPropMods = new List<Action<TEntity>>();
        private List<Action<TEntity>> _propMods = new List<Action<TEntity>>();

        public static TConcrete Create()
        {
            return new TConcrete();
        }

        public IGenericBuilder GenericWithRandomProps()
        {
            return this.WithRandomProps() as IGenericBuilder;
        }

        public IGenericBuilder WithBuildLevel(int level)
        {
            this._buildLevel = level;
            return this;
        }

        public object GenericBuild()
        {
            return this.Build();
        }

        public static TEntity BuildDefault()
        {
            return Create().Build();
        }

        public static TEntity BuildRandom()
        {
            return Create().WithRandomProps().Build();
        }

        public static void WithDefaultProp(Action<TEntity> action)
        {
            _defaultPropMods.Add(action);
        }

        public TConcrete WithProp(Action<TEntity> action)
        {
            this._propMods.Add(action);
            return this as TConcrete;
        }

        public virtual TEntity ConstructEntity()
        {
            return (TEntity)Activator.CreateInstance(typeof (TEntity));
        }

        public virtual TEntity Build()
        {
            var entity = ConstructEntity();
            foreach (var action in _defaultPropMods.Union(this._propMods))
            {
                action(entity);
            }
            return entity;
        }

        public virtual TConcrete WithRandomProps()
        {
            this.WithProp(e => this.SetRandomProps(e));
            return this as TConcrete;
        }

        private static Object _lockObject = new Object();
        private static PropertyInfo[] _EntityPropInfoField;
        private static PropertyInfo[] _EntityPropInfo
        {
            get
            {
                lock (_lockObject)
                {
                    if (_EntityPropInfoField == null)
                    {
                        _EntityPropInfoField = typeof(TEntity).GetProperties();
                    }
                    return _EntityPropInfoField;
                }
            }
        }

        private static Dictionary<Type, Type> _dynamicBuilders = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> _userBuilders = new Dictionary<Type, Type>(); 
        private static Dictionary<string, Action<TEntity, int>> _randomPropSettersField;
        private static Dictionary<string, Action<TEntity, int>> _randomPropSetters
        {
            get
            {
                var entityProps = _EntityPropInfo;
                lock (_lockObject)
                {
                    if (_randomPropSettersField == null)
                    {
                        _randomPropSettersField = new Dictionary<string, Action<TEntity, int>>();
                        foreach (var prop in entityProps)
                        {
                            SetSetterForType(prop);
                        }
                    }
                    return _randomPropSettersField;
                }
            }
        }

        private static Dictionary<Type, Func<PropertyInfo, Action<TEntity, int>>> _setters =
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
                   (pi.Name.ToLower().Contains("phone") ||
                    pi.Name.ToLower().StartsWith("tel") ||
                    pi.Name.ToLower().Contains("mobile") ||
                    pi.Name.ToLower().Contains("fax"));
        }

        private static bool MayBeUrl(PropertyInfo pi)
        {
            return pi != null &&
                   (pi.Name.ToLower().Contains("url") ||
                    pi.Name.ToLower().Contains("website"));
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

        private static Type _nullableGeneric = typeof (Nullable<>);
        private static Type _collectionGeneric = typeof (ICollection<>);

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType &&
                    type.GetGenericTypeDefinition() == _nullableGeneric;
        }

        private static bool IsCollectionType(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == _collectionGeneric;
        }

        private static void SetSetterForType(PropertyInfo prop, Type propertyType = null)
        {
            if (!prop.CanWrite) return;
            propertyType = propertyType ?? prop.PropertyType;
            if (IsCollectionType(propertyType))
                return;

            Func<PropertyInfo, Action<TEntity, int>> setterGenerator;
            if (_setters.TryGetValue(propertyType, out setterGenerator))
                _randomPropSetters[prop.Name] = setterGenerator(prop);
            else if (IsNullableType(propertyType))
            {
                var underlyingType = Nullable.GetUnderlyingType(propertyType);
                SetSetterForType(prop, underlyingType);
            }
            else
            {
                var builderType = TryFindUserBuilderFor(prop.PropertyType);
                if (builderType == null)
                    builderType = FindOrCreateDynamicBuilderFor(prop);
                if (prop.CanWrite)
                {
                    _randomPropSettersField[prop.Name] = (e, i) =>
                    {
                        if (i > MAX_RANDOM_PROPS_LEVEL) return;
                        var dynamicBuilder = Activator.CreateInstance(builderType) as IGenericBuilder;
                        prop.SetValue(e, dynamicBuilder.GenericWithRandomProps().WithBuildLevel(i).GenericBuild(), null);
                    };
                }
            }
        }

        private static Type FindOrCreateDynamicBuilderFor(PropertyInfo propInfo)
        {
            Type builderType = null;
            if (_dynamicBuilders.TryGetValue(propInfo.PropertyType, out builderType))
                return builderType;
            return GenerateDynamicBuilderFor(propInfo);
        }

        private static Type TryFindUserBuilderFor(Type propertyType)
        {
            Type builderType = null;
            if (!_userBuilders.TryGetValue(propertyType, out builderType))
            {
                var existingBuilder = TryFindExistingBuilderFor(propertyType);
                if (existingBuilder != null)
                {
                    _userBuilders[propertyType] = builderType;
                    builderType = existingBuilder;
                }
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
                    .Where(t => t.IsBuilderFor(propertyType));
                if (!types.Any())
                    return null;
                if (types.Count() == 1)
                    return types.First();
                return FindClosestNamespaceMatchFor(propertyType, types);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error whilst searching for user builder for type '" + propertyType.PrettyName() + "' in all loaded assemblies: " + ex.Message);
                return null;
            }
        }

        private static Type FindClosestNamespaceMatchFor(Type propertyType, IEnumerable<Type> types)
        {
            var seekNamespace = propertyType.Namespace.Split('.');
            return types.Aggregate((Type) null, (acc, cur) =>
            {
                if (acc == null)
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
            var moduleName = String.Join("_", new[] { "DynamicEntityBuilders", prop.PropertyType.Name });
            var modBuilder = _dynamicAssemblyBuilder.DefineDynamicModule(moduleName);

            var typeBuilder = modBuilder.DefineType(prop.PropertyType + "Builder", TypeAttributes.Public | TypeAttributes.Class);
            // Typebuilder is a sub class of Type
            typeBuilder.SetParent(t.MakeGenericType(typeBuilder, prop.PropertyType));
            var dynamicBuilderType = typeBuilder.CreateType();
            _dynamicBuilders[prop.PropertyType] = dynamicBuilderType;
            return dynamicBuilderType;
        }

        // TODO: expose the generic builder generator for use from consumers
        //public static Type GenerateDynamicBuilder<TEntity>()
        //{
        //    var t = typeof (GenericBuilder<,>);
        //    var entityType = typeof (TEntity);
        //    var moduleName = String.Join("_", new[] {"DynamicEntityBuilders", entityType.Name});
        //    var modBuilder = _dynamicAssemblyBuilder.DefineDynamicModule(moduleName);

        //}


        private static Object _dynamicAssemblyLock = new object();
        private static AssemblyBuilder _dynamicAssemblyBuilderField;
        private int _buildLevel;

        private static AssemblyBuilder _dynamicAssemblyBuilder
        {
            get
            {
                lock (_dynamicAssemblyLock)
                {
                    if (_dynamicAssemblyBuilderField == null)
                    {
                        _dynamicAssemblyBuilderField = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("DynamicEntityBuilders"), 
                            AssemblyBuilderAccess.RunAndSave);
                    }
                    return _dynamicAssemblyBuilderField;
                }
            }
        }
        private void SetRandomProps(TEntity entity)
        {
            foreach (var prop in _EntityPropInfo)
            {
                try
                {
                    _randomPropSetters[prop.Name](entity, this._buildLevel + 1);
                }
                catch (Exception ex)
                {
                    var foo = ex;
                    //throw new Exception("Missing propSetter for: " + prop.Name);
                }
            }
        }
    }
}
