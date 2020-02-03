using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.DuckTyping.Shimming;

namespace DuckTypingInDotnetCore3
{
    public interface IHasId
    {
        int Id { get; set; }
    }

    public class Duckable
    {
        public int Id { get; set; }
    }

    public abstract class ShimBase
    {
    }

    public interface IShim
    {
        object GetPropertyValue(string name);
        void SetPropertyValue(string name, object value);
    }

    public class Shim: ShimBase, IShim
    {
        
        private readonly Dictionary<string, object> _values
            = new Dictionary<string, object>();
        public void SetPropertyValue(
            string name,
            object value)
        {
            _values[name] = value;
        }

        public object GetPropertyValue(
            string name
        )
        {
            return _values[name];
        }
    }

    class Program
    {
        static int Main(string[] args)
        {
            var localShim = new Shim();
            var setter = typeof(IShim).GetMethod(nameof(IShim.SetPropertyValue));
            var getter = typeof(IShim).GetMethod(nameof(IShim.GetPropertyValue));
            setter.Invoke(localShim, new object[] { "__moo__", 123 });
            var result = getter.Invoke(localShim, new object[] { "__moo__" });
            ReplicateILGeneration();
            ReplicateViaImportedType();
            
            var subject = new Duckable()
            {
                Id = 123
            };
            var ducked = subject.DuckAs<IHasId>();
            ducked.Id = 123;
            // var foo = ducked.Id;
            // Console.WriteLine($"id: {ducked.Id}");

            return 0;
        }

        private static void ReplicateViaImportedType()
        {
            var instance = TestIdea.MakeFoo();
            var type = instance.GetType();
            var setter = type.GetMethod("set_Id");
            setter.Invoke(instance, new object[] { 42 });
            var getter = type.GetMethod("get_Id");
            var result = getter.Invoke(instance, new object[0]);

            Console.WriteLine($"Stored and retrieved: {result}");
        }

        private static void ReplicateILGeneration()
        {
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("__foo__"), AssemblyBuilderAccess.Run
            );
            var modBuilder = asmBuilder.DefineDynamicModule("__bar__");
            var typeBuilder = modBuilder.DefineType("__my_type__", TypeAttributes.Class | TypeAttributes.Public);
            // define field
            var field = typeBuilder.DefineField(
                "_id",
                typeof(object),
                FieldAttributes.Private);
            // define setter
            var propertyMethodAttributes = MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.Virtual |
                MethodAttributes.HideBySig;

            var setMethod = typeBuilder.DefineMethod(
                "set_Id",
                propertyMethodAttributes,
                null,
                new[] { typeof(int) }
            );
            var il = setMethod.GetILGenerator();
            var shimSetter = typeof(Shim)
                .GetMethod(nameof(IShim.SetPropertyValue));
            var boxed = il.DeclareLocal(typeof(object));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Box, typeof(IShim));
            il.Emit(OpCodes.Stloc, boxed);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Ldstr, "_id_");
            il.Emit(OpCodes.Ldloc, boxed);
            il.Emit(OpCodes.Call, shimSetter);
            il.Emit(OpCodes.Ret);

            // - end as per pb


            // define getter
            var getMethod = typeBuilder.DefineMethod(
                "get_Id",
                propertyMethodAttributes,
                typeof(int),
                new Type[0]
            );
            var shimGetter = typeof(Shim)
                .GetMethod(nameof(IShim.GetPropertyValue));

            il = getMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // this
            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Ldstr, "_id_");
            il.Emit(OpCodes.Call, shimGetter);
            il.Emit(OpCodes.Ret);

            var type = typeBuilder.CreateTypeInfo();
            var instance = Activator.CreateInstance(type);
            var shimField = type.GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic);
            shimField.SetValue(instance, new Shim());
            
            
            var setter = type.GetMethod("set_Id");
            setter.Invoke(instance, new object[] { 42 });
            var getter = type.GetMethod("get_Id");
            var result = getter.Invoke(instance, new object[0]);

            Console.WriteLine($"Stored and retrieved: {result}");
        }
    }
}