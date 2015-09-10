namespace PeanutButter.Reflection
{
    public interface IMemberEnumeratorFilterCriteria<TFilterReturnType>
    {
        TFilterReturnType PublicAccessibility();
        TFilterReturnType ProtectedInternalAccessibility();
        TFilterReturnType InternalAccessibility();
        TFilterReturnType ProtectedAccessibility();
        TFilterReturnType PrivateAccessibility();

        TFilterReturnType MoreAccessibleThan(MemberAccessibility accessibility);
        TFilterReturnType LessAccessibleThan(MemberAccessibility accessibility);
        TFilterReturnType AtMostAsAccessibleAs(MemberAccessibility accessibility);
        TFilterReturnType AtLeastAsAccessibleAs(MemberAccessibility accessibility);
    }
}
