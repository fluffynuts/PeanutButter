using System.Reflection;

namespace PeanutButter.Reflection.Extensions
{
    public static class EventInfoExtensions
    {
        public static MemberAccessibility GetAccessibility(this EventInfo @event)
        {
            MemberAccessibility accessibility = 0;
            MethodInfo method = @event.GetAddMethod(true);

            if (method != null) accessibility = method.GetAccessibility();

            method = @event.GetRemoveMethod(true);

            if (method != null)
            {
                MemberAccessibility removeAccessibility = method.GetAccessibility();
                if (accessibility == 0 || removeAccessibility < accessibility) accessibility = removeAccessibility;
            }

            return accessibility;
        }
    }
}
