using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping
{
    /// <summary>
    /// Utility class to create types on the fly which implement provided interfaces, 
    /// when possible.
    /// </summary>
    public class TypeMaker : ITypeMaker
    {
        private const string ASSEMBLY_NAME = "PeanutButter.DuckTyping.GeneratedTypes";
        private static readonly Type _shimInterfaceType = typeof(IShimSham);
        private static readonly Type _shimType = typeof(ShimSham);
        private static readonly Type _dictionaryShim = typeof(DictionaryShimSham);

        private static readonly ConstructorInfo _shimConstructor = _shimType.GetConstructor(
            new[] {typeof(object), typeof(Type), typeof(bool)}
        );

        private static readonly ConstructorInfo _dictionaryShimConstructor =
            _dictionaryShim.GetConstructor(new[]
            {
                typeof(IDictionary<string, object>), typeof(Type)
            });

        private static readonly ConstructorInfo _objectConstructor = typeof(object).GetConstructor(new Type[0]);

        private static readonly MethodInfo _shimGetPropertyValueMethod =
            _shimInterfaceType.GetMethod("GetPropertyValue");

        private static readonly MethodInfo _shimSetPropertyValueMethod =
            _shimInterfaceType.GetMethod("SetPropertyValue");

        private static readonly MethodInfo _shimCallThroughMethod =
            _shimInterfaceType.GetMethod("CallThrough");

        private static readonly MethodInfo _shimCallThroughVoidMethod =
            _shimInterfaceType.GetMethod("CallThroughVoid");

        private static readonly MethodAttributes _propertyGetterSetterMethodAttributes =
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.Virtual |
            MethodAttributes.HideBySig;

        private static readonly object _dynamicAssemblyLock = new object();
        private static AssemblyBuilder _dynamicAssemblyBuilderField;

        /// <inheritdoc />
        public Type MakeTypeImplementing<T>()
        {
            return MakeTypeImplementing<T>(false);
        }

        /// <inheritdoc />
        public Type MakeFuzzyTypeImplementing<T>()
        {
            return MakeTypeImplementing<T>(true);
        }

        private Type MakeTypeImplementing<T>(bool isFuzzy)
        {
            var interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
                throw new InvalidOperationException("MakeTypeImplementing<T> requires an interface for the type parameter");
            var identifier = Guid.NewGuid().ToString("N");
            var moduleName = string.Join("_", identifier, "_Gen_", interfaceType.Name);
            var modBuilder = DynamicAssemblyBuilder.DefineDynamicModule(moduleName);

            var generatedTypeName = interfaceType.Name + "_Duck_" + identifier;
            var typeBuilder = modBuilder.DefineType(generatedTypeName, TypeAttributes.Public);

            var attribConstructor = typeof(IsADuckAttribute).GetConstructor(new Type[0]);
            // we have full control over the constructor; testing for null is a waste of time.
            // ReSharper disable once AssignNullToNotNullAttribute
            var attribBuilder = new CustomAttributeBuilder(attribConstructor, new object[0]);
            typeBuilder.SetCustomAttribute(attribBuilder);
            CopyCustomAttributes(interfaceType, typeBuilder);

            typeBuilder.AddInterfaceImplementation(interfaceType);

            var shimField = AddShimField(typeBuilder);
            var allInterfaceTypes = interfaceType.GetAllImplementedInterfaces();
            AddAllPropertiesAsShimmable(typeBuilder, allInterfaceTypes, shimField);
            AddAllMethodsAsShimmable(typeBuilder, allInterfaceTypes, shimField);
            AddDefaultConstructor(typeBuilder, shimField, interfaceType, isFuzzy);
            AddWrappingConstructor(typeBuilder, shimField, interfaceType, isFuzzy);
            AddDictionaryWrappingConstructor(typeBuilder, shimField, interfaceType);

            return typeBuilder.CreateType();
        }

        private FieldBuilder AddShimField(TypeBuilder typeBuilder)
        {
            return typeBuilder.DefineField("_shim", _shimInterfaceType, FieldAttributes.Private);
        }

        private void AddDefaultConstructor(
            TypeBuilder typeBuilder,
            FieldBuilder shimField,
            Type interfaceType,
            bool isFuzzy)
        {
            var ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[0]);
            var il = ctor.GetILGenerator();
            CallBaseObjectConstructor(il);

            var result = CreateWrappingShimForThisWith(il, interfaceType, isFuzzy);

            StoreShimInFieldWith(shimField, il, result);

            ImplementMethodReturnWith(il);
        }

        private void AddWrappingConstructor(
            TypeBuilder typeBuilder,
            FieldBuilder shimField,
            Type interfaceType,
            bool isFuzzy
        )
        {
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] {typeof(object)});
            var il = ctorBuilder.GetILGenerator();
            CallBaseObjectConstructor(il);
            var result = CreateWrappingShimForArg1With(il, interfaceType, isFuzzy);
            StoreShimInFieldWith(shimField, il, result);
            ImplementMethodReturnWith(il);
        }

        private void AddDictionaryWrappingConstructor(
            TypeBuilder typeBuilder,
            FieldBuilder shimField,
            Type interfaceType
        )
        {
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] {typeof(IDictionary<string, object>)});
            var il = ctorBuilder.GetILGenerator();
            CallBaseObjectConstructor(il);
            var result = CreateWrappingDictionaryShimFor(il, interfaceType);
            StoreShimInFieldWith(shimField, il, result);
            ImplementMethodReturnWith(il);
        }

        private static LocalBuilder CreateWrappingDictionaryShimFor(ILGenerator il, Type interfaceType)
        {
            var result = il.DeclareLocal(typeof(IShimSham));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldtoken, interfaceType);
            il.Emit(OpCodes.Newobj, _dictionaryShimConstructor);
            il.Emit(OpCodes.Stloc, result);
            return result;
        }

        private static void StoreShimInFieldWith(FieldBuilder shimField, ILGenerator generator, LocalBuilder result)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Stfld, shimField);
        }

        private static LocalBuilder CreateWrappingShimForArg1With(ILGenerator il, Type interfaceType, bool isFuzzy)
        {
            return CreateWrappingShimFor(OpCodes.Ldarg_1, il, interfaceType, isFuzzy);
        }

        private static LocalBuilder CreateWrappingShimForThisWith(ILGenerator il, Type interfaceType, bool isFuzzy)
        {
            return CreateWrappingShimFor(OpCodes.Ldarg_0, il, interfaceType, isFuzzy);
        }

        private static LocalBuilder CreateWrappingShimFor(OpCode code, ILGenerator il, Type interfaceType, bool isFuzzy)
        {
            var result = il.DeclareLocal(typeof(IShimSham));
            il.Emit(code);
            il.Emit(OpCodes.Ldtoken, interfaceType);
            il.Emit(isFuzzy ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Newobj, _shimConstructor);
            il.Emit(OpCodes.Stloc, result);
            return result;
        }

        private static void CallBaseObjectConstructor(ILGenerator generator)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, _objectConstructor);
        }

        private void AddAllMethodsAsShimmable(
            TypeBuilder typeBuilder,
            Type[] interfaceTypes,
            FieldBuilder shimField)
        {
            var methodInfos = GetAllMethodsFor(interfaceTypes);
            foreach (var methodInfo in methodInfos)
            {
                AddMethod(interfaceTypes[0], typeBuilder, methodInfo, shimField);
            }
        }

        private static bool MethodIsNotSpecial(MethodInfo mi)
        {
            return ((int) mi.Attributes & (int) MethodAttributes.SpecialName) != (int) MethodAttributes.SpecialName;
        }

        private void AddMethod(
            Type interfaceType,
            TypeBuilder typeBuilder,
            MethodInfo methodInfo,
            FieldBuilder shimField)
        {
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name,
                _propertyGetterSetterMethodAttributes, methodInfo.ReturnType,
                methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
            var il = methodBuilder.GetILGenerator();
            if (methodInfo.ReturnType == typeof(void))
                ImplementVoidReturnCallThroughWith(methodInfo, shimField, il);
            else
                ImplementNonVoidReturnCallThroughWith(methodInfo, shimField, il);

            ImplementMethodReturnWith(il);
            ImplementInterfaceMethodAsRequired(interfaceType, typeBuilder, methodInfo.Name, methodBuilder);

            CopyCustomAttributes(methodInfo, methodBuilder);
        }

        private static void ImplementVoidReturnCallThroughWith(
            MethodInfo methodInfo,
            FieldBuilder shimField,
            ILGenerator il
        )
        {
            ImplementCallThroughWith(_shimCallThroughVoidMethod, methodInfo, shimField, il);
        }

        private static void ImplementNonVoidReturnCallThroughWith(
            MethodInfo methodInfo,
            FieldBuilder shimField,
            ILGenerator il
        )
        {
            ImplementCallThroughWith(_shimCallThroughMethod, methodInfo, shimField, il);
            il.Emit(OpCodes.Unbox_Any, methodInfo.ReturnType);
        }

        private static void ImplementCallThroughWith(
            MethodInfo shimMethod,
            MethodInfo callThroughMethod,
            FieldBuilder shimField,
            ILGenerator il
        )
        {
            var methodParameters = callThroughMethod.GetParameters();
            var boxedParameters = DeclareArray<object>(il, methodParameters.Length);

            LoadMethodArgumentsIntoArray(il, boxedParameters, methodParameters);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, shimField);
            il.Emit(OpCodes.Ldstr, callThroughMethod.Name);
            il.Emit(OpCodes.Ldloc, boxedParameters);

            il.Emit(OpCodes.Call, shimMethod);
        }

        private static void LoadMethodArgumentsIntoArray(ILGenerator il, LocalBuilder boxedParameters, ParameterInfo[] methodParameters)
        {
            var boxed = il.DeclareLocal(typeof(object));

            var idx = 0;
            foreach (var param in methodParameters)
            {
                il.Emit(OpCodes.Ldarg, idx + 1);
                il.Emit(OpCodes.Box, param.ParameterType);
                il.Emit(OpCodes.Stloc, boxed);

                SetRefTypeArrayItem(il, boxedParameters, idx, boxed);

                idx++;
            }
        }

        private static void SetRefTypeArrayItem(ILGenerator il, LocalBuilder array, int idx, LocalBuilder local)
        {
            il.Emit(OpCodes.Ldloc, array);
            il.Emit(OpCodes.Ldc_I4, idx);
            il.Emit(OpCodes.Ldloc, local);
            il.Emit(OpCodes.Stelem_Ref);
        }

        private static LocalBuilder DeclareArray<T>(ILGenerator il, int elements)
        {
            var array = il.DeclareLocal(typeof(T[]));
            il.Emit(OpCodes.Ldc_I4, elements);
            il.Emit(OpCodes.Newarr, typeof(T));
            il.Emit(OpCodes.Stloc, array);
            return array;
        }

        private void AddAllPropertiesAsShimmable(
            TypeBuilder typeBuilder,
            Type[] interfaceTypes,
            FieldBuilder shimField)
        {
            var allProperties = GetAllPropertiesFor(interfaceTypes);
            foreach (var prop in allProperties)
            {
                AddShimmableProperty(interfaceTypes[0], typeBuilder, prop, shimField);
            }
        }

        private PropertyInfo[] GetAllPropertiesFor(Type[] allImplementedInterfaces)
        {
            return GetAllFor(
                    allImplementedInterfaces,
                    t => t.GetProperties())
                .Distinct(new PropertyInfoComparer())
                .ToArray();
        }

        private MethodInfo[] GetAllMethodsFor(Type[] allImplementedInterfaces)
        {
            return GetAllFor(allImplementedInterfaces,
                t => t.GetMethods().Where(MethodIsNotSpecial));
        }

        private T[] GetAllFor<T>(Type[] interfaces, Func<Type, IEnumerable<T>> fetcher)
        {
            return interfaces
                .Select(fetcher)
                .SelectMany(a => a)
                .ToArray();
        }

        private static void AddShimmableProperty(
            Type interfaceType,
            TypeBuilder typeBuilder,
            PropertyInfo prop,
            FieldBuilder shimField)
        {
            DefineBackingFieldForDuckDucks(typeBuilder, prop);
            var propBuilder = typeBuilder.DefineProperty(
                prop.Name,
                PropertyAttributes.HasDefault,
                prop.PropertyType,
                null);

            var getMethod = MakeShimGetMethodFor(interfaceType, typeBuilder, prop, shimField);
            var setMethod = MakeShimSetMethodFor(interfaceType, typeBuilder, prop, shimField);

            propBuilder.SetSetMethod(setMethod);
            propBuilder.SetGetMethod(getMethod);
            CopyCustomAttributes(prop, propBuilder);
        }

        private static void CopyCustomAttributes(Type src, TypeBuilder typeBuilder)
        {
            var attribData = CustomAttributeData.GetCustomAttributes(src);
            foreach (var data in attribData)
            {
                var builder = data.ToAttributeBuilder();
                if (builder != null)
                    typeBuilder.SetCustomAttribute(builder);
            }
        }

        private static void CopyCustomAttributes(MemberInfo src, PropertyBuilder propBuilder)
        {
            var attribData = CustomAttributeData.GetCustomAttributes(src);
            foreach (var data in attribData)
            {
                var builder = data.ToAttributeBuilder();
                if (builder != null)
                    propBuilder.SetCustomAttribute(builder);
            }
        }

        private static void CopyCustomAttributes(MemberInfo src, MethodBuilder methodBuilder)
        {
            var attribData = CustomAttributeData.GetCustomAttributes(src);
            foreach (var data in attribData)
            {
                var builder = data.ToAttributeBuilder();
                if (builder != null)
                    methodBuilder.SetCustomAttribute(builder);
            }
        }

        private static void DefineBackingFieldForDuckDucks(TypeBuilder typeBuilder, PropertyInfo prop)
        {
            typeBuilder.DefineField("_" + prop.Name, prop.PropertyType, FieldAttributes.Private);
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
            FieldBuilder shimField
        )
        {
            var methodName = "set_" + prop.Name;
            var setMethod = typeBuilder.DefineMethod(methodName,
                _propertyGetterSetterMethodAttributes, null, new[] {prop.PropertyType});
            var il = setMethod.GetILGenerator();

            var boxed = il.DeclareLocal(typeof(object));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Box, prop.PropertyType);
            il.Emit(OpCodes.Stloc, boxed);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, shimField);
            il.Emit(OpCodes.Ldstr, prop.Name);
            il.Emit(OpCodes.Ldloc, boxed);
            il.Emit(OpCodes.Call, _shimSetPropertyValueMethod);

            ImplementMethodReturnWith(il);

            ImplementInterfaceMethodAsRequired(
                interfaceType,
                typeBuilder,
                methodName,
                setMethod);

            return setMethod;
        }

        private static void ImplementMethodReturnWith(ILGenerator setIlGenerator)
        {
            setIlGenerator.Emit(OpCodes.Ret);
        }

        private static void ImplementInterfaceMethodAsRequired(
            Type interfaceType,
            TypeBuilder typeBuilder,
            string methodName,
            MethodBuilder methodImplementation)
        {
            var interfaceMethod = interfaceType.GetMethod(methodName);
            if (interfaceMethod != null)
                typeBuilder.DefineMethodOverride(methodImplementation, interfaceMethod);
        }


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