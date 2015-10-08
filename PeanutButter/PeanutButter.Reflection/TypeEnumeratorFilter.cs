using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using PeanutButter.Reflection.Extensions;

namespace PeanutButter.Reflection
{
    internal sealed class TypeEnumeratorFilter : ITypeEnumeratorFilter, ITypeEnumeratorFilterCriteria
    {
        private static bool IsStatic(Type examineType)
        {
            return examineType.IsClass && examineType.IsAbstract && examineType.IsSealed;
        }

        private static bool IsAbstract(Type examineType)
        {
            return !examineType.IsValueType && examineType.IsAbstract && !examineType.IsSealed;
        }

        private static bool IsSealed(Type examineType)
        {
            return examineType.IsSealed && !examineType.IsAbstract;
        }

        private static bool IsNested(Type examineType, Type declaringType)
        {
            return examineType.IsNested && examineType.DeclaringType == declaringType;
        }

        private static bool IsNested(Type examineType)
        {
            return examineType.IsNested;
        }

        private static bool IsInterfaces(Type examineType)
        {
            return examineType.IsInterface;
        }

        private static bool IsEnumerations(Type examineType)
        {
            return examineType.IsEnum;
        }

        private static bool IsAttributes(Type examineType)
        {
            return examineType == typeof(Attribute) || DescendantsOf(examineType, typeof(Attribute));
        }

        private static bool IsStructures(Type examineType)
        {
            return examineType.IsValueType && !examineType.IsEnum;
        }

        private static bool IsClass(Type examineType)
        {
            return examineType.IsClass;
        }

        private static bool IsValueTypes(Type examineType)
        {
            return examineType.IsValueType;
        }

        private static bool IsReferenceTypes(Type examineType)
        {
            return !IsValueTypes(examineType);
        }

        private static bool ChildrenOf(Type examineType, Type parentType)
        {
            return examineType.BaseType == parentType;
        }

        private static bool DescendantsOf(Type examineType, Type ancestorType)
        {
            if (IsValueTypes(examineType)) return false;
            if (examineType == ancestorType) return false;
            Type currentType = examineType;

            while ((currentType = currentType.BaseType) != null)
            {
                if (currentType == ancestorType)
                    return true;
            }

            return false;
        }

        private static bool AncestorsOf(Type examineType, Type descendantType)
        {
            return DescendantsOf(descendantType, examineType);
        }

        private static bool ConstructedWithNoParameters(Type examineType, MemberAccessibility accessibility)
        {
            ConstructorInfo[] constructors = accessibility == MemberAccessibility.Public ? examineType.GetConstructors() : examineType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return constructors.Where(constructor => constructor.GetAccessibility() >= accessibility).Any(constructor => constructor.GetParameters().Length == 0);
        }

        private static bool ConstructedWithParameters(Type examineType, MemberAccessibility accessibility, Type[] parameterTypes)
        {
            ConstructorInfo[] constructors = accessibility == MemberAccessibility.Public ? examineType.GetConstructors() : examineType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return constructors.Where(constructor => constructor.GetAccessibility() >= accessibility).Any(constructor => ParametersAreCompatible(constructor.GetParameters(), parameterTypes));
        }

        private static bool ParametersAreCompatible(ParameterInfo[] parameters, Type[] desiredParameterTypes)
        {
            if (parameters.Length != desiredParameterTypes.Length) return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo currentParameter = parameters[i];
                Type desiredType = desiredParameterTypes[i];
                if (!currentParameter.ParameterType.IsAssignableFrom(desiredType)) return false;
            }

            return true;
        }

        private static bool ExactlyAsVisible(Type examineType, MemberAccessibility accessibility)
        {
            return examineType.GetAccessibility() == accessibility;
        }

        private static bool GreaterOrEqualVisible(Type examineType, MemberAccessibility accessibility)
        {
            return examineType.GetAccessibility() >= accessibility;
        }

        private static bool LessOrEqualVisible(Type examineType, MemberAccessibility accessibility)
        {
            return examineType.GetAccessibility() <= accessibility;
        }

