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
    public interface ITypeMaker
    {
        Type MakeTypeImplementing<T>();
    }
    public class TypeMaker: ITypeMaker
    {
        public Type MakeTypeImplementing<T>()
        {
            var interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
                throw new InvalidOperationException("MakeTypeImplementing<T> requires an interface for the type parameter");
            var moduleName = string.Join("_", "Generated", interfaceType.Name);
            var modBuilder = DynamicAssemblyBuilder.DefineDynamicModule(moduleName);

            var generatedTypeName = interfaceType.Name + "Impl";
            var typeBuilder = modBuilder.DefineType(generatedTypeName, TypeAttributes.Public);
            typeBuilder.AddInterfaceImplementation(interfaceType);

            AddAllProperties(typeBuilder, interfaceType);
            AddDefaultConstructor(typeBuilder);
            AddWrappingConstructor(typeBuilder, interfaceType);

            return typeBuilder.CreateType();
        }

        private void AddDefaultConstructor(TypeBuilder typeBuilder)
        {
            var ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[0]);
            var generator = ctor.GetILGenerator();
            generator.Emit(OpCodes.Ret);
        }

        private void AddWrappingConstructor(
            TypeBuilder typeBuilder,
            Type interfaceType)
        {
            var ctorBuilder = typeBuilder.DefineConstructor(
                                    MethodAttributes.Public, 
                                    CallingConventions.Standard, 
                                    new[] {typeof(object)});
            var backingField = typeBuilder.DefineField("_actual", interfaceType, FieldAttributes.Private);
            var ilGenerator = ctorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stfld, backingField);
            ilGenerator.Emit(OpCodes.Ret);

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

        private void AddAllProperties(TypeBuilder typeBuilder, Type interfaceType)
        {
            foreach (var prop in interfaceType.GetProperties())
            {
                AddProperty(interfaceType, typeBuilder, prop);
            }
        }

        private static void AddProperty(Type interfaceType, TypeBuilder typeBuilder, PropertyInfo prop)
        {
            var backingField = typeBuilder.DefineField("_" + prop.Name, prop.PropertyType, FieldAttributes.Private);

            var propBuilder = typeBuilder.DefineProperty(
                                    prop.Name,
                                    PropertyAttributes.HasDefault,
                                    prop.PropertyType,
                                    null);

            var getMethod = MakeGetMethodFor(interfaceType, typeBuilder, prop, backingField);
            var setMethod = MakeSetMethodFor(interfaceType, typeBuilder, prop, backingField);

            propBuilder.SetSetMethod(setMethod);
            propBuilder.SetGetMethod(getMethod);
        }

        private static MethodBuilder MakeGetMethodFor(
            Type interfaceType, 
            TypeBuilder typeBuilder, 
            PropertyInfo prop, 
            FieldBuilder backingField)
        {
            var methodName = "get_" + prop.Name;
            var getMethod = typeBuilder.DefineMethod(methodName,
                _propertyGetterSetterMethodAttributes, prop.PropertyType, Type.EmptyTypes);
            var getIlGenerator = getMethod.GetILGenerator();
            getIlGenerator.Emit(OpCodes.Ldarg_0);
            getIlGenerator.Emit(OpCodes.Ldfld, backingField);
            getIlGenerator.Emit(OpCodes.Ret);

            ImplementInterfaceMethodAsRequired(
                interfaceType,
                typeBuilder,
                methodName,
                getMethod);

            return getMethod;
        }

        private static MethodBuilder MakeSetMethodFor(
            Type interfaceType,
            TypeBuilder typeBuilder,
            PropertyInfo prop,
            FieldBuilder backingField)
        {
            var methodName = "set_" + prop.Name;
            var setMethod = typeBuilder.DefineMethod(methodName,
                _propertyGetterSetterMethodAttributes, null, new[] {prop.PropertyType});
            var setIlGenerator = setMethod.GetILGenerator();
            setIlGenerator.Emit(OpCodes.Ldarg_0);
            setIlGenerator.Emit(OpCodes.Ldarg_1);
            setIlGenerator.Emit(OpCodes.Stfld, backingField);
            setIlGenerator.Emit(OpCodes.Ret);

            ImplementInterfaceMethodAsRequired(
                interfaceType, 
                typeBuilder, 
                methodName, 
                setMethod);

            return setMethod;
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
