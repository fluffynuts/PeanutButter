using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Abstract base class for housing shared logic between all builders and
    /// allowing a base, unconstructable class to use to reference a collection
    /// of builders
    /// </summary>
    public abstract class GenericBuilderBase
    {
        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        /// <summary>
        /// Sets the maximum level to go to when generating random properties of
        /// random properties, to prevent stack and memory overflows
        /// </summary>
        public static int MaxRandomPropsLevel { get; set; } = 10;

        /// <summary>
        /// Holds a lookup of all GenericBuilder classes which have been generated
        /// to facilitate automatic property building. Searched when looking for a builder
        /// to generate a property before attempting to generate a builder type (re-use)
        /// </summary>
        protected static readonly Dictionary<Type, Type> DynamicBuilders = new Dictionary<Type, Type>();

        /// <summary>
        /// Holds a lookup of GenericBuilder classes which were provided by consuming
        /// code. Searched before searching the DynamicBuilders lookup or attempting to create
        /// an auto-generated builder type.
        /// </summary>
        protected static readonly Dictionary<Type, Type> UserBuilders = new Dictionary<Type, Type>();

        /// <summary>
        /// Provides a lookup to the type which is the generic Nulllable
        /// </summary>
        protected static readonly Type NullableGeneric = typeof(Nullable<>);

        /// <summary>
        /// Provides a lookup to the type which is the GenericBuilder
        /// </summary>
        protected static readonly Type GenericBuilderBaseType = typeof(GenericBuilderBase);

        // ReSharper disable once InconsistentNaming
        private static readonly object _dynamicAssemblyLock = new object();

        private static AssemblyBuilder _dynamicAssemblyBuilderField;

        private static AssemblyBuilder DynamicAssemblyBuilder
        {
            get
            {
                lock (_dynamicAssemblyLock)
                {
                    return _dynamicAssemblyBuilderField ??=
                        DefineDynamicAssembly("PeanutButter.RandomGenerators.GeneratedBuilders");
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

        private static ModuleBuilder _moduleBuilder;
        private static readonly object ModuleBuilderLock = new object();

        private static ModuleBuilder CreateOrReuseDynamicModule()
        {
            lock (ModuleBuilderLock)
            {
                return _moduleBuilder ??=
                    DynamicAssemblyBuilder.DefineDynamicModule("GeneratedBuilders");
            }
        }

        internal static Type ReuseOrGenerateDynamicBuilderFor(Type type)
        {
            if (type == null)
                return null;
            lock (DynamicBuilders)
            {
                if (DynamicBuilders.TryGetValue(type, out var dynamicBuilderType))
                    return dynamicBuilderType;
                var t = typeof(GenericBuilder<,>);

                var modBuilder = CreateOrReuseDynamicModule();
                var typeName = string.Join("_", type.Name, "Builder", Guid.NewGuid().ToString("N"));
                var typeBuilder = modBuilder.DefineType(typeName,
                    TypeAttributes.Public | TypeAttributes.Class);
                // TypeBuilder is a sub class of Type
                typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
                typeBuilder.SetParent(t.MakeGenericType(typeBuilder, type));
                try
                {
                    dynamicBuilderType = typeBuilder
#if NETSTANDARD
                        .CreateTypeInfo().AsType();
#else
                        .CreateType();
#endif
                }
                catch (TypeLoadException ex)
                {
                    throw new UnableToCreateDynamicBuilderException(type, ex);
                }

                DynamicBuilders[type] = dynamicBuilderType;
                return dynamicBuilderType;
            }
        }

        /// <summary>
        /// Attempt to create a substitute for the given type
        /// </summary>
        /// <param name="callThrough">Create a partial sub where calls go through to the original implementation</param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryCreateSubstituteFor<T>(bool callThrough, out T result)
        {
            return TryCreateSubstituteFor(
                throwOnError: false,
                callThrough,
                out result
            );
        }

        /// <summary>
        /// Attempts perform Substitute.For&lt;T&gt;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool TryCreateSubstituteFor<T>(
            bool throwOnError,
            bool callThrough,
            out T result
        )
        {
            result = default;
            var loadedNSubstitute = FindOrLoadNSubstitute<T>();
            if (loadedNSubstitute is null)
            {
                if (throwOnError)
                {
                    throw new Exception("Can't find (or load) NSubstitute )':");
                }

                return false;
            }

            // FIXME: calling .Received() on the result doesn't work
            // unless NSubstitute has been invoked in the calling assembly
            // eg with a pre-existing Substitute.For or by reading SubstitutionContext.Current
            var nsubstituteTypes = loadedNSubstitute.GetTypes();
            var subType = nsubstituteTypes
                .FirstOrDefault(t => t.Name == "Substitute");
            if (subType is null)
            {
                if (throwOnError)
                {
                    throw new Exception(
                        "NSubstitute assembly loaded -- but no Substitute class? )':"
                    );
                }

                return false;
            }

            var seekMethod = callThrough
                ? "ForPartsOf"
                : "For";

            var genericMethod = subType.GetMethods()
                .FirstOrDefault(m => m.Name == seekMethod &&
                    IsObjectParams(m.GetParameters())
                );
            if (genericMethod is null)
            {
                if (throwOnError)
                {
                    throw new Exception(
                        $"Can't find NSubstitute.Substitute.{seekMethod} method )':"
                    );
                }

                return false;
            }

            var specificMethod = genericMethod.MakeGenericMethod(typeof(T));
            try
            {
                result = (T) specificMethod.Invoke(
                    null,
                    new object[]
                    {
                        new object[] { }
                    });
                return true;
            }
            catch
            {
                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
        }

        private static Assembly FindOrLoadNSubstitute<T>()
        {
            return FindOrLoadAssembly<T>("NSubstitute", false);
        }

        /// <summary>
        /// Attempts to load the assembly alongside the Type T
        /// </summary>
        /// <param name="name"></param>
        /// <param name="retrying"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected static Assembly FindOrLoadAssembly<T>(
            string name,
            bool retrying
        )
        {
            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(
                    a => a.GetName().Name == name
                );
            if (loaded != null ||
                retrying)
            {
                return loaded;
            }

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
            return parameterInfos.Length == 1 &&
                parameterInfos[0]
                    .ParameterType ==
                typeof(object[]);
        }
    }
}