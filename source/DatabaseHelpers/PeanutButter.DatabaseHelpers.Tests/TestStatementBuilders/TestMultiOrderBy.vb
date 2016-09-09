Imports NUnit.Framework
Imports PeanutButter.RandomGenerators

Namespace TestStatementBuilders

    <TestFixture()>
    Public Class TestMultiOrderBy
        <Test()>
        Public Sub Construct_GivenOneField()
            Dim field = RandomValueGen.GetRandomString()
            Dim ob = New MultiOrderBy(OrderBy.Directions.Descending, field)
            Dim expected = "order by [" & field & "] desc"
            Assert.AreEqual(expected, ob.ToString())
        End Sub

        <Test()>
        Public Sub Construct_GivenTwoFields()
            Dim f1 = RandomValueGen.GetRandomString(),
                f2 = RandomValueGen.GetRandomString()
            Dim ob = New MultiOrderBy(OrderBy.Directions.Ascending, f1, f2)
            Dim expected = "order by [" & f1 & "] asc, [" & f2 & "] asc"
            Assert.AreEqual(expected, ob.ToString())
        End Sub
    End Class
End NameSpace