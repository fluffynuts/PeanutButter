using System;
using System.Collections.Generic;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides methods on lists as one might expect from JavaScript
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class ListExtensions
    {
        /// <summary>
        /// Removes the first item from the list and returns it
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="InvalidOperationException">Thrown when the list is empty</exception>
        /// <returns></returns>
        public static T Shift<T>(
            this IList<T> list
        )
        {
            ValidateListContainsElements(list);
            var result = list[0];
            list.RemoveAt(0);
            return result;
        }

        /// <summary>
        /// Removes the last item from the list and returns it
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="InvalidOperationException">Thrown when the list is empty</exception>
        /// <returns></returns>
        public static T Pop<T>(
            this IList<T> list
        )
        {
            ValidateListContainsElements(list);
            var idx = list.Count - 1;
            var result = list[idx];
            list.RemoveAt(idx);
            return result;
        }

        /// <summary>
        /// Attempt to pop the last element off of a list
        /// - Returns true and sets the result when the list has elements
        /// - Returns false if the list has no elements to pop
        /// </summary>
        /// <param name="list"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryPop<T>(
            this IList<T> list,
            out T result
        )
        {
            if (list.Count < 1)
            {
                result = default;
                return false;
            }

            var idx = list.Count - 1;
            result = list[idx];
            list.RemoveAt(idx);
            return true;
        }

        /// <summary>
        /// Attempt to shift the first element off of a list
        /// - Returns true and sets the result when the list has elements
        /// - Returns false if the list has no elements to shift
        /// </summary>
        /// <param name="list"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryShift<T>(
            this IList<T> list,
            out T result
        )
        {
            if (list.Count < 1)
            {
                result = default;
                return false;
            }

            result = list[0];
            list.RemoveAt(0);
            return true;
        }

        /// <summary>
        /// Inserts an item at the beginning of the list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static void Unshift<T>(
            this IList<T> list,
            T value
        )
        {
            list.Insert(0, value);
        }

        /// <summary>
        /// Alias for .Add: appends an item to the list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static void Push<T>(
            this IList<T> list,
            T value
        )
        {
            list.Add(value);
        }

        /// <summary>
        /// Adds the value to the list if the flag was set to true
        /// shortcut for:
        /// if (flag)
        /// {
        ///     list.Add(value);
        /// }
        /// </summary>
        /// <param name="list"></param>
        /// <param name="shouldAdd"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddIf<T>(
            this IList<T> list,
            bool shouldAdd,
            T value
        )
        {
            if (shouldAdd)
            {
                list.Add(value);
            }
        }

        /// <summary>
        /// Adds all the provided items and returns the list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<T> AddAll<T>(
            this IList<T> list,
            params T[] items
        )
        {
            if (list is List<T> lst)
            {
                // concrete list can do this more efficiently
                lst.AddRange(items);
            }
            else
            {
                foreach (var item in items)
                {
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// Attempts to eject the first matching 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="matcher"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryEjectFirst<T>(
            this IList<T> list,
            Func<T, bool> matcher,
            out T result
        )
        {
            return TryEject(
                list,
                matcher,
                findFirst: true,
                out result
            );
        }

        private static bool TryEject<T>(
            IList<T> list,
            Func<T, bool> matcher,
            bool findFirst,
            out T result
        )
        {
            result = default;
            var index = -1;
            var start = findFirst
                ? 0
                : list.Count - 1;
            var end = findFirst
                ? list.Count
                : -1;
            var i = start;
            Action moveNext = findFirst
                ? Increment
                : Decrement;
            Func<bool> check = findFirst
                ? CheckAfterIncrement
                : CheckAfterDecrement;

            for (i = start; check(); moveNext())
            {
                var current = list[i];
                if (matcher(current))
                {
                    result = current;
                    index = i;
                    break;
                }
            }

            if (index > -1)
            {
                list.RemoveAt(index);
                return true;
            }

            return false;

            void Increment()
            {
                i++;
            }

            void Decrement()
            {
                i--;
            }

            bool CheckAfterIncrement()
            {
                return i < end;
            }

            bool CheckAfterDecrement()
            {
                return i > end;
            }
        }

        /// <summary>
        /// Ejects the first matched item from the collection
        /// or throws if it cannot:
        /// - when no items to search, will throw InvalidOperationException
        /// - when not found, will throw NotFoundException
        /// </summary>
        /// <param name="list"></param>
        /// <param name="matcher"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T EjectFirst<T>(
            this IList<T> list,
            Func<T, bool> matcher
        )
        {
            return list.TryEjectFirst(matcher, out var result)
                ? result
                : throw FailToEjectErrorFor(list);
        }

        /// <summary>
        /// Tries to eject the first matched item from the collection
        /// </summary>
        /// <param name="list"></param>
        /// <param name="matcher"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool TryEjectLast<T>(
            this IList<T> list,
            Func<T, bool> matcher,
            out T result
        )
        {
            return TryEject<T>(
                list,
                matcher,
                findFirst: false,
                out result
            );
        }

        /// <summary>
        /// Ejects the first matched item from the collection
        /// or throws if it cannot:
        /// - when no items to search, will throw InvalidOperationException
        /// - when not found, will throw NotFoundException
        /// </summary>
        /// <param name="list"></param>
        /// <param name="matcher"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T EjectLast<T>(
            this IList<T> list,
            Func<T, bool> matcher
        )
        {
            return list.TryEjectLast<T>(
                matcher,
                out var result
            )
                ? result
                : throw FailToEjectErrorFor(list);
        }

        private static Exception FailToEjectErrorFor<T>(
            IList<T> list
        )
        {
            if (list is null)
            {
                return new InvalidOperationException("Sequence is null");
            }

            if (list.Count == 0)
            {
                return new InvalidOperationException("Sequence contains no elements");
            }

            return new NotFoundException(
                "Unable to eject value from sequence: value not found"
            );
        }

        private static void ValidateListContainsElements<T>(IList<T> list)
        {
            if (list.Count < 1)
            {
                throw new InvalidOperationException("List contains no elements");
            }
        }
    }
}

/// <summary>
/// Thrown when the caller expects to eject a message from
/// a non-empty collection but no match was found
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public NotFoundException(string message) : base(message)
    {
    }
}