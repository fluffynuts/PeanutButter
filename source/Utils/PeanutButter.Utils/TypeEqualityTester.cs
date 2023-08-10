using System;
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
    /// Tests shape equality between two types (YMMV)
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class TypeEqualityTester
    {
        /// <summary>
        /// Provides a read-only collection of the errors encountered during the last comparison
        /// operation
        /// </summary>
        public IEnumerable<string> Errors => _errors.ToArray();


        /// <summary>
        /// Allow sub-matching: when True, the comparison type can contain more
        /// properties than the master as long as all master properties are matched
        /// </summary>
        public bool SubMatch { get; set; }

        /// <summary>
        /// Recognises properties on the comparison type to which the master
        /// property can be assigned as being equivalent
        /// </summary>
        public bool AllowAssignmentEquivalence { get; set; }

        private readonly List<string> _errors;
        private readonly Type _master;
        private readonly Type _compare;

        /// <summary>
        /// Creates a new TypeEqualityCTester with the two types to test
        /// </summary>
        /// <param name="master">Master type</param>
        /// <param name="compare">Comparison type</param>
        public TypeEqualityTester(Type master, Type compare) : this(master, compare, new List<string>())
        {
        }

        internal TypeEqualityTester(Type master, Type compare, List<string> errorCollection)
        {
            _master = master;
            _compare = compare;
            _errors = errorCollection;
        }

        /// <summary>
        /// Performs a new Deep Equivalence test.
        /// </summary>
        /// <returns></returns>
        public bool AreDeepEquivalent()
        {
            _errors.Clear();
            var masterMembers = GetMembers(_master);
            var compareMembers = GetMembers(_compare);
            return ExactCompare(masterMembers, compareMembers);
        }

        private bool ExactCompare(
            PropertyOrField[] masterMembers,
            PropertyOrField[] compareMembers
        )
        {
            if (!SubMatch && masterMembers.Length != compareMembers.Length)
            {
                // TODO: report on /what/ is missing / extra
                return AddError($"Property count mismatch: ${masterMembers.Length} vs ${compareMembers.Length}");
            }

            return masterMembers.Select(cur => new
                {
                    master = cur,
                    matched = Types.PrimitivesAndImmutables.Contains(cur.Type)
                        ? TryFindPrimitiveMatchFor(cur, compareMembers)
                        : TryFindComplexMatchFor(cur, compareMembers)
                })
                .All(o => o.matched);
        }

        private bool TryFindComplexMatchFor(
            PropertyOrField master,
            PropertyOrField[] compareMembers)
        {
            var nameMatch = compareMembers.FirstOrDefault(c => c.Name == master.Name);
            return nameMatch == null
                ? AddError($"No property found to match {master.Name} by name on {_compare}")
                : DeepCompare(master.Type, nameMatch.Type);
        }

        private bool DeepCompare(Type masterType, Type compareType)
        {
            var util = new TypeEqualityTester(masterType, compareType, _errors)
            {
                SubMatch = SubMatch
            };
            return util.AreDeepEquivalent();
        }

        private bool TryFindPrimitiveMatchFor(PropertyOrField cur, PropertyOrField[] compareMembers)
        {
            return compareMembers.ContainsOnlyOneMatching(
                    cur,
                    (o1, o2) => AllowAssignmentEquivalence
                        ? o1.IsAssignmentMatchFor(o2)
                        : o1.IsMatchFor(o2)
                ) ||
                AddError($"No match for: {cur.PrettyPrint()}");
        }

        private bool AddError(string s)
        {
            _errors.Add(s);
            return false;
        }

        private PropertyOrField[] GetMembers(Type t)
        {
            return t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(PropertyOrField.Create)
                .ToArray();
        }
    }
}