using System;

namespace PeanutButter.Reflection
{
    public interface ITypeEnumeratorFilterCriteria : IMemberEnumeratorFilterCriteria<ITypeEnumeratorFilter>
    {
        ITypeEnumeratorFilter AttributedWith<TAttributeType>(bool inherit = false) where TAttributeType : Attribute;
        ITypeEnumeratorFilter AttributedWith(Type attributeType, bool inherit = false);

        ITypeEnumeratorFilter AssignableFrom(Type castType);
        ITypeEnumeratorFilter AssignableAs(Type castType);

        ITypeEnumeratorFilter DescendantsOf(Type ancestorType);
        ITypeEnumeratorFilter AncestorsOf(Type descendantType);
        ITypeEnumeratorFilter ChildrenOf(Type parentType);

        ITypeEnumeratorFilter GenericParametersPresent();
        ITypeEnumeratorFilter GenericTypeDefinitions();
        ITypeEnumeratorFilter GenericTypes();
        ITypeEnumeratorFilter ConstructedGenericTypes();
        ITypeEnumeratorFilter ConstructedGenericTypes(Type genericTypeDefinition);

        ITypeEnumeratorFilter Static();
        ITypeEnumeratorFilter Abstract();
        ITypeEnumeratorFilter Sealed();
        ITypeEnumeratorFilter Nested();
        ITypeEnumeratorFilter Nested(Type declaringType);

        ITypeEnumeratorFilter Interfaces();
        ITypeEnumeratorFilter Enumerations();
        ITypeEnumeratorFilter Attributes();

        ITypeEnumeratorFilter Structures();
        ITypeEnumeratorFilter Classes();

        ITypeEnumeratorFilter ValueTypes();
        ITypeEnumeratorFilter ReferenceTypes();

        ITypeEnumeratorFilter ConstructedWithNoParameters(MemberAccessibility accessibility);
        ITypeEnumeratorFilter ConstructedWithParameters(MemberAccessibility accessibility, params Type[] parameterTypes);
    }
}
