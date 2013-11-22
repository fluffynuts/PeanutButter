/* Generic Builder base class
 * author: Davyd McColl (davydm@gmail.com)
 * license: BSD
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;


namespace PeanutButter.RandomGenerators
{
    public interface IGenericBuilder
    {
        IGenericBuilder GenericWithRandomProps();
        object GenericBuild();
    }
    public class GenericBuilder<TConcreteBuilder, TEntity> : IGenericBuilder, IBuilder<TEntity> where TConcreteBuilder: GenericBuilder<TConcreteBuilder, TEntity>, new()
                                                                                  where TEntity: new()
    {
        private static List<Action<TEntity>> _defaultPropMods = new List<Action<TEntity>>();
        private List<Action<TEntity>> _propMods = new List<Action<TEntity>>();

        public static TConcreteBuilder Create()
        {
            return new TConcreteBuilder();
        }

        public IGenericBuilder GenericWithRandomProps()
        {
            return this.WithRandomProps() as IGenericBuilder;
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

        public TConcreteBuilder WithProp(Action<TEntity> action)
        {
            this._propMods.Add(action);
            return this as TConcreteBuilder;
        }

        public virtual TEntity Build()
        {
            var entity = new TEntity();
            foreach (var action in _defaultPropMods.Union(this._propMods))
            {
                action(entity);
            }
            return entity;
        }

        public virtual TConcreteBuilder WithRandomProps()
        {
            this.WithProp(e => this.SetRandomProps(e));
            return this as TConcreteBuilder;
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
        private static Dictionary<string, Action<TEntity>> _randomPropSettersField;
        private static Dictionary<string, Action<TEntity>> _randomPropSetters
        {
            get
            {
                var entityProps = _EntityPropInfo;
                lock (_lockObject)
                {
                    if (_randomPropSettersField == null)
                    {
                        _randomPropSettersField = new Dictionary<string, Action<TEntity>>();
                        foreach (var prop in entityProps)
                        {
                            SetSetterForType(prop);
                        }
                    }
                    return _randomPropSettersField;
                }
            }
        }

        private static void SetSetterForType(PropertyInfo prop, string typeName = null)
        {
            typeName = typeName ?? prop.PropertyType.Name;
            switch (typeName.ToLower())
            {
                case "boolean":
                    _randomPropSettersField[prop.Name] = (e) => { prop.SetValue(e, RandomValueGen.GetRandomBoolean()); };
                    break;
                case "int64":
                case "long":
                    _randomPropSettersField[prop.Name] = (e) => { prop.SetValue(e, (Int64)RandomValueGen.GetRandomInt()); };
                    break;
                case "int32":
                case "int":
                    _randomPropSettersField[prop.Name] = (e) => { prop.SetValue(e, RandomValueGen.GetRandomInt()); };
                    break;
                case "float":
                    _randomPropSettersField[prop.Name] = (e) => { prop.SetValue(e, (float)RandomValueGen.GetRandomDouble()); };
                    break;
                case "double":
                    _randomPropSettersField[prop.Name] = (e) => { prop.SetValue(e, RandomValueGen.GetRandomDouble()); };
                    break;
                case "decimal":
                    _randomPropSettersField[prop.Name] = (e) => { prop.SetValue(e, RandomValueGen.GetRandomDecimal()); };
                    break;
                case "datetime":
                    _randomPropSettersField[prop.Name] = (e) => { prop.SetValue(e, RandomValueGen.GetRandomDate()); };
                    break;
                case "guid":
                    _randomPropSettersField[prop.Name] = (e) => { prop.SetValue(e, Guid.NewGuid()); };
                    break;
                case "string":
                    _randomPropSettersField[prop.Name] = (e) => { prop.SetValue(e, RandomValueGen.GetRandomString()); };
                    break;
                case "nullable`1":
                    var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
                    SetSetterForType(prop, underlyingType.Name);
                    break;
                case "icollection`1":
                    break;
                default:
                    if (!_dynamicBuilders.Keys.Any(k => k == prop.PropertyType))
                        GenerateDynamicBuilder(prop);
                    if (_dynamicBuilders.Keys.Any(k => k == prop.PropertyType))
                    {
                        _randomPropSettersField[prop.Name] = (e) =>
                        {
                            var dynamicBuilder = Activator.CreateInstance(_dynamicBuilders[prop.PropertyType]) as IGenericBuilder;
                            var obj = dynamicBuilder.GenericWithRandomProps().GenericBuild();
                            prop.SetValue(e, obj);
                            SetIDFieldsTheSame(e, obj);
                        };
                    }
                    break;
            }
        }

        private static void SetIDFieldsTheSame(TEntity parent, object child)
        {   // mocks relational mappings via fields ending with "id"
            var propInfos = parent.GetType().GetProperties();
            foreach (var propInfo in propInfos)
            {
                var name = propInfo.Name;
                if (!name.ToLower().EndsWith("id")) continue;
                var oprop = child.GetType().GetProperty(propInfo.Name);
                if (oprop == null) continue;
                try
                {
                    oprop.SetValue(child, propInfo.GetValue(parent));
                } catch {}
            }
        }

        private static void GenerateDynamicBuilder(PropertyInfo prop)
        {
            try
            {
                var t = typeof(GenericBuilder<,>);
                var moduleName = String.Join("_", new[] { "DynamicEntityBuilders", prop.PropertyType.Name });
                var modBuilder = _dynamicAssemblyBuilder.DefineDynamicModule(moduleName);

                var typeBuilder = modBuilder.DefineType(prop.PropertyType.Name + "Builder", TypeAttributes.Public | TypeAttributes.Class);
                // Typebuilder is a sub class of Type
                typeBuilder.SetParent(t.MakeGenericType(typeBuilder, prop.PropertyType));
                var dynamicBuilderType = typeBuilder.CreateType();
                _dynamicBuilders[prop.PropertyType] = dynamicBuilderType;
            }
            catch { }
        }


        private static Object _dynamicAssemblyLock = new object();
        private static AssemblyBuilder _dynamicAssemblyBuilderField;
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
        public void SetRandomProps(TEntity entity)
        {
            foreach (var prop in _EntityPropInfo)
            {
                try
                {
                    _randomPropSetters[prop.Name](entity);
                }
                catch
                {
                    //throw new Exception("Missing propSetter for: " + prop.Name);
                }
            }
        }

        private static object _lock = new object();
        private static Int64 _lastID = 1;
        public Int64 GetUniqueID()
        {
            lock(_lock)
            {
                return _lastID++;
            }
        }
    }
}