        private static bool MoreVisible(Type examineType, MemberAccessibility accessibility)
        {
            return examineType.GetAccessibility() > accessibility;
        }

        private static bool LessVisible(Type examineType, MemberAccessibility accessibility)
        {
            return examineType.GetAccessibility() < accessibility;
        }

        private static bool IsDefined(Type examineType, Type attributeType, bool inherit)
        {
            if (IsValueTypes(examineType)) return false;
            IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(examineType);

            foreach (CustomAttributeData attribute in customAttributes)
            {
                if (attribute.AttributeType == attributeType)
                    return true;

                if (inherit && DescendantsOf(attributeType, attribute.AttributeType))
                    return true;
            }

            return false;
        }

        private readonly Type[] _types;
        private readonly bool _reflectionOnly;
        private FilterMode _mode;

        private List<KeyValuePair<bool, Predicate<Type>>> _operations;

        public ITypeEnumeratorFilterCriteria AndAre { get { _mode = FilterMode.AndTrue; return this; } }
        public ITypeEnumeratorFilterCriteria AndNot { get { _mode = FilterMode.AndFalse; return this; } }

        public TypeEnumeratorFilter(Type[] types, bool reflectionOnly)
        {
            _types = types;
            _reflectionOnly = reflectionOnly;
        }

