using System;
using System.Collections.Generic;
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
        protected static readonly Type NullableGeneric = typeof (Nullable<>);

        /// <summary>
        /// Provides a lookup to the type which is the GenericBuilder
        /// </summary>
        protected static readonly Type GenericBuilderBaseType = typeof(GenericBuilder<,>);
        // ReSharper disable once InconsistentNaming
        private static readonly object _dynamicAssemblyLock = new object();
        private static AssemblyBuilder _dynamicAssemblyBuilderField;

        private static AssemblyBuilder DynamicAssemblyBuilder
        {
            get
            {
                lock (_dynamicAssemblyLock)
                {
                    return _dynamicAssemblyBuilderField ?? (_dynamicAssemblyBuilderField = DefineDynamicAssembly("PeanutButter.RandomGenerators.GeneratedBuilders"));
                }
            }
        }

        private static AssemblyBuilder DefineDynamicAssembly(string withName)
        {
            return AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(withName), AssemblyBuilderAccess.RunAndSave);
        }

        internal static Type FindOrGenerateDynamicBuilderFor(Type type)
        {
            if (type == null)
                return null;
            lock(DynamicBuilders)
            {
                Type dynamicBuilderType;
                if (DynamicBuilders.TryGetValue(type, out dynamicBuilderType))
                    return dynamicBuilderType;
                var t = typeof(GenericBuilder<,>);
                var moduleName = string.Join("_", Guid.NewGuid().ToString("N"), "DEBuilder_", type.Name);
                var modBuilder = DynamicAssemblyBuilder.DefineDynamicModule(moduleName);

                var typeName = string.Join("_", type.Name, "Builder", Guid.NewGuid().ToString("N"));
                var typeBuilder = modBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class);
                // Typebuilder is a sub class of Type
                typeBuilder.SetParent(t.MakeGenericType(typeBuilder, type));
                try
                {
                    dynamicBuilderType = typeBuilder.CreateType();
                }
                catch (TypeLoadException ex)
                {
                    throw new UnableToCreateDynamicBuilderException(type, ex);
                }
                DynamicBuilders[type] = dynamicBuilderType;
                return dynamicBuilderType;
            }
        }

    }
}