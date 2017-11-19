using System;
using System.Linq;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    internal static class PropertyOrFieldExtensions
    {
        public static bool IsAssignmentMatchFor(
            this PropertyOrField master,
            PropertyOrField other
        )
        {
            return master.IsAssignmentMatchFor(other, true);
        }

        public static bool IsAssignmentMatchFor(
            this PropertyOrField master,
            PropertyOrField other,
            bool mustMatchMemberType
        )
        {
#pragma warning disable S1067
            return master.Name == other.Name &&
                   other.Type.IsAssignableOrUpcastableTo(master.Type) &&
                   master.AccessMatches(other) &&
                   (!mustMatchMemberType || master.MemberType == other.MemberType);
#pragma warning restore S1067
        }

        public static bool IsMatchFor(
            this PropertyOrField master, 
            PropertyOrField other
        )
        {
            return master.IsMatchFor(other, false);
        }

        public static bool IsMatchFor(
            this PropertyOrField master, 
            PropertyOrField other, 
            bool mustMatchMemberType
        )
        {
            return master.Name == other.Name &&
                   master.Type == other.Type &&
                   master.AccessMatches(other) &&
                   (!mustMatchMemberType || master.MemberType == other.MemberType);
        }

        private static bool AccessMatches(
            this PropertyOrField master,
            PropertyOrField other
        )
        {
            return master.CanRead == other.CanRead &&
                    master.CanWrite == other.CanWrite;
        }

        public static string PrettyPrint(this PropertyOrField item)
        {
            return $"{item.Name} ({item.Type}) [{item.PrintAccess()}]";
        }

        private static readonly Tuple<bool, bool, string>[] _accessNames =
        {
            Tuple.Create(true, true, "read-write"),
            Tuple.Create(true, false, "read-only"),
            Tuple.Create(false, true, "write-only")
        };

        public static string PrintAccess(this PropertyOrField item)
        {
            return _accessNames
                .First(t => t.Item1 == item.CanRead && t.Item2 == item.CanWrite)
                .Item3;
        }
    }
}