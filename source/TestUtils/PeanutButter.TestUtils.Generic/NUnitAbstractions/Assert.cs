namespace PeanutButter.TestUtils.Generic.NUnitAbstractions
{
    // provide nunit-like assertions
    internal static class Assert
    {
        internal static void IsTrue(bool value)
        {
            if (!value)
                Assertions.Throw("Expected True, but got False");
        }

        internal static void AreEqual(object left, object right, string customMessage = null)
        {
            if (left.Equals(right))
                return;
            Assertions.Throw(
                FinalMessageFor($"Expected {left} to equal {right}", customMessage)
            );
        }

        internal static void AreNotEqual(object left, object right, string message)
        {
            if (!left.Equals(right))
                return;
            Assertions.Throw(
                FinalMessageFor($"Expected {left} not to equal {right}", message)
            );
        }

        internal static void IsNotNull(object test, string message)
        {
            if (test != null)
                return;
            Assertions.Throw(
                // ReSharper disable once ExpressionIsAlwaysNull
                FinalMessageFor($"Expected {test} not to be null", message)
            );
        }

        internal static void IsNull(object test, string message) {
            if (test == null)
                return;
            Assertions.Throw(
                FinalMessageFor($"Expected {test} to be null", message)
            );
        }

        internal static void Fail(string message)
        {
            Assertions.Throw(message);
        }

        private static string FinalMessageFor(string defaultMessage, string customMessage)
        {
            return string.IsNullOrWhiteSpace(customMessage)
                ? defaultMessage
                : $"{customMessage}\n\n{defaultMessage}";
        }
    }
}