using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Useful extensions for IEnumerable&lt;T&gt; collections
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class ExtensionsForIEnumerables
    {
        /// <summary>
        /// The missing ForEach method
        /// </summary>
        /// <param name="collection">Subject collection to operate over</param>
        /// <param name="toRun">Action to run on each member of the collection</param>
        /// <typeparam name="T">Item type of the collection</typeparam>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> toRun)
        {
            foreach (var item in collection)
            {
                toRun(item);
            }
        }

        /// <summary>
        /// The missing ForEach method - synchronous variant which also provides the current item index
        /// </summary>
        /// <param name="collection">Subject collection to operate over</param>
        /// <param name="toRunWithIndex">Action to run on each member of the collection</param>
        /// <typeparam name="T">Item type of the collection</typeparam>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T, int> toRunWithIndex)
        {
            var idx = 0;
            collection.ForEach(o =>
            {
                toRunWithIndex(o, idx++);
            });
        }

        /// <summary>
        /// Calculates if two collections hold the same items, irrespective of order
        /// </summary>
        /// <param name="collection">Source collection</param>
        /// <param name="otherCollection">Collection to compare with</param>
        /// <typeparam name="T">Item type of the collections</typeparam>
        /// <returns>True if all values in the source collection are found in the target collection</returns>
        public static bool IsSameAs<T>(
            this IEnumerable<T> collection,
            IEnumerable<T> otherCollection)
        {
            if (collection == null && otherCollection == null)
            {
                return true;
            }

            if (collection == null || otherCollection == null)
            {
                return false;
            }

            var source = collection.ToArray();
            var target = otherCollection.ToArray();
            return source.Length == target.Length &&
                source.Aggregate(
                    true,
                    (state, item) => state && target.Contains(item)
                );
        }

        /// <summary>
        /// Fluent alternative to string.Join()
        /// </summary>
        /// <param name="collection">Source collection to operate on</param>
        /// <param name="joinWith">String to join items with</param>
        /// <typeparam name="T">Underlying type of the collection</typeparam>
        /// <returns>
        /// string representing items of the collection joined with the joinWith parameter.
        /// Where a collection of non-strings is provided, the objects' ToString() methods
        /// are used to get a string representation.
        /// </returns>
        public static string JoinWith<T>(
            this IEnumerable<T> collection,
            string joinWith)
        {
            var stringArray = collection as string[];
            if (stringArray != null)
            {
                return string.Join(joinWith, stringArray ?? new string[0]);
            }

            if (typeof(T) == typeof(string))
            {
                stringArray = collection.ToArray() as string[];
            }
            else
            {
                stringArray = collection.Select(i => i.ToString()).ToArray();
            }

            return string.Join(joinWith, stringArray ?? new string[0]);
        }

        /// <summary>
        /// Convenience method, essentially opposite to Any(), except
        /// that it also handles null collections
        /// </summary>
        /// <param name="collection">Source collection to operate on</param>
        /// <typeparam name="T">Item type of the collection</typeparam>
        /// <returns>True if the collection is null or has no items; false otherwise.</returns>
        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return !collection?.Any() ?? true;
        }

        /// <summary>
        /// Convenience method to mitigate null checking and errors when
        /// a null collection can be treated as if it were empty, eg:
        /// someCollection.EmptyIfNull().ForEach(DoSomething);
        /// </summary>
        /// <param name="collection">Source collection to operate over</param>
        /// <typeparam name="T">Item type of the collection</typeparam>
        /// <returns>An empty collection if the source is null; otherwise the source.</returns>
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> collection)
        {
            return collection ?? new T[0];
        }

        /// <summary>
        /// Convenience method to create a new array with the provided element(s) appended
        /// </summary>
        /// <param name="source">Source array to start with</param>
        /// <param name="toAdd">Item(s) to add to the result array</param>
        /// <typeparam name="T">Item type of the array</typeparam>
        /// <returns>A new array which is the source with the new items appended</returns>
        public static T[] And<T>(this IEnumerable<T> source, params T[] toAdd)
        {
            return source.Concat(toAdd).ToArray();
        }

        /// <summary>
        /// Convenience / fluent method to provide an array without the provided item(s)
        /// </summary>
        /// <param name="source">Source collection</param>
        /// <param name="toRemove">items which should not appear in the result array</param>
        /// <typeparam name="T">Item type of the array</typeparam>
        /// <returns>A new array of T with the specified items not present</returns>
        public static T[] ButNot<T>(this IEnumerable<T> source, params T[] toRemove)
        {
            return source.Except(toRemove).ToArray();
        }

        /// <summary>
        /// Convenience wrapper around SelectMany; essentially flattens a nested collection
        /// of collection(s) of some item. Exactly equivalent to:
        /// collection.SelectMany(o => o);
        /// </summary>
        /// <param name="collection">Source collection to operate on</param>
        /// <typeparam name="T">Item type of the collection</typeparam>
        /// <returns>A new, flat collection</returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> collection)
        {
            return collection.SelectMany(o => o);
        }

        /// <summary>
        /// Convenience method to get the results of a selection where the results are non-null
        /// -> this variant works on Nullable types
        /// </summary>
        /// <param name="collection">Source collection to operate over</param>
        /// <param name="grabber">Function to grab the data you're interested in off of each source item</param>
        /// <typeparam name="TCollection">Item type of the source collection</typeparam>
        /// <typeparam name="TResult">Item type of the result collection</typeparam>
        /// <returns>
        /// A new collection which is the result of a Select with the provided grabber
        /// where the Select results are non-null
        /// </returns>
        public static IEnumerable<TResult> SelectNonNull<TCollection, TResult>(
            this IEnumerable<TCollection> collection,
            Func<TCollection, TResult?> grabber) where TResult : struct
        {
            return collection
                .Select(grabber)
                .Where(i => i.HasValue)
                .Select(i => i.Value);
        }

        /// <summary>
        /// Convenience method to get the results of a selection where the results are non-null
        /// -> this variant works on types which can natively hold the value null
        /// </summary>
        /// <param name="collection">Source collection to operate over</param>
        /// <param name="grabber">Function to grab the data you're interested in off of each source item</param>
        /// <typeparam name="TCollection">Item type of the source collection</typeparam>
        /// <typeparam name="TResult">Item type of the result collection</typeparam>
        /// <returns>
        /// A new collection which is the result of a Select with the provided grabber
        /// where the Select results are non-null
        /// </returns>
        public static IEnumerable<TResult> SelectNonNull<TCollection, TResult>(
            this IEnumerable<TCollection> collection,
            Func<TCollection, TResult> grabber) where TResult : class
        {
            return collection
                .Select(grabber)
                .Where(i => i != null)
                .Select(i => i);
        }

        /// <summary>
        /// Convenience method to produce a block of text from a collection of items
        /// -> optionally, delimit with a string of your choice instead of a newline
        /// -> essentially a wrapper around JoinWith()
        /// </summary>
        /// <param name="input">Source input lines</param>
        /// <param name="delimiter">Optional delimiter (default is Environment.NewLine)</param>
        /// <typeparam name="T">Item type of collection</typeparam>
        /// <returns>String representation of the the items</returns>
        public static string AsText<T>(this IEnumerable<T> input, string delimiter = null)
        {
            return input.JoinWith(delimiter ?? Environment.NewLine);
        }

        /// <summary>
        /// Convenience method to test if a collection has a single item matching the
        /// provided matcher function
        /// </summary>
        /// <param name="input">Source collection</param>
        /// <param name="matcher">Function to run over each item to test if it passes</param>
        /// <typeparam name="T">Item type of the collection</typeparam>
        /// <returns>
        /// True if only one item in the collection got a true value from the matcher
        /// function; false if zero or more than one items were matched.
        /// </returns>
        public static bool HasUnique<T>(this IEnumerable<T> input, Func<T, bool> matcher)
        {
            var matches = input.Where(matcher);
            return matches.Count() == 1;
        }

        /// <summary>
        /// Fluency method to run an action a certain number of times, eg:
        /// 10.TimesDo(() => Console.WriteLine("Hello World"));
        /// </summary>
        /// <param name="howMany">Number of times to run the provided action</param>
        /// <param name="toRun">Action to run</param>
        public static void TimesDo(this int howMany, Action toRun)
        {
            howMany.TimesDo(i => toRun());
        }

        /// <summary>
        /// Fluency method to run an action a certain number of times. This
        /// variant runs on an action given the current index at each run, eg:
        /// 10.TimesDo(i => Console.WriteLine($"This action has run {i} times"));
        /// </summary>
        /// <param name="howMany">Number of times to run the provided action</param>
        /// <param name="toRun">Action to run</param>
        public static void TimesDo(this int howMany, Action<int> toRun)
        {
            if (howMany < 0)
                throw new ArgumentException("TimesDo must be called on positive integer", nameof(howMany));
            for (var i = 0; i < howMany; i++)
                toRun(i);
        }

        /// <summary>
        /// Convenience method to get the second item from a collection
        /// </summary>
        /// <param name="src">Source collection</param>
        /// <typeparam name="T">Item type of the collection</typeparam>
        /// <returns>The second item, when available. Will throw if there is no item available.</returns>
        public static T Second<T>(this IEnumerable<T> src)
        {
            return src.FirstAfter(1);
        }

        /// <summary>
        /// Convenience method to get the third item from a collection
        /// </summary>
        /// <param name="src">Source collection</param>
        /// <typeparam name="T">Item type of the collection</typeparam>
        /// <returns>The third item, when available. Will throw if there is no item available.</returns>
        public static T Third<T>(this IEnumerable<T> src)
        {
            return src.FirstAfter(2);
        }

        /// <summary>
        /// Convenience method to get the first item after skipping N items from a collection
        /// -> equivalent to collection.Skip(N).First();
        /// -> collection.FirtstAfter(2) returns the 3rd element
        /// </summary>
        /// <param name="src">Source collection</param>
        /// <param name="toSkip">How many items to skip</param>
        /// <typeparam name="T">Item type of the collection</typeparam>
        /// <returns>The third item, when available. Will throw if there is no item available.</returns>
        public static T FirstAfter<T>(this IEnumerable<T> src, int toSkip)
        {
            return src.Skip(toSkip).First();
        }

        /// <summary>
        /// Convenience method to get the first item after skipping N items from a collection
        /// -> equivalent to collection.Skip(N).First();
        /// -> collection.FirtstAfter(2) returns the 3rd element
        /// -> this variant returns the default value for T if the N is out of bounds
        /// </summary>
        /// <param name="src">Source collection</param>
        /// <param name="toSkip">How many items to skip</param>
        /// <typeparam name="T">Item type of the collection</typeparam>
        /// <returns>The third item, when available. Will return the default value for T otherwise.</returns>
        public static T FirstOrDefaultAfter<T>(this IEnumerable<T> src, int toSkip)
        {
            return src.Skip(toSkip).FirstOrDefault();
        }

        /// <summary>
        /// Find duplicates within a collectio according to a provided discriminator
        /// </summary>
        /// <param name="src">Collection to operate on</param>
        /// <typeparam name="TItem">Type of items in the collection</typeparam>
        /// <returns>Collection of duplicate items</returns>
        public static IEnumerable<TItem> FindDuplicates<TItem>(
            this IEnumerable<TItem> src
        )
        {
            return src.FindDuplicates(x => x).Select(o => o.Key);
        }

        /// <summary>
        /// Find duplicates within a collectio according to a provided discriminator
        /// </summary>
        /// <param name="src">Collection to operate on</param>
        /// <param name="descriminator">Function to determine uniqueness of each item: should
        /// return whatever identifies a particular item uniquely</param>
        /// <typeparam name="TItem">Type of items in the collection</typeparam>
        /// <typeparam name="TKey">Type of key used to discriminate items</typeparam>
        /// <returns>Collection of DuplicateResult items which contain duplicates, according to the provided discriminator</returns>
        public static IEnumerable<DuplicateResult<TKey, TItem>> FindDuplicates<TItem, TKey>(
            this IEnumerable<TItem> src,
            Func<TItem, TKey> descriminator
        )
        {
            return src.GroupBy(descriminator)
                .Where(g => g.Count() > 1)
                .Select(g => new DuplicateResult<TKey, TItem>(g.Key, g.AsEnumerable()));
        }

        /// <summary>
        /// Inverse of All() LINQ method: test should return false for all elements
        /// </summary>
        /// <param name="collection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool None<T>(
            this IEnumerable<T> collection
        )
        {
            return !collection?.Any() ?? true;
        }

        /// <summary>
        /// Inverse of All() LINQ method: test should return false for all elements
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="test"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool None<T>(
            this IEnumerable<T> collection,
            Func<T, bool> test)
        {
            return (collection ?? new T[0]).All(o => !test(o));
        }


        /// <summary>
        /// DTO for conveying results from the more complex FindDuplicates
        /// variant which includes a key discriminator
        /// </summary>
        /// <typeparam name="TKey">Type of the key that duplication was determined by</typeparam>
        /// <typeparam name="TItem">Type of the duplicated item(s)</typeparam>
        public class DuplicateResult<TKey, TItem>
        {
            /// <summary>
            /// Key of duplication
            /// </summary>
            public TKey Key { get; }

            /// <summary>
            /// Duplicated items matching this key
            /// </summary>
            public IEnumerable<TItem> Items { get; }

            /// <summary>
            /// Constructs a read-only dto
            /// </summary>
            /// <param name="key">Key value</param>
            /// <param name="items">Duplicated items</param>
            public DuplicateResult(TKey key, IEnumerable<TItem> items)
            {
                Key = key;
                Items = items;
            }
        }

        /// <summary>
        /// Performs implicit casting on a collection
        /// -> just like .Cast&lt;T&gt;, this will explode if the
        ///     cast cannot succeed. C'est la vie
        /// </summary>
        /// <param name="collection"></param>
        /// <typeparam name="TOther"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TOther> ImplicitCast<TOther>(
            this IEnumerable collection)
        {
            MethodInfo implicitOp = null;

            foreach (var item in collection)
            {
                var op = ResolveImplicitOperator(item.GetType());
                yield return (TOther) op.Invoke(null, new object[] { item });
            }

            MethodInfo ResolveImplicitOperator(Type inputType)
            {
                if (implicitOp != null)
                {
                    return implicitOp;
                }

                var otherType = typeof(TOther);

                var candidates = otherType
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(mi => mi.Name == "op_Implicit" &&
                        mi.ReturnType == otherType
                    );
                foreach (var candidate in candidates)
                {
                    var parameters = candidate.GetParameters();
                    if (parameters.Length != 1)
                    {
                        continue;
                    }

                    var parameterType = parameters[0].ParameterType;
                    if (parameterType.IsAssignableFrom(inputType))
                    {
                        return implicitOp = candidate;
                    }
                }

                throw new InvalidCastException(
                    $"No implicit operator found on {otherType} to get an instance of {inputType}"
                );
            }
        }

        /// <summary>
        /// Similar to LINQ's Zip extension method, this will zip
        /// two enumerables together using yield
        /// - however it will throw an exception if one enumerable
        /// runs out before the other
        /// </summary>
        /// <param name="left">left collection</param>
        /// <param name="right">right collection</param>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <returns>A new collection of Tuple&lt;TLeft, TRight&gt;</returns>
        public static IEnumerable<Tuple<TLeft, TRight>> StrictZip<TLeft, TRight>(
            this IEnumerable<TLeft> left,
            IEnumerable<TRight> right)
        {
            return left.StrictZip(right, Tuple.Create);
        }

        /// <summary>
        /// Similar to LINQ's Zip extension method, this will zip
        /// two enumerables together using yield
        /// - however it will throw an exception if one enumerable
        /// runs out before the other
        /// </summary>
        /// <param name="left">left collection</param>
        /// <param name="right">right collection</param>
        /// <param name="generator">generator function to produce each item of TResult</param>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns>A new collection of TResult, as determined by your generator function</returns>
        public static IEnumerable<TResult> StrictZip<TLeft, TRight, TResult>(
            this IEnumerable<TLeft> left,
            IEnumerable<TRight> right,
            Func<TLeft, TRight, TResult> generator)

        {
            if (left is null || right is null)
            {
                throw new CannotZipNullException();
            }

            using var leftEnumerator = left.GetEnumerator();
            using var rightEnumerator = right.GetEnumerator();
            while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
            {
                yield return generator(leftEnumerator.Current, rightEnumerator.Current);
            }

            if (leftEnumerator.MoveNext() || rightEnumerator.MoveNext())
            {
                throw new UnevenZipException<TLeft, TRight>(left, right);
            }
        }

        /// <summary>
        /// Performs full-collection matching on two collections of the same type,
        /// assuming that .Equals() is a valid comparator between two objects of type T
        /// </summary>
        /// <param name="left">left collection</param>
        /// <param name="right">right collection</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// true if collections are of the same size and each item, in order,
        /// from the left item, matches the right one
        /// </returns>
        public static bool Matches<T>(
            this IEnumerable<T> left,
            IEnumerable<T> right
        )
        {
            return left.Matches(right, (a, b) =>
            {
                if (a is null && b is null)
                {
                    return true;
                }

                if (a is null || b is null)
                {
                    return false;
                }

                return a.Equals(b);
            });
        }

        /// <summary>
        /// Performs matching on collections of the same type
        /// </summary>
        /// <param name="left">left collection</param>
        /// <param name="right">right collection</param>
        /// <param name="comparer">function used to compare two values</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// true if collections are of the same size and each item, in order,
        /// from the left item, matches the right one
        /// </returns>
        public static bool Matches<T>(
            this IEnumerable<T> left,
            IEnumerable<T> right,
            Func<T, T, bool> comparer)
        {
            return left.CrossMatches<T, T>(right, comparer);
        }

        /// <summary>
        /// Performs cross-type matching on collections
        /// </summary>
        /// <param name="left">left collection</param>
        /// <param name="right">right collection</param>
        /// <param name="comparer">function to compare items</param>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <returns>
        /// true if collections are of the same size and each item, in order,
        /// from the left item, matches the right one
        /// </returns>
        public static bool CrossMatches<TLeft, TRight>(
            this IEnumerable<TLeft> left,
            IEnumerable<TRight> right,
            Func<TLeft, TRight, bool> comparer
        )
        {
            if (left is null && right is null)
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            try
            {
                return left.StrictZip(right)
                    .All(item =>
                        comparer(
                            item.Item1,
                            item.Item2
                        )
                    );
            }
            catch (UnevenZipException)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the original collection of strings trimmed
        /// - will handle null input as if it were an empty collection
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<string> Trim(
            this IEnumerable<string> source
        )
        {
            foreach (var item in source ?? new string[0])
            {
                yield return item?.Trim();
            }
        }

        /// <summary>
        /// Returns the original collection of strings trimmed at the start
        /// - will handle null input as if it were an empty collection
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<string> TrimStart(
            this IEnumerable<string> source
        )
        {
            foreach (var item in source ?? new string[0])
            {
                yield return item?.TrimStart();
            }
        }

        /// <summary>
        /// Returns the original collection of strings trimmed at the start
        /// - will handle null input as if it were an empty collection
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<string> TrimEnd(
            this IEnumerable<string> source
        )
        {
            foreach (var item in source ?? new string[0])
            {
                yield return item?.TrimEnd();
            }
        }


        /// <summary>
        /// Returns a copy of the input strings where
        /// all are padded to the left with spaces to fit
        /// to the longest item in the collection
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<string> PadLeft<T>(
            this IEnumerable<T> source
        )
        {
            return source.PadLeft(' ');
        }

        /// <summary>
        /// Returns a copy of the input strings where
        /// all are padded to the left with the `padWith`
        /// char to fit to the longest item in the collection
        /// </summary>
        /// <param name="source"></param>
        /// <param name="padWith"></param>
        /// <returns></returns>
        public static IEnumerable<string> PadLeft<T>(
            this IEnumerable<T> source,
            char padWith
        )
        {
            var asArray = source is T[] arr
                ? arr
                : source?.ToArray() ?? new T[0];
            var padChars = typeof(T) == typeof(string)
                ? asArray.Cast<string>().Select(s => s?.Length ?? 0).Max()
                : asArray.Select(s => $"{s}".Length).Max();
            return asArray.PadLeft(padChars, padWith);
        }

        /// <summary>
        /// Returns a copy of the input strings where
        /// all are padded to the left to the provided
        /// required length with spaces
        /// </summary>
        /// <param name="source"></param>
        /// <param name="requiredLength"></param>
        /// <returns></returns>
        public static IEnumerable<string> PadLeft<T>(
            this IEnumerable<T> source,
            int requiredLength
        )
        {
            return source.PadLeft(requiredLength, ' ');
        }

        /// <summary>
        /// Returns a copy of the input strings where
        /// all are padded to the left to the provided
        /// required length with the provided padWith
        /// character
        /// </summary>
        /// <param name="source"></param>
        /// <param name="requiredLength"></param>
        /// <param name="padWith"></param>
        /// <returns></returns>
        public static IEnumerable<string> PadLeft<T>(
            this IEnumerable<T> source,
            int requiredLength,
            char padWith
        )
        {
            if (typeof(T) == typeof(string))
            {
                var asStringArray = source as string[] ?? new string[0];
                foreach (var item in asStringArray)
                {
                    yield return item is null
                        ? new String(padWith, requiredLength)
                        : item?.PadLeft(requiredLength, padWith);
                }
                yield break;
            }

            foreach (var item in source ?? new T[0])
            {
                yield return $"{item}".PadLeft(requiredLength, padWith);
            }
        }

        /// <summary>
        /// Returns a copy of the input strings where
        /// all are padded to the right with spaces to fit
        /// to the longest item in the collection
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<string> PadRight<T>(
            this IEnumerable<T> source
        )
        {
            return source.PadRight(' ');
        }

        /// <summary>
        /// Returns a copy of the input strings where
        /// all are padded to the right with the `padWith`
        /// char to fit to the longest item in the collection
        /// </summary>
        /// <param name="source"></param>
        /// <param name="padWith"></param>
        /// <returns></returns>
        public static IEnumerable<string> PadRight<T>(
            this IEnumerable<T> source,
            char padWith
        )
        {
            var asArray = source is T[] arr
                ? arr
                : source?.ToArray() ?? new T[0];
            var padChars = typeof(T) == typeof(string)
                ? (asArray as string[])!.Select(s => s?.Length ?? 0).Max()
                : asArray.Select(s => $"{s}".Length).Max();
            return asArray.PadRight(padChars, padWith);
        }

        /// <summary>
        /// Returns a copy of the input strings where
        /// all are padded to the right with spaces
        /// char to fit to the requiredLength
        /// </summary>
        /// <param name="source"></param>
        /// <param name="requiredLength"></param>
        /// <returns></returns>
        public static IEnumerable<string> PadRight<T>(
            this IEnumerable<T> source,
            int requiredLength
        )
        {
            return source.PadRight(requiredLength, ' ');
        }

        /// <summary>
        /// Returns a copy of the input strings where
        /// all are padded to the right with the `padWith`
        /// char to fit to the requiredLength
        /// </summary>
        /// <param name="source"></param>
        /// <param name="requiredLength"></param>
        /// <param name="padWith"></param>
        /// <returns></returns>
        public static IEnumerable<string> PadRight<T>(
            this IEnumerable<T> source,
            int requiredLength,
            char padWith
        )
        {
            if (typeof(T) == typeof(string))
            {
                var stringArray = source as string[] ?? new string[0];
                foreach (var item in stringArray)
                {
                    yield return item is null
                        ? new String(padWith, requiredLength)
                        : item.PadRight(requiredLength, padWith);
                }
                
                yield break;
            }
            foreach (var item in source ?? new T[0])
            {
                yield return $"{item}".PadRight(requiredLength, padWith);
            }
        }
    }
}