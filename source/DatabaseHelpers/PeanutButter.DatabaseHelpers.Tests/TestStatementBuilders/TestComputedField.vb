Imports NUnit.Framework
Imports PeanutButter.DatabaseHelpers.StatementBuilders
Imports PeanutButter.RandomGenerators

Namespace TestStatementBuilders

    <TestFixture()>
    Public Class TestComputedField
        <TestCase(ComputedField.ComputeFunctions.Max, "Max")>
        <TestCase(ComputedField.ComputeFunctions.Min, "Min")>
        <TestCase(ComputedField.ComputeFunctions.Coalesce, "Coalesce")>
        <TestCase(ComputedField.ComputeFunctions.Count, "Count")>
        Public Sub ToString_ShouldHaveCorrectFunction(fn As ComputedField.ComputeFunctions, str As String)
            Dim fieldName = RandomValueGen.GetRandomString(2)
            Dim theAlias = RandomValueGen.GetRandomString(2)
            Dim sut = new ComputedField(fieldName, fn, theAlias)
            Dim result = sut.ToString()
            Assert.AreEqual(str + "([" + fieldName + "]) as [" + theAlias + "]", result)
        End Sub
    End Class
End NameSpace