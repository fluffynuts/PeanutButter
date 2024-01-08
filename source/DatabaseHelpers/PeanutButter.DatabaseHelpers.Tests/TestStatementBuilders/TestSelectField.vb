Imports NUnit.Framework
Imports PeanutButter.DatabaseHelpers.StatementBuilders
Imports PeanutButter.RandomGenerators
Imports NExpect
Imports NExpect.Expectations

Namespace TestStatementBuilders

    <TestFixture()>
    Public Class TestSelectField
        <Test()>
        Public Sub Constructor_GivenFieldNameOnly_ToStringIsExpected()
            Dim fld = RandomValueGen.GetRandomString()
            Dim expected = "[" + fld + "]"
            Dim sut = New SelectField(fld)
            Expect(sut.ToString()).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Constructor_GivenFieldAndTable_ToStringIsExpected()
            Dim table = RandomValueGen.GetRandomString(),
                field = RandomValueGen.GetRandomString()
            Dim sut = New SelectField(table, field)
            Dim expected  = "[" + table + "].[" + field + "]"
            Expect(sut.ToString()).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub SetAlias_GivenAlias_ShouldRenderWithAlias()
            Dim field = RandomValueGen.GetRandomString(2),
                a = RandomValueGen.GetRandomString(2)
            Dim sut = New SelectField(field)
            sut.SetAlias(a)
            dim expected = "[" + field + "] as [" + a + "]"
            Expect(sut.ToString()).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub SetAlias_GivenNothing_ShouldMakeToStringRenderWithoutAnAlias()
            Dim field = RandomValueGen.GetRandomString(2)
            Dim sut = New SelectField(field)
            sut.SetAlias(Nothing)
            dim expected = "[" + field + "]"
            Expect(sut.ToString()).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WhenFieldIsStar_ShouldNotQuote()
            Dim sut = new SelectField("*")
            Dim expected = "*"
            Expect(sut.ToString()).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WhenFieldIsStarWithTable_ShouldNotQuoteField()
            Dim table = RandomValueGen.GetRandomString(2)
            Dim sut = new SelectField(table, "*")
            Dim expected = "[" + table + "].*"
            Expect(sut.ToString()).To.Equal(expected)
        End Sub
    End Class
End NameSpace