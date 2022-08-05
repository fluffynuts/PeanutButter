using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
using Imported.PeanutButter.DuckTyping.Comparers;
using Imported.PeanutButter.DuckTyping.Extensions;
#else
using PeanutButter.DuckTyping.Comparers;
using PeanutButter.DuckTyping.Extensions;
#endif
using Imported.PeanutButter.Utils;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Shimming
#else
namespace PeanutButter.DuckTyping.Shimming
#endif
{
    /// <summary>
    /// Utility class to create types on the fly which implement provided interfaces,
    /// when possible.
    /// </summary>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    internal
#else
    public
#endif
    class TypeMaker : ITypeMaker
    {
        private const string ASSEMBLY_NAME = "PeanutButter.DuckTyping.GeneratedTypes";
        private static readonly Type ShimInterfaceType = typeof(IShimSham);
        private static readonly Type ShimType = typeof(ShimSham);
        private static readonly Type DictionaryShim = typeof(DictionaryShimSham);

        private static readonly ConstructorInfo ShimConstructorForObjectArray = ShimType.GetConstructor(
            new[] { typeof(object[]), typeof(Type), typeof(bool), typeof(bool) }
        );

        private static readonly ConstructorInfo ShimConstructorForObject = ShimType.GetConstructor(
            new[] { typeof(object), typeof(Type), typeof(bool), typeof(bool) }
        );

        private static readonly ConstructorInfo DictionaryArrayShimConstructor =
            DictionaryShim.GetConstructor(new[]
            {
                typeof(IDictionary<string, object>[]), typeof(Type)
            });

        private static readonly ConstructorInfo DictionaryShimConstructor =
            DictionaryShim.GetConstructor(new[]
            {
                typeof(IDictionary<string, object>), typeof(Type)
            });

        private static readonly ConstructorInfo ObjectConstructor = typeof(object).GetConstructor(new Type[0]);

        private static readonly MethodInfo ShimGetPropertyValueMethod =
            ShimInterfaceType.GetMethod(nameof(IShimSham.GetPropertyValue));

        private static readonly MethodInfo ShimSetPropertyValueMethod =
            ShimInterfaceType.GetMethod(nameof(IShimSham.SetPropertyValue));

        private static readonly MethodInfo ShimCallThroughMethod =
            ShimInterfaceType.GetMethod(nameof(IShimSham.CallThrough));

        private static readonly MethodInfo ShimCallThroughVoidMethod =
            ShimInterfaceType.GetMethod(nameof(IShimSham.CallThroughVoid));

        private static readonly MethodAttributes PropertyGetterSetterMethodVirtualAttributes =
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.Virtual |
            MethodAttributes.HideBySig;

        private static readonly object DynamicAssemblyLock = new object();
        private static AssemblyBuilder _dynamicAssemblyBuilderField;
        private static ModuleBuilder _moduleBuilder;

        /// <inheritdoc />
        public Type MakeTypeImplementing<T>()
        {
            return MakeTypeImplementing(typeof(T), false);
        }

        /// <inheritdoc />
        public Type MakeTypeImplementing<T>(bool forceConcrete)
        {
            return MakeTypeImplementing(typeof(T), forceConcrete);
        }

        /// <inheritdoc />
        public Type MakeTypeImplementing(Type type, bool forceConcrete)
        {
            return MakeTypeImplementing(
                type,
                isFuzzy: false,
                allowDefaultsForReadonlyMembers: false,
                forceConcrete
            );
        }

        /// <inheritdoc />
        public Type MakeTypeImplementing(Type type)
        {
            return MakeTypeImplementing(
                type,
                isFuzzy: false,
                allowDefaultsForReadonlyMembers: false,
                forceConcreteClass: false
            );
        }

        /// <inheritdoc />
        public Type MakeFuzzyTypeImplementing<T>()
        {
            return MakeFuzzyTypeImplementing(typeof(T));
        }

        /// <inheritdoc />
        public Type MakeFuzzyTypeImplementing<T>(bool forceConcrete)
        {
            return MakeFuzzyTypeImplementing(typeof(T), forceConcrete);
        }

        /// <inheritdoc />
        public Type MakeFuzzyTypeImplementing(Type type)
        {
            return MakeFuzzyTypeImplementing(type, false);
        }

        /// <inheritdoc />
        public Type MakeFuzzyTypeImplementing(
            Type type,
            bool forceConcrete
        )
        {
            return MakeTypeImplementing(type, true, false, forceConcrete);
        }

        /// <inheritdoc />
        public Type MakeFuzzyDefaultingTypeImplementing<T>()
        {
            return MakeFuzzyDefaultingTypeImplementing(typeof(T), false);
        }

        /// <inheritdoc />
        public Type MakeFuzzyDefaultingTypeImplementing<T>(
            bool forceConcrete
        )
        {
            return MakeFuzzyDefaultingTypeImplementing(typeof(T), forceConcrete);
        }

        /// <inheritdoc />
        public Type MakeFuzzyDefaultingTypeImplementing(Type type)
        {
            return MakeFuzzyDefaultingTypeImplementing(type, false);
        }

        /// <inheritdoc />
        public Type MakeFuzzyDefaultingTypeImplementing(
            Type type,
            bool forceConcreteType
        )
        {
            return MakeTypeImplementing(type, true, true, forceConcreteType);
        }

        private static ModuleBuilder ModuleBuilder =>
            _moduleBuilder ??= CreateModuleBuilder();

        private static ModuleBuilder CreateModuleBuilder()
        {
            var moduleName = "__PeanutButter_DuckTyped_Gen__";
            return DynamicAssemblyBuilder.DefineDynamicModule(moduleName);
        }

        private Type MakeTypeImplementing(
            Type type,
            bool isFuzzy,
            bool allowDefaultsForReadonlyMembers,
            bool forceConcreteClass = false)
        {
            if (!type.IsInterface && !forceConcreteClass)
            {
                if (!type.AllPublicInstancePropertiesAndMethodsAreVirtualOrAbstractAndNonFinal())
                {
                    // don't even try to duck something that's not all-virtual unless forced
                    return ThrowCannotDuckType();
                }
            }

            var identifier = Guid.NewGuid().ToString("N");

            var generatedTypeName = type.Name + "_Duck_" + identifier;
            var typeBuilder = type.IsInterface
                ? ModuleBuilder.DefineType(generatedTypeName, TypeAttributes.Public)
                : ModuleBuilder.DefineType(generatedTypeName, TypeAttributes.Public, type);

            var attribConstructor = typeof(IsADuckAttribute).GetConstructor(new Type[0]);
            // we have full control over the constructor; testing for null is a waste of time.
            // ReSharper disable once AssignNullToNotNullAttribute
            var attribBuilder = new CustomAttributeBuilder(attribConstructor, new object[0]);
            typeBuilder.SetCustomAttribute(attribBuilder);
            CopyCustomAttributes(type, typeBuilder);

            if (type.IsInterface)
            {
                typeBuilder.AddInterfaceImplementation(type);
            }

            var shimField = AddShimField(typeBuilder);
            var allTypesToImplement = type.GetAllImplementedInterfaces();
            if (!type.IsInterface)
            {
                allTypesToImplement = allTypesToImplement.And(type);
            }

            AddAllPropertiesAsShimmable(typeBuilder, allTypesToImplement, shimField, forceConcreteClass);
            AddAllMethodsAsShimmable(typeBuilder, allTypesToImplement, shimField, forceConcreteClass);

            AddDefaultConstructor(typeBuilder, shimField, type, isFuzzy, allowDefaultsForReadonlyMembers);
            AddObjectWrappingConstructors(typeBuilder, shimField, type, isFuzzy,
                allowDefaultsForReadonlyMembers);
            AddDictionaryWrappingConstructors(typeBuilder, shimField, type);

            try
            {
#if NETSTANDARD
            return typeBuilder.CreateTypeInfo();
#else
                return typeBuilder.CreateType();
#endif
            }
            catch (TypeLoadException ex)
            {
                if (forceConcreteClass && ex.Message.Contains("final method"))
                {
                    // sometimes the forced-duck can fail on a concrete class
                    return ThrowCannotDuckType();
                }

                throw;
            }

            Type ThrowCannotDuckType()
            {
                throw new InvalidOperationException(
                    @$"
Cannot make a type implementing {type}:

MakeTypeImplementing<T> requires an interface, or class with all-virtual members 
for the type parameter.

Concrete classes with non-virtual members will produce unexpected results as 
casting down to the given type will result in accessing the members
on that type instead of the type generated for duck-typing, or will result
in run-time errors like:
'Declaration referenced in a method implementation cannot be a final method'

You may force duck-typing to happen, but be aware that only virtual and abstract
members will behave as expected. Other members can be reached via reflection, eg
by using the helper extension methods GetTopmostProperty and SetTopmostProperty:

duckedResult.GetTopmostProperty<T>(""PropertyName"");

but this really isn't pretty ):"
                );
            }
        }

        private const BindingFlags PUBLIC_INSTANCE = BindingFlags.Public | BindingFlags.Instance;

        private FieldBuilder AddShimField(TypeBuilder typeBuilder)
        {
            return typeBuilder.DefineField("_shim", ShimInterfaceType, FieldAttributes.Private);
        }

        private void AddDefaultConstructor(
            TypeBuilder typeBuilder,
            FieldBuilder shimField,
            Type interfaceType,
            bool isFuzzy,
            bool allowDefaultsForReadonlyMembers)
        {
            var ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[0]);
            var il = ctor.GetILGenerator();
            CallBaseObjectConstructor(il);

            var result = CreateWrappingShimForThisWith(
                il,
                interfaceType,
                isFuzzy,
                allowDefaultsForReadonlyMembers);

            StoreShimInFieldWith(shimField, il, result);

            ImplementMethodReturnWith(il);
        }

        private void AddObjectWrappingConstructors(
            TypeBuilder typeBuilder,
            FieldBuilder shimField,
            Type interfaceType,
            bool isFuzzy,
            bool allowDefaultsForReadonlyMembers
        )
        {
            var paramTypes = new[]
            {
                new { pt = new[] { typeof(object[]) }, ctor = ShimConstructorForObjectArray },
                new { pt = new[] { typeof(object) }, ctor = ShimConstructorForObject }
            };
            foreach (var pt in paramTypes)
            {
                var ctorBuilder = typeBuilder.DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    pt.pt);
                var il = ctorBuilder.GetILGenerator();
                CallBaseObjectConstructor(il);
                var result = CreateWrappingShimForArg1With(
                    il,
                    interfaceType,
                    isFuzzy,
                    allowDefaultsForReadonlyMembers,
                    pt.ctor);
                StoreShimInFieldWith(shimField, il, result);
                ImplementMethodReturnWith(il);
            }
        }

        private void AddDictionaryWrappingConstructors(
            TypeBuilder typeBuilder,
            FieldBuilder shimField,
            Type interfaceType
        )
        {
            var paramTypes = new[]
            {
                new { pt = new[] { typeof(IDictionary<string, object>[]) }, ctor = DictionaryArrayShimConstructor },
                new { pt = new[] { typeof(IDictionary<string, object>) }, ctor = DictionaryShimConstructor }
            };
            foreach (var pt in paramTypes)
            {
                var ctorBuilder = typeBuilder.DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    pt.pt);
                var il = ctorBuilder.GetILGenerator();
                CallBaseObjectConstructor(il);
                var result = CreateWrappingDictionaryShimFor(il, interfaceType, pt.ctor);
                StoreShimInFieldWith(shimField, il, result);
                ImplementMethodReturnWith(il);
            }
        }

        private static LocalBuilder CreateWrappingDictionaryShimFor(
            ILGenerator il,
            Type interfaceType,
            ConstructorInfo ctor
        )
        {
            var result = il.DeclareLocal(typeof(IShimSham));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldtoken, interfaceType);
            // required for Mono to be happy
            il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Stloc, result);
            return result;
        }

        private static void StoreShimInFieldWith(FieldBuilder shimField, ILGenerator generator, LocalBuilder result)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Stfld, shimField);
        }

        private static LocalBuilder CreateWrappingShimForArg1With(
            ILGenerator il,
            Type interfaceType,
            bool isFuzzy,
            bool allowDefaultsForReadonlyMembers,
            ConstructorInfo ctor
        )
        {
            return CreateWrappingShimFor(
                OpCodes.Ldarg_1,
                il,
                interfaceType,
                isFuzzy,
                allowDefaultsForReadonlyMembers,
                ctor);
        }

        private static LocalBuilder CreateWrappingShimForThisWith(
            ILGenerator il,
            Type interfaceType,
            bool isFuzzy,
            bool allowDefaultsForReadonlyMembers)
        {
            return CreateWrappingShimFor(
                OpCodes.Ldarg_0,
                il,
                interfaceType,
                isFuzzy,
                allowDefaultsForReadonlyMembers,
                ShimConstructorForObject);
        }

        private static LocalBuilder CreateWrappingShimFor(
            OpCode code,
            ILGenerator il,
            Type interfaceType,
            bool isFuzzy,
            bool allowDefaultsForReadonlyMembers,
            ConstructorInfo shimConstructor)
        {
            var result = il.DeclareLocal(typeof(IShimSham));
            il.Emit(code);
            il.Emit(OpCodes.Ldtoken, interfaceType);
            // required for Mono to be happy
            il.Emit(OpCodes.Call, GetTypeFromHandleMethod);
            il.Emit(isFuzzy
                ? OpCodes.Ldc_I4_1
                : OpCodes.Ldc_I4_0);
            il.Emit(allowDefaultsForReadonlyMembers
                ? OpCodes.Ldc_I4_1
                : OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Newobj, shimConstructor);
            il.Emit(OpCodes.Stloc, result);
            return result;
        }

        private static readonly MethodInfo GetTypeFromHandleMethod =
            typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))
            ?? throw new InvalidOperationException($"Can't find method System.Type.GetTypeFromHandle");

        private static void CallBaseObjectConstructor(ILGenerator generator)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, ObjectConstructor);
        }

        private void AddAllMethodsAsShimmable(
            TypeBuilder typeBuilder,
            Type[] interfaceTypes,
            FieldBuilder shimField,
            bool forceNonVirtual
        )
        {
            var methodInfos = GetAllMethodsFor(interfaceTypes, forceNonVirtual);
            foreach (var methodInfo in methodInfos)
            {
                AddMethod(
                    interfaceTypes[0],
                    typeBuilder,
                    methodInfo,
                    shimField
                );
            }
        }

        private static bool MethodIsNotSpecial(MethodInfo mi)
        {
            return ((int) mi.Attributes & (int) MethodAttributes.SpecialName) != (int) MethodAttributes.SpecialName;
        }

        private static void AddMethod(
            Type interfaceType,
            TypeBuilder typeBuilder,
            MethodInfo methodInfo,
            FieldBuilder shimField)
        {
            if (!methodInfo.IsVirtualOrAbstract())
            {
                return;
            }

            var returnType = methodInfo.ReturnType;
            var parameterTypes = methodInfo.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();

            var methodBuilder = typeBuilder.DefineMethod(
                methodInfo.Name,
                PropertyGetterSetterMethodVirtualAttributes,
                returnType,
                parameterTypes
            );
            var il = methodBuilder.GetILGenerator();
            if (methodInfo.ReturnType == typeof(void))
            {
                ImplementVoidReturnCallThroughWith(methodInfo, shimField, il);
            }
            else
            {
                ImplementNonVoidReturnCallThroughWith(methodInfo, shimField, il);
            }

            ImplementMethodReturnWith(il);
            ImplementInterfaceMethodAsRequired(
                interfaceType,
                typeBuilder,
                methodInfo.Name,
                returnType,
                parameterTypes,
                methodBuilder);

            CopyCustomAttributes(methodInfo, methodBuilder);
        }

        private static void ImplementVoidReturnCallThroughWith(
            MethodInfo methodInfo,
            FieldBuilder shimField,
            ILGenerator il
        )
        {
            ImplementCallThroughWith(ShimCallThroughVoidMethod, methodInfo, shimField, il);
        }

        private static void ImplementNonVoidReturnCallThroughWith(
            MethodInfo methodInfo,
            FieldBuilder shimField,
            ILGenerator il
        )
        {
            ImplementCallThroughWith(ShimCallThroughMethod, methodInfo, shimField, il);
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

            il.Emit(OpCodes.Callvirt, shimMethod);
        }

        private static void LoadMethodArgumentsIntoArray(ILGenerator il,
            LocalBuilder boxedParameters,
            ParameterInfo[] methodParameters)
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
            Type[] types,
            FieldBuilder shimField,
            bool forceNonVirtual
        )
        {
            var allProperties = GetAllPropertiesFor(types, forceNonVirtual);
            foreach (var prop in allProperties)
            {
                AddShimmableProperty(types[0], typeBuilder, prop, shimField);
            }
        }

        private PropertyInfo[] GetAllPropertiesFor(
            Type[] types,
            bool forceNonVirtual
        )
        {
            return GetAllFor(
                    types,
                    t => t.GetProperties()
                )
                .Where(pi => forceNonVirtual || pi.IsVirtualOrAbstract())
                .Distinct(new PropertyInfoComparer())
                .ToArray();
        }

        private MethodInfo[] GetAllMethodsFor(
            Type[] types,
            bool forceNonVirtual
        )
        {
            return GetAllFor(
                types,
                t => t.GetMethods()
                    .Where(mi => forceNonVirtual || mi.IsVirtualOrAbstract())
                    .Where(MethodIsNotSpecial)
            );
        }

        private T[] GetAllFor<T>(Type[] interfaces, Func<Type, IEnumerable<T>> fetcher)
        {
            return interfaces
                .Select(fetcher)
                .SelectMany(a => a)
                .ToArray();
        }

        private void AddShimmableProperty(
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
                PropertyGetterSetterMethodVirtualAttributes, prop.PropertyType,
                Type.EmptyTypes);
            var il = getMethod.GetILGenerator();

            var local = StorePropertyValueInLocal(il, shimField, prop.Name);

            UnboxLocal(il, local, prop.PropertyType);
            il.Emit(OpCodes.Ret);

            ImplementInterfaceMethodAsRequired(
                interfaceType,
                typeBuilder,
                methodName,
                prop.PropertyType,
                new Type[0],
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
            il.Emit(OpCodes.Callvirt, ShimGetPropertyValueMethod);
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
            var setMethod = typeBuilder.DefineMethod(
                methodName,
                PropertyGetterSetterMethodVirtualAttributes,
                null, // void return
                new[] { prop.PropertyType } // one parameter of type of the property
            );
            var il = setMethod.GetILGenerator();

            var boxed = il.DeclareLocal(typeof(object));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Box, prop.PropertyType);
            il.Emit(OpCodes.Stloc, boxed);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, shimField);
            il.Emit(OpCodes.Ldstr, prop.Name);
            il.Emit(OpCodes.Ldloc, boxed);
            il.Emit(OpCodes.Callvirt, ShimSetPropertyValueMethod);

            ImplementMethodReturnWith(il);

            ImplementInterfaceMethodAsRequired(
                interfaceType,
                typeBuilder,
                methodName,
                prop.PropertyType,
                new Type[0],
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
            Type returnType,
            Type[] parameterTypes,
            MethodBuilder methodImplementation)
        {
            var interfaceMethod = TryFindInterfaceMethodMatching(
                interfaceType,
                methodName,
                returnType,
                parameterTypes);
            if (interfaceMethod != null)
            {
                typeBuilder.DefineMethodOverride(methodImplementation, interfaceMethod);
            }
        }

        private static MethodInfo TryFindInterfaceMethodMatching(
            Type interfaceType,
            string methodName,
            Type returnType,
            Type[] parameterTypes)
        {
            lock (MethodCache)
            {
                PopulateMethodCacheIfNecessaryFor(interfaceType);
                return MethodCache.TryGetValue(Tuple.Create(
                    interfaceType,
                    methodName,
                    returnType,
                    parameterTypes), out var result)
                    ? result
                    : null;
            }
        }

        private static void PopulateMethodCacheIfNecessaryFor(Type interfaceType)
        {
            if (TypesWithCachedMethods.Contains(interfaceType))
            {
                return;
            }

            foreach (var methodInfo in interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var key = Tuple.Create(
                    interfaceType,
                    methodInfo.Name,
                    methodInfo.ReturnType,
                    methodInfo.GetParameters().Select(p => p.ParameterType).ToArray()
                );
                MethodCache[key] = methodInfo;
            }

            TypesWithCachedMethods.Add(interfaceType);
        }

        private static readonly HashSet<Type> TypesWithCachedMethods = new HashSet<Type>();

        private static readonly Dictionary<Tuple<Type, string, Type, Type[]>, MethodInfo>
            MethodCache = new Dictionary<Tuple<Type, string, Type, Type[]>, MethodInfo>()
            {
            };

        private static AssemblyBuilder DynamicAssemblyBuilder
        {
            get
            {
                lock (DynamicAssemblyLock)
                {
                    return _dynamicAssemblyBuilderField ??=
                        DefineDynamicAssembly(ASSEMBLY_NAME);
                }
            }
        }

        private static AssemblyBuilder DefineDynamicAssembly(string withName)
        {
            return
#if NETSTANDARD
                AssemblyBuilder
                    .DefineDynamicAssembly(new AssemblyName(withName), AssemblyBuilderAccess.RunAndCollect);
#else
                AppDomain.CurrentDomain
                    .DefineDynamicAssembly(new AssemblyName(withName), AssemblyBuilderAccess.RunAndCollect);
#endif
        }
    }
}