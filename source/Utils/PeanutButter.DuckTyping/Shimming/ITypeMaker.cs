using System;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Shimming
#else
namespace PeanutButter.DuckTyping.Shimming
#endif
{
    /// <summary>
    /// Interface implemented by the TypeMaker
    /// </summary>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    internal
#else
    public
#endif
    interface ITypeMaker
    {
        /// <summary>
        /// Attempts to create a type implementing interface T or deriving from abstract / all-virtual type T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction
        /// </summary>
        /// <typeparam name="T">Interface to implement</typeparam>
        /// <returns>A new type implementing interface T</returns>
        Type MakeTypeImplementing<T>();

        /// <summary>
        /// Attempts to create a type implementing interface T or deriving from abstract / all-virtual type T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction
        /// </summary>
        /// <typeparam name="T">Interface to implement</typeparam>
        /// <param name="forceConcrete">Force a concrete type for T</param>
        /// <returns>A new type implementing interface T</returns>
        Type MakeTypeImplementing<T>(bool forceConcrete);

        /// <summary>
        /// Attempts to create a type implementing interface T or deriving from abstract / all-virtual type T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction
        /// </summary>
        /// <param name="type">Type to implement</param>
        /// <returns>A new type implementing interfaceType</returns>
        Type MakeTypeImplementing(Type type);
        
        /// <summary>
        /// Attempts to create a type implementing interface T or deriving from abstract / all-virtual type T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction
        /// </summary>
        /// <param name="type">Type to implement</param>
        /// <param name="forceConcrete">Force a concrete type for T</param>
        /// <returns>A new type implementing interfaceType</returns>
        Type MakeTypeImplementing(Type type, bool forceConcrete);

        /// <summary>
        /// Attempts to create a type implementing interface T or deriving from abstract / all-virtual type T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction for approximate duck-typing (which also
        /// supports accurate duck-typing)
        /// </summary>
        /// <typeparam name="T">Interface to implement</typeparam>
        /// <returns>A new type implementing interface T</returns>
        Type MakeFuzzyTypeImplementing<T>();
        
        /// <summary>
        /// Attempts to create a type implementing interface T or deriving from abstract / all-virtual type T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction for approximate duck-typing (which also
        /// supports accurate duck-typing)
        /// </summary>
        /// <param name="forceConcrete">force a concrete type for T</param>
        /// <typeparam name="T">Interface to implement</typeparam>
        /// <returns>A new type implementing interface T</returns>
        Type MakeFuzzyTypeImplementing<T>(bool forceConcrete);

        /// <summary>
        /// Attempts to create a type implementing interface T or deriving from abstract / all-virtual type T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction for approximate duck-typing (which also
        /// supports accurate duck-typing)
        /// </summary>
        /// <param name="type">Interface to implement</param>
        /// <returns>A new type implementing interfaceType</returns>
        Type MakeFuzzyTypeImplementing(Type type);

        /// <summary>
        /// Attempts to create a type implementing interface T or deriving from abstract / all-virtual type T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction for approximate duck-typing (which also
        /// supports accurate duck-typing) where missing properties on the ducked source
        /// are readable as the default value for that property's type
        /// </summary>
        /// <returns>A new type implementing interface T</returns>
        Type MakeFuzzyDefaultingTypeImplementing<T>();
        
        /// <summary>
        /// Attempts to create a type implementing interface T or deriving from abstract / all-virtual type T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction for approximate duck-typing (which also
        /// supports accurate duck-typing) where missing properties on the ducked source
        /// are readable as the default value for that property's type
        /// </summary>
        /// <param name="forceConcrete">Force a concrete type for T</param>
        /// <returns>A new type implementing interface T</returns>
        public Type MakeFuzzyDefaultingTypeImplementing<T>(bool forceConcrete);

        /// <summary>
        /// Attempts to create a type implementing interface T or deriving from abstract / all-virtual type T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction for approximate duck-typing (which also
        /// supports accurate duck-typing) where missing properties on the ducked source
        /// are readable as the default value for that property's type
        /// </summary>
        /// <param name="type">Interface to implement</param>
        /// <returns>A new type implementing interfaceType</returns>
        Type MakeFuzzyDefaultingTypeImplementing(Type type);
        
        /// <summary>
        /// Attempts to create a type implementing interface T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction for approximate duck-typing (which also
        /// supports accurate duck-typing) where missing properties on the ducked source
        /// are readable as the default value for that property's type. Will force
        /// concrete types with non-virtual members (use at own peril)
        /// </summary>
        /// <param name="type">type to generate a derivative / implementation of</param>
        /// <param name="forceConcreteType">forces type generation even when the target has non-virtual members</param>
        /// <returns></returns>
        Type MakeFuzzyDefaultingTypeImplementing(Type type, bool forceConcreteType);

        /// <summary>
        /// Attempts to create a type implementing interface T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction for approximate duck-typing (which also
        /// supports accurate duck-typing). Will force creating derivatives of concrete
        /// types with non-virtual members (use at own peril)
        /// </summary>
        /// <param name="type">Interface to implement</param>
        /// <param name="forceConcrete">forces type generation even when the target has non-virtual members</param>
        /// <returns>A new type implementing interfaceType</returns>
        public Type MakeFuzzyTypeImplementing(Type type, bool forceConcrete);

    }
}