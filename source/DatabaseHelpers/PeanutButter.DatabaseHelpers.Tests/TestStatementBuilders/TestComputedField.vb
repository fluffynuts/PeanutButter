Imports NUnit.Framework
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
            Dim _alias = RandomValueGen.GetRandomString(2)
            Dim sut = new ComputedField(fieldName, fn, _alias)
            Dim result = sut.ToString()
            Assert.AreEqual(str & "([" & fieldName & "]) as [" & _alias & "]", result)
        End Sub
    End Class
End NameSpace