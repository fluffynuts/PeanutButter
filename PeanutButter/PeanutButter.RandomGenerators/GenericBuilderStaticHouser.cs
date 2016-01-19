using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PeanutButter.RandomGenerators
{
    public abstract class GenericBuilderStaticHouser
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
                    return _dynamicAssemblyBuilderField ?? (_dynamicAssemblyBuilderField = DefineDynamicAssembly("DynamicEntityBuilders"));
                }
            }
        }

        private static AssemblyBuilder DefineDynamicAssembly(string withName)
        {
            return AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(withName), AssemblyBuilderAccess.RunAndSave);
        }

    }
}