        public IEnumerator<Type> GetEnumerator()
        {
            return _operations == null ? _types.AsEnumerable().GetEnumerator() : _types.Where(OnApplyFilter).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool OnApplyFilter(Type type)
        {
            for (int i = 0; i < _operations.Count; i++)
            {
                KeyValuePair<bool, Predicate<Type>> kvp = _operations[i];
                if (kvp.Value(type) != kvp.Key)
                    return false;
            }

            return true;
        }

        // ReSharper disable NotResolvedInText
        private void AddOperation(Predicate<Type> operation)
        {
            if (_operations == null)
                _operations = new List<KeyValuePair<bool, Predicate<Type>>>();

            bool comparand;

            switch (_mode)
            {
                case FilterMode.AndTrue: comparand = true; break;
                case FilterMode.AndFalse: comparand = false; break;
                default: throw new InvalidEnumArgumentException("_mode", (int)_mode, typeof(FilterMode));
            }

            _operations.Add(new KeyValuePair<bool, Predicate<Type>>(comparand, operation));
        }
        // ReSharper restore NotResolvedInText

        private ITypeEnumeratorFilter AttributedWith(Type attributeType, bool inherit)
        {
            Predicate<Type> operation;

            if (_reflectionOnly)
                operation = examineType => IsDefined(examineType, attributeType, inherit);
            else
                operation = examineType => examineType.IsDefined(attributeType, inherit);

            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.AttributedWith<TAttributeType>(bool inherit)
        {
            return AttributedWith(typeof(TAttributeType), inherit);
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.AttributedWith(Type attributeType, bool inherit)
        {
            return AttributedWith(attributeType, inherit);
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.AssignableFrom(Type castType)
        {
            Predicate<Type> operation = examineType => examineType.IsAssignableFrom(castType);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.AssignableAs(Type castType)
        {
            Predicate<Type> operation = castType.IsAssignableFrom;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.DescendantsOf(Type ancestorType)
        {
            Predicate<Type> operation = examineType => DescendantsOf(examineType, ancestorType);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.AncestorsOf(Type descendantType)
        {
            Predicate<Type> operation = examineType => AncestorsOf(examineType, descendantType);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.ChildrenOf(Type parentType)
        {
            Predicate<Type> operation = examineType => ChildrenOf(examineType, parentType);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.Static()
        {
            Predicate<Type> operation = IsStatic;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.Abstract()
        {
            Predicate<Type> operation = IsAbstract;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.Sealed()
        {
            Predicate<Type> operation = IsSealed;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.Nested()
        {
            Predicate<Type> operation = IsNested;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.Nested(Type declaringType)
        {
            Predicate<Type> operation = examineType => IsNested(examineType, declaringType);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.Interfaces()
        {
            Predicate<Type> operation = IsInterfaces;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.Enumerations()
        {
            Predicate<Type> operation = IsEnumerations;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.Attributes()
        {
            Predicate<Type> operation = IsAttributes;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.Structures()
        {
            Predicate<Type> operation = IsStructures;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.Classes()
        {
            Predicate<Type> operation = IsClass;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.ValueTypes()
        {
            Predicate<Type> operation = IsValueTypes;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.ReferenceTypes()
        {
            Predicate<Type> operation = IsReferenceTypes;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.ConstructedWithNoParameters(MemberAccessibility accessibility)
        {
            Predicate<Type> operation = examineType => ConstructedWithNoParameters(examineType, accessibility);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.ConstructedWithParameters(MemberAccessibility accessibility, params Type[] parameterTypes)
        {
            Predicate<Type> operation = examineType => ConstructedWithParameters(examineType, accessibility, parameterTypes);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter IMemberEnumeratorFilterCriteria<ITypeEnumeratorFilter>.PublicAccessibility()
        {
            Predicate<Type> operation = examineType => ExactlyAsVisible(examineType, MemberAccessibility.Public);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter IMemberEnumeratorFilterCriteria<ITypeEnumeratorFilter>.ProtectedInternalAccessibility()
        {
            Predicate<Type> operation = examineType => ExactlyAsVisible(examineType, MemberAccessibility.ProtectedInternal);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter IMemberEnumeratorFilterCriteria<ITypeEnumeratorFilter>.InternalAccessibility()
        {
            Predicate<Type> operation = examineType => ExactlyAsVisible(examineType, MemberAccessibility.Internal);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter IMemberEnumeratorFilterCriteria<ITypeEnumeratorFilter>.ProtectedAccessibility()
        {
            Predicate<Type> operation = examineType => ExactlyAsVisible(examineType, MemberAccessibility.Protected);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter IMemberEnumeratorFilterCriteria<ITypeEnumeratorFilter>.PrivateAccessibility()
        {
            Predicate<Type> operation = examineType => ExactlyAsVisible(examineType, MemberAccessibility.Private);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter IMemberEnumeratorFilterCriteria<ITypeEnumeratorFilter>.MoreAccessibleThan(MemberAccessibility accessibility)
        {
            Predicate<Type> operation = examineType => MoreVisible(examineType, accessibility);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter IMemberEnumeratorFilterCriteria<ITypeEnumeratorFilter>.LessAccessibleThan(MemberAccessibility accessibility)
        {
            Predicate<Type> operation = examineType => LessVisible(examineType, accessibility);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter IMemberEnumeratorFilterCriteria<ITypeEnumeratorFilter>.AtMostAsAccessibleAs(MemberAccessibility accessibility)
        {
            Predicate<Type> operation = examineType => LessOrEqualVisible(examineType, accessibility);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter IMemberEnumeratorFilterCriteria<ITypeEnumeratorFilter>.AtLeastAsAccessibleAs(MemberAccessibility accessibility)
        {
            Predicate<Type> operation = examineType => GreaterOrEqualVisible(examineType, accessibility);
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.GenericParametersPresent()
        {
            Predicate<Type> operation = examineType => examineType.ContainsGenericParameters;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.GenericTypeDefinitions()
        {
            Predicate<Type> operation = examineType => examineType.IsGenericTypeDefinition;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.GenericTypes()
        {
            Predicate<Type> operation = examineType => examineType.IsGenericType;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.ConstructedGenericTypes(Type genericTypeDefinition)
        {
            Predicate<Type> operation = examineType => examineType.IsConstructedGenericType && examineType.GetGenericTypeDefinition() == genericTypeDefinition;
            AddOperation(operation);
            return this;
        }

        ITypeEnumeratorFilter ITypeEnumeratorFilterCriteria.ConstructedGenericTypes()
        {
            Predicate<Type> operation = examineType => examineType.IsConstructedGenericType;
            AddOperation(operation);
            return this;
        }

        private enum FilterMode
        {
            AndTrue,
            AndFalse
        }
    }
}
