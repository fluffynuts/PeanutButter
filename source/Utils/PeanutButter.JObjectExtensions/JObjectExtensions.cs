using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace PeanutButter.JObjectExtensions
{
    public static class JObjectExtensions
    {
        private static readonly JTokenResolvers Resolvers = new JTokenResolvers
        {
            {JTokenType.None, o => null},
            {JTokenType.Array, ConvertJTokenArray},
            {JTokenType.Property, ConvertJTokenProperty},
            {JTokenType.Integer, o => o.Value<int>()},
            {JTokenType.String, o => o.Value<string>()},
            {JTokenType.Boolean, o => o.Value<bool>()},
            {JTokenType.Null, o => null},
            {JTokenType.Undefined, o => null},
            {JTokenType.Date, o => o.Value<DateTime>()},
            {JTokenType.Bytes, o => o.Value<byte[]>()},
            {JTokenType.Guid, o => o.Value<Guid>()},
            {JTokenType.Uri, o => o.Value<Uri>()},
            {JTokenType.TimeSpan, o => o.Value<TimeSpan>()},
            {JTokenType.Object, TryConvertObject}
        };

        // just because JsonCovert doesn't believe in using your provided type all the way down >_<
        public static Dictionary<string, object> ToDictionary(this JObject src)
        {
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (src == null)
                return result;
            foreach (var prop in src.Properties())
                result[prop.Name] = Resolvers[prop.Type](prop.Value);
            return result;
        }

        private static object TryConvertObject(JToken arg)
        {
            var asJObject = arg as JObject;
            if (asJObject != null)
                return asJObject.ToDictionary();
            return GetResolverFor(arg)(arg);
        }

        private static object PassThrough(JToken arg)
        {
            return arg;
        }

        private static Func<JToken, object> GetResolverFor(JToken arg)
        {
            return Resolvers.TryGetValue(arg.Type, out var result)
                ? result
                : PassThrough;
        }

        private static object ConvertJTokenProperty(JToken arg)
        {
            var resolver = GetResolverFor(arg);
            if (resolver == null)
                throw new InvalidOperationException($"Unable to handle JToken of type: {arg.Type}");
            return resolver(arg);
        }

        private static object ConvertJTokenArray(JToken arg)
        {
            var array = arg as JArray;
            if (array == null)
                throw new NotImplementedException();
            var result = new List<object>();
            foreach (var item in array)
            {
                result.Add(TryConvertObject(item));
            }
            var distinctType = FindSameTypeOf(result);
            return distinctType == null
                ? result.ToArray()
                : ConvertToTypedArray(result, distinctType);
        }

        private static Type FindSameTypeOf(IEnumerable<object> src)
        {
            var types = src.Select(o => o.GetType()).Distinct().ToArray();
            return types.Length == 1 ? types[0] : null;
        }

        private static object ConvertToTypedArray(IEnumerable<object> src, Type newType)
        {
            var method = ConvertToTypedArrayGenericMethod.MakeGenericMethod(newType);
            return method.Invoke(null, new object[] {src});
        }

        private static readonly MethodInfo ConvertToTypedArrayGenericMethod
            = typeof(JObjectExtensions).GetMethod(
                nameof(ConvertToTypedArrayGeneric),
                BindingFlags.NonPublic | BindingFlags.Static
            );

        private static T[] ConvertToTypedArrayGeneric<T>(IEnumerable<object> src)
        {
            return src.Cast<T>().ToArray();
        }

        private class JTokenResolvers : Dictionary<JTokenType, Func<JToken, object>>
        {
        }
    }
}