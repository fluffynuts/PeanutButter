Imports NUnit.Framework
Imports PeanutButter.RandomGenerators

Namespace TestStatementBuilders

    <TestFixture()>
    Public Class TestOrderBy
        <TestCase(OrderBy.Directions.Ascending)>
        <TestCase(OrderBy.Directions.Descending)>
        Public Sub ToString_GivenFieldAndDescendingDirection_ProducesExpectedString(direction As OrderBy.Directions)
            Dim fld = RandomValueGen.GetRandomString()
            Dim o = New OrderBy(fld, direction)
            If (direction = OrderBy.Directions.Descending) Then
                Assert.AreEqual("order by [" + fld + "] desc", o.ToString())
            Else
                Assert.AreEqual("order by [" + fld + "] asc", o.ToString())
            End If
        End Sub

        <TestCase(OrderBy.Directions.Ascending)>
        <TestCase(OrderBy.Directions.Descending)>
        Public Sub ToString_GivenFieldAndQualifyingTableAndDirection_ProducesExpectedString(direction As OrderBy.Directions)
            Dim fld = RandomValueGen.GetRandomString()
            Dim table = RandomValueGen.GetRandomString()
            Dim o = New OrderBy(table, fld, direction)
            If (direction = OrderBy.Directions.Descending) Then
                Assert.AreEqual("order by [" + table + "].[" + fld + "] desc", o.ToString())
            Else
                Assert.AreEqual("order by [" + table + "].[" + fld + "] asc", o.ToString())
            End If

        End Sub

    End Class
End NameSpace