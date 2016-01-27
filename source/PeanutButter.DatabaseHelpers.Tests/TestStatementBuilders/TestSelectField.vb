Imports NUnit.Framework
Imports PeanutButter.RandomGenerators

Namespace TestStatementBuilders

    <TestFixture()>
    Public Class TestSelectField
        <Test()>
        Public Sub Constructor_GivenFieldNameOnly_ToStringIsExpected()
            Dim fld = RandomValueGen.GetRandomString()
            Dim sf = New SelectField(fld)
            Assert.AreEqual("[" + fld + "]", sf.ToString())
        End Sub

        <Test()>
        Public Sub Constructor_GivenFieldAndTable_ToStringIsExpected()
            Dim table = RandomValueGen.GetRandomString(),
                field = RandomValueGen.GetRandomString()
            Dim selectField = New SelectField(table, field)
            Assert.AreEqual("[" + table + "].[" + field + "]", selectField.ToString())
        End Sub

        <Test()>
        Public Sub SetAlias_GivenAlias_ShouldRenderWithAlias()
            Dim field = RandomValueGen.GetRandomString(2),
                a = RandomValueGen.GetRandomString(2)
            Dim sut = New SelectField(field)
            sut.SetAlias(a)
            dim expected = "[" + field + "] as [" + a + "]"
            Assert.AreEqual(expected, sut.ToString())
        End Sub

        <Test()>
        Public Sub SetAlias_GivenNothing_ShouldMakeToStringRenderWithoutAnAlias()
            Dim field = RandomValueGen.GetRandomString(2)
            Dim sut = New SelectField(field)
            sut.SetAlias(Nothing)
            dim expected = "[" + field + "]"
            Assert.AreEqual(expected, sut.ToString())
        End Sub

        <Test()>
        Public Sub WhenFieldIsStar_ShouldNotQuote()
            Dim sut = new SelectField("*")
            Dim expected = "*"
            Assert.AreEqual(expected, sut.ToString())
        End Sub

        <Test()>
        Public Sub WhenFieldIsStarWithTable_ShouldNotQuoteField()
            Dim table = RandomValueGen.GetRandomString(2)
            Dim sut = new SelectField(table, "*")
            Dim expected = "[" + table + "].*"
            Assert.AreEqual(expected, sut.ToString())
        End Sub
    End Class
End NameSpace