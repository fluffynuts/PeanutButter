using System;
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators
{
  /// <summary>
  /// Some object extensions levering off of generic builders
  /// </summary>
  public static class ObjectExtensions
  {
    /// <summary>
    /// Provides a deep clone of the source object
    /// </summary>
    /// <param name="src">Object to clone</param>
    /// <typeparam name="T">Type of object to clone</typeparam>
    /// <returns>Cloned object</returns>
    public static T DeepClone<T>(this T src)
    {
      var builder = GenericBuilderLocator.GetGenericBuilderInstanceFor(typeof(T));
      var result = (T)builder.GenericDeepBuild();
      src.CopyPropertiesTo(result);
      return result;
    }
  }
}
