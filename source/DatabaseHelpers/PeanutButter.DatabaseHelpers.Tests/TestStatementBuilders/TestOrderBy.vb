Imports NUnit.Framework
Imports PeanutButter.DatabaseHelpers.StatementBuilders
Imports PeanutButter.RandomGenerators
Imports NExpect
Imports NExpect.Expectations

Namespace TestStatementBuilders

    <TestFixture()>
    Public Class TestOrderBy
        <TestCase(OrderBy.Directions.Ascending)>
        <TestCase(OrderBy.Directions.Descending)>
        Public Sub ToString_GivenFieldAndDescendingDirection_ProducesExpectedString(direction As OrderBy.Directions)
            Dim fld = RandomValueGen.GetRandomString()
            Dim o = New OrderBy(fld, direction)
            If (direction = OrderBy.Directions.Descending) Then
                Expect(o.ToString()) _
                .To.Equal("order by [" + fld + "] desc")
            Else
                Expect(o.ToString()) _
                    .To.Equal("order by [" + fld + "] asc")
            End If
        End Sub

        <TestCase(OrderBy.Directions.Ascending)>
        <TestCase(OrderBy.Directions.Descending)>
        Public Sub ToString_GivenFieldAndQualifyingTableAndDirection_ProducesExpectedString(direction As OrderBy.Directions)
            Dim fld = RandomValueGen.GetRandomString()
            Dim table = RandomValueGen.GetRandomString()
            Dim o = New OrderBy(table, fld, direction)
            If (direction = OrderBy.Directions.Descending) Then
                Expect(o.ToString()) _
                    .To.Equal("order by [" + table + "].[" + fld + "] desc")
            Else
                Expect(o.ToString()) _
                    .To.Equal("order by [" + table + "].[" + fld + "] asc")
            End If

        End Sub

    End Class
End NameSpace