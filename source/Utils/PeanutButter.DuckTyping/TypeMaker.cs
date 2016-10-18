using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace PeanutButter.DuckTyping
{

    public class PropertyNotFoundException : Exception
    {
        public PropertyNotFoundException(Type owningType, string propertyName)
            : base($"Property {propertyName} not found on type {owningType.Name}")
        {
        }
    }

    public class BackingFieldForPropertyNotFoundException : Exception
    {
        public BackingFieldForPropertyNotFoundException(Type owningType, string propertyName)
            : base($"Property {propertyName} not found on type {owningType.Name}")
        {
        }
    }

    public class ReadOnlyPropertyException: Exception
    {
        public ReadOnlyPropertyException(Type owningType, string propertyName)
            : base($"Property {propertyName} on type {owningType.Name} is read-only")
        {
        }
    }

    public class WriteOnlyPropertyException: Exception
    {
        public WriteOnlyPropertyException(Type owningType, string propertyName)
            : base($"Property {propertyName} on type {owningType.Name} is read-only")
        {
        }
    }
    public class ShimSham
    {
        private readonly object _wrapped;
        private readonly PropertyInfo[] _propertyInfos;
        private readonly Type _wrappedType;
        private readonly bool _wrappingADuck;
        private readonly FieldInfo[] _fieldInfos;

        public ShimSham(object toWrap)
        {
            _wrapped = toWrap;
            _wrappedType = toWrap.GetType();
            _wrappingADuck = IsObjectADuck();
            _propertyInfos = _wrappedType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (_wrappingADuck)
                _fieldInfos = _wrappedType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private bool IsObjectADuck()
        {
            return _wrappedType.GetCustomAttributes(true).OfType<IsADuckAttribute>().Any();
        }

        public object GetPropertyValue(string propertyName)
        {
            if (_wrappingADuck)
            {
                return FieldValueFor(propertyName);
            }

            var propInfo = FindPropertyInfoFor(propertyName);
            return propInfo.GetValue(_wrapped);
        }

        public void SetPropertyValue(string propertyName, object newValue)
        {

            if (_wrappingADuck)
            {
                SetFieldValue(propertyName, newValue);
                return;
            }

            var propInfo = FindPropertyInfoFor(propertyName);
            if (!propInfo.CanWrite)
            {
                throw new ReadOnlyPropertyException(_wrappedType, propertyName);
            }
            propInfo.SetValue(_wrapped, newValue);
        }

        private PropertyInfo FindPropertyInfoFor(string propertyName)
        {
            var propInfo = _propertyInfos.FirstOrDefault(pi => pi.Name.ToLower() == propertyName.ToLower());
            if (propInfo == null)
                throw new PropertyNotFoundException(_wrappedType, propertyName);
            return propInfo;
        }

        private void SetFieldValue(string propertyName, object newValue)
        {
            var fieldInfo = FindPrivateBackingFieldFor(propertyName);
            fieldInfo.SetValue(_wrapped, newValue);
        }

        private object FieldValueFor(string propertyName)
        {
            var fieldInfo = FindPrivateBackingFieldFor(propertyName);
            return fieldInfo.GetValue(_wrapped);
        }

        private FieldInfo FindPrivateBackingFieldFor(string propertyName)
        {
            var seek = "_" + propertyName;
            var fieldInfo = _fieldInfos.FirstOrDefault(fi => fi.Name.ToLower() == seek.ToLower());
            if (fieldInfo == null)
                throw new BackingFieldForPropertyNotFoundException(_wrappedType, propertyName);
            return fieldInfo;
        }
    }

    public class IsADuckAttribute : Attribute
    {
    }

    public interface ITypeMaker
    {
        Type MakeTypeImplementing<T>();
    }
    public class TypeMaker : ITypeMaker
    {
        private static readonly Type _shimType = typeof(ShimSham);
        private static readonly ConstructorInfo _shimConstructor = _shimType.GetConstructor(new[] { typeof(object) });
        private static readonly ConstructorInfo _objectConstructor = typeof(object).GetConstructor(new Type[0]);
        private static readonly MethodInfo _shimGetPropertyValueMethod = 
            _shimType.GetMethod("GetPropertyValue");
        private static readonly MethodInfo _shimSetPropertyValueMethod = 
            _shimType.GetMethod("SetPropertyValue");

        public Type MakeTypeImplementing<T>()
        {
            var interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
                throw new InvalidOperationException("MakeTypeImplementing<T> requires an interface for the type parameter");
            var identifier = (Guid.NewGuid().ToString("N"));
            var moduleName = string.Join("_", "Generated", interfaceType.Name, identifier);
            var modBuilder = DynamicAssemblyBuilder.DefineDynamicModule(moduleName);

            var generatedTypeName = interfaceType.Name + "_Duck_" + identifier;
            var typeBuilder = modBuilder.DefineType(generatedTypeName, TypeAttributes.Public);

            var attribConstructor = typeof(IsADuckAttribute).GetConstructor(new Type[0]);
            var attribBuilder = new CustomAttributeBuilder(attribConstructor, new object[0]);
            typeBuilder.SetCustomAttribute(attribBuilder);

            typeBuilder.AddInterfaceImplementation(interfaceType);


            var shimField = AddShimField(typeBuilder);
            AddAllProperties(typeBuilder, interfaceType, shimField);
            AddDefaultConstructor(typeBuilder, shimField);
            AddWrappingConstructor(typeBuilder, shimField);

            return typeBuilder.CreateType();
        }

        private FieldBuilder AddShimField(TypeBuilder typeBuilder)
        {
            return typeBuilder.DefineField("_shim", _shimType, FieldAttributes.Private);
        }

        private void AddDefaultConstructor(TypeBuilder typeBuilder, FieldBuilder shimField)
        {
            var ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[0]);
            var il = ctor.GetILGenerator();
            CallBaseObjectConstructor(il);

            var result = CreateWrappingShimForThisWith(il);

            StoreShimInFieldWith(shimField, il, result);

            MethodReturn(il);
        }

        private void AddWrappingConstructor(
            TypeBuilder typeBuilder,
            FieldBuilder shimField
        )
        {
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(object) });
            var il = ctorBuilder.GetILGenerator();
            CallBaseObjectConstructor(il);
            var result = CreateWrappingShimForArg1With(il);
            StoreShimInFieldWith(shimField, il, result);
            MethodReturn(il);
        }

        private static void StoreShimInFieldWith(FieldBuilder shimField, ILGenerator generator, LocalBuilder result)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Stfld, shimField);
        }

        private static LocalBuilder CreateWrappingShimForArg1With(ILGenerator il)
        {
            return CreateWrappingShimFor(OpCodes.Ldarg_1, il);
        }

        private static LocalBuilder CreateWrappingShimForThisWith(ILGenerator il)
        {
            return CreateWrappingShimFor(OpCodes.Ldarg_0, il);
        }

        private static LocalBuilder CreateWrappingShimFor(OpCode code, ILGenerator il)
        {
            var result = il.DeclareLocal(typeof(ShimSham));
            il.Emit(code);
            il.Emit(OpCodes.Newobj, _shimConstructor);
            il.Emit(OpCodes.Stloc, result);
            return result;
        }

        private static void CallBaseObjectConstructor(ILGenerator generator)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, _objectConstructor);
        }

        // TODO: get on to this when property pass-through works
        private int AddAllMethods(TypeBuilder typeBuilder, Type interfaceType)
        {
            var methodInfos = interfaceType.GetMethods();
            foreach (var methodInfo in methodInfos)
            {
                AddMethod(interfaceType, typeBuilder, methodInfo);
            }
            return methodInfos.Length;
        }

        private void AddMethod(Type interfaceType, TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name,
                _propertyGetterSetterMethodAttributes, methodInfo.ReturnType,
                    methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
            var ilGenerator = methodBuilder.GetILGenerator();
            ImplementInterfaceMethodAsRequired(interfaceType, typeBuilder, methodInfo.Name, methodBuilder);

            var callThroughMi = GetType().GetMethod("CallThrough", BindingFlags.Static | BindingFlags.NonPublic);
            ilGenerator.Emit(OpCodes.Call, callThroughMi);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private void AddAllProperties(
            TypeBuilder typeBuilder,
            Type interfaceType,
            FieldBuilder shimField)
        {
            foreach (var prop in interfaceType.GetProperties())
            {
                AddProperty(interfaceType, typeBuilder, prop, shimField);
            }
        }

        private static void AddProperty(
            Type interfaceType,
            TypeBuilder typeBuilder,
            PropertyInfo prop,
            FieldBuilder shimField)
        {
            var backingField = typeBuilder.DefineField("_" + prop.Name, prop.PropertyType, FieldAttributes.Private);

            var propBuilder = typeBuilder.DefineProperty(
                                    prop.Name,
                                    PropertyAttributes.HasDefault,
                                    prop.PropertyType,
                                    null);

            var getMethod = MakeShimGetMethodFor(interfaceType, typeBuilder, prop, shimField);
//            var setMethod = MakeSetMethodOnBackingFieldFor(interfaceType, typeBuilder, prop, backingField);
            var setMethod = MakeShimSetMethodFor(interfaceType, typeBuilder, prop, shimField, backingField);

            propBuilder.SetSetMethod(setMethod);
            propBuilder.SetGetMethod(getMethod);
        }

        private static MethodBuilder MakeShimGetMethodFor(
            Type interfaceType,
            TypeBuilder typeBuilder,
            PropertyInfo prop,
            FieldBuilder shimField)
        {
            var methodName = "get_" + prop.Name;
            var getMethod = typeBuilder.DefineMethod(methodName,
                _propertyGetterSetterMethodAttributes, prop.PropertyType, Type.EmptyTypes);
            var il = getMethod.GetILGenerator();

            var local = StorePropertyValueInLocal(il, shimField, prop.Name);

            UnboxLocal(il, local, prop.PropertyType);
            il.Emit(OpCodes.Ret);

            ImplementInterfaceMethodAsRequired(
                interfaceType,
                typeBuilder,
                methodName,
                getMethod);
            return getMethod;
        }

        private static void UnboxLocal(
            ILGenerator il, 
            LocalBuilder local, 
            Type unboxType)
        {
            il.Emit(OpCodes.Ldloc, local);
            il.Emit(OpCodes.Unbox_Any, unboxType);
        }

        private static LocalBuilder StorePropertyValueInLocal(
            ILGenerator il, 
            FieldBuilder shimField, 
            string propertyName)
        {
            var local = il.DeclareLocal(typeof(object));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, shimField);
            il.Emit(OpCodes.Ldstr, propertyName);
            il.Emit(OpCodes.Call, _shimGetPropertyValueMethod);
            il.Emit(OpCodes.Stloc, local);
            return local;
        }

        private static MethodBuilder MakeShimSetMethodFor(
            Type interfaceType,
            TypeBuilder typeBuilder,
            PropertyInfo prop,
            FieldBuilder shimField,
            FieldBuilder backingField
        )
        {
            var methodName = "set_" + prop.Name;
            var setMethod = typeBuilder.DefineMethod(methodName,
                _propertyGetterSetterMethodAttributes, null, new[] { prop.PropertyType });
            var il = setMethod.GetILGenerator();

            var boxed = il.DeclareLocal(typeof(object));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Box, prop.PropertyType);
            il.Emit(OpCodes.Stloc, boxed);

            var propertyName = prop.Name;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, shimField);
            il.Emit(OpCodes.Ldstr, propertyName);
            il.Emit(OpCodes.Ldloc, boxed);
            il.Emit(OpCodes.Call, _shimSetPropertyValueMethod);
//            il.Emit(OpCodes.Stloc, local);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, backingField);

            MethodReturn(il);

            ImplementInterfaceMethodAsRequired(
                interfaceType,
                typeBuilder,
                methodName,
                setMethod);

            return setMethod;
        }


        private static MethodBuilder MakeSetMethodOnBackingFieldFor(
            Type interfaceType,
            TypeBuilder typeBuilder,
            PropertyInfo prop,
            FieldBuilder backingField)
        {
            var methodName = "set_" + prop.Name;
            var setMethod = typeBuilder.DefineMethod(methodName,
                _propertyGetterSetterMethodAttributes, null, new[] { prop.PropertyType });
            var il = setMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, backingField);
            MethodReturn(il);

            ImplementInterfaceMethodAsRequired(
                interfaceType,
                typeBuilder,
                methodName,
                setMethod);

            return setMethod;
        }

        private static void MethodReturn(ILGenerator setIlGenerator)
        {
            setIlGenerator.Emit(OpCodes.Ret);
        }

        private static void ImplementInterfaceMethodAsRequired(Type interfaceType, TypeBuilder typeBuilder, string methodName, MethodBuilder setMethod)
        {
            var interfaceMethod = interfaceType.GetMethod(methodName);
            if (interfaceMethod != null)
                typeBuilder.DefineMethodOverride(setMethod, interfaceMethod);
        }

        private static readonly MethodAttributes _propertyGetterSetterMethodAttributes =
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.Virtual |
            MethodAttributes.HideBySig;

        private static readonly object _dynamicAssemblyLock = new object();
        private static AssemblyBuilder _dynamicAssemblyBuilderField;
        private const string ASSEMBLY_NAME = "PeanutButter.DuckTyping.GeneratedTypes";

        private static AssemblyBuilder DynamicAssemblyBuilder
        {
            get
            {
                lock (_dynamicAssemblyLock)
                {
                    return _dynamicAssemblyBuilderField ??
                            (_dynamicAssemblyBuilderField = DefineDynamicAssembly(ASSEMBLY_NAME));
                }
            }
        }
        private static AssemblyBuilder DefineDynamicAssembly(string withName)
        {
            return AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName(withName),
                AssemblyBuilderAccess.RunAndSave);
        }
    }
}
