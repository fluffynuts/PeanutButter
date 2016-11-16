using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PeanutButter.RandomGenerators
{
    public abstract class GenericBuilderBase
    {
        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public static int MaxRandomPropsLevel { get; set; } = 10;
        protected static readonly Dictionary<Type, Type> DynamicBuilders = new Dictionary<Type, Type>();
        protected static readonly Dictionary<Type, Type> UserBuilders = new Dictionary<Type, Type>(); 

        protected static readonly Type NullableGeneric = typeof (Nullable<>);

        protected static readonly Type[] CollectionGenerics =
        {
            typeof(ICollection<>),
            typeof(IEnumerable<>),
            typeof(List<>),
            typeof(IList<>),
            typeof(IDictionary<,>),
            typeof(Dictionary<,>)
        };

        protected static readonly Type GenericBuilderBaseType = typeof(GenericBuilder<,>);
        private static readonly object DynamicAssemblyLock = new object();
        private static AssemblyBuilder _dynamicAssemblyBuilderField;
        protected static AssemblyBuilder DynamicAssemblyBuilder
        {
            get
            {
                lock (DynamicAssemblyLock)
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
                var moduleName = string.Join("_", "DynamicEntityBuilders", type.Name, Guid.NewGuid().ToString("N"));
                var modBuilder = DynamicAssemblyBuilder.DefineDynamicModule(moduleName);

                var typeBuilder = modBuilder.DefineType(type.Name + "Builder", TypeAttributes.Public | TypeAttributes.Class);
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