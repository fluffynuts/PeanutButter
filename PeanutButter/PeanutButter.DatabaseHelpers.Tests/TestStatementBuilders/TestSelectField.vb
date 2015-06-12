Imports NUnit.Framework
Imports PeanutButter.RandomGenerators

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
End Class